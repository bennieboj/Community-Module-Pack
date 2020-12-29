﻿using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Compass_Module {

    [Export(typeof(Module))]
    public class CompassModule : Module {

        internal static CompassModule ModuleInstance;

        // Service Managers
        internal SettingsManager    SettingsManager    => this.ModuleParameters.SettingsManager;
        internal ContentsManager    ContentsManager    => this.ModuleParameters.ContentsManager;

        private SettingEntry<float> _settingCompassSize;
        private SettingEntry<float> _settingCompassRadius;
        private SettingEntry<float> _settingVerticalOffset;

        private Blish_HUD.Entities.Primitives.Billboard _northBb;
        private Blish_HUD.Entities.Primitives.Billboard _eastBb;
        private Blish_HUD.Entities.Primitives.Billboard _southBb;
        private Blish_HUD.Entities.Primitives.Billboard _westBb;

        /// <summary>
        /// Ideally you should keep the constructor as is.
        /// Use "Initialize()" to handle initializing the module.
        /// </summary>
        [ImportingConstructor]
        public CompassModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            ModuleInstance = this;
        }

        protected override void Initialize() {
            _northBb = new Blish_HUD.Entities.Primitives.Billboard(ContentsManager.GetTexture("north.png"));
            _eastBb  = new Blish_HUD.Entities.Primitives.Billboard(ContentsManager.GetTexture("east.png"));
            _southBb = new Blish_HUD.Entities.Primitives.Billboard(ContentsManager.GetTexture("south.png"));
            _westBb  = new Blish_HUD.Entities.Primitives.Billboard(ContentsManager.GetTexture("west.png"));

            UpdateBillboardSize(null);

            GameService.Graphics.World.Entities.Add(_northBb);
            GameService.Graphics.World.Entities.Add(_eastBb);
            GameService.Graphics.World.Entities.Add(_southBb);
            GameService.Graphics.World.Entities.Add(_westBb);
        }

        private void UpdateBillboardSize(object sender = null, ValueChangedEventArgs<float> e = null) {
            var newSize = new Vector2(_settingCompassSize.Value);

            _northBb.Size = newSize;
            _eastBb.Size  = newSize;
            _southBb.Size = newSize;
            _westBb.Size  = newSize;
        }

        private const float VERTICALOFFSET_MIDDLE = 2.5f;

        protected override void DefineSettings(SettingCollection settings) {
            _settingCompassSize    = settings.DefineSetting("CompassSize",    0.5f, "Compass Size",    "Size of the compass elements.");
            _settingCompassRadius  = settings.DefineSetting("CompassRadius",  0f,   "Compass Radius",  "Radius of the compass.");
            _settingVerticalOffset = settings.DefineSetting("VerticalOffset", VERTICALOFFSET_MIDDLE,   "Vertical Offset", "How high to offset the compass off the ground.");

            _settingCompassSize.SetRange(0.1f, 2f);
            _settingCompassRadius.SetRange(0f, 4f);
            _settingVerticalOffset.SetRange(0f, VERTICALOFFSET_MIDDLE * 2);

            _settingCompassSize.SettingChanged += UpdateBillboardSize;
        }

        protected override Task LoadAsync() {
            return Task.CompletedTask;
        }

        protected override void Update(GameTime gameTime) {
            UpdateBillboardPosition();
            UpdateBillboardOpacity();
        }

        private void UpdateBillboardPosition() {
            _northBb.Position = GameService.Gw2Mumble.PlayerCharacter.Position + new Vector3(0, 1 + _settingCompassRadius.Value, _settingVerticalOffset.Value    - VERTICALOFFSET_MIDDLE);
            _eastBb.Position  = GameService.Gw2Mumble.PlayerCharacter.Position + new Vector3(1    + _settingCompassRadius.Value, 0, _settingVerticalOffset.Value - VERTICALOFFSET_MIDDLE);
            _southBb.Position = GameService.Gw2Mumble.PlayerCharacter.Position + new Vector3(0,                                  -1                              - _settingCompassRadius.Value, _settingVerticalOffset.Value    - VERTICALOFFSET_MIDDLE);
            _westBb.Position  = GameService.Gw2Mumble.PlayerCharacter.Position + new Vector3(-1                                                                  - _settingCompassRadius.Value, 0, _settingVerticalOffset.Value - VERTICALOFFSET_MIDDLE);
        }

        private void UpdateBillboardOpacity() {
            _northBb.Opacity = Math.Min(1 - GameService.Gw2Mumble.PlayerCamera.Forward.Y, 1f);
            _eastBb.Opacity  = Math.Min(1 - GameService.Gw2Mumble.PlayerCamera.Forward.X, 1f);
            _southBb.Opacity = Math.Min(1 + GameService.Gw2Mumble.PlayerCamera.Forward.Y, 1f);
            _westBb.Opacity  = Math.Min(1 + GameService.Gw2Mumble.PlayerCamera.Forward.X, 1f);
        }

        /// <inheritdoc />
        protected override void Unload() {
            ModuleInstance = null;

            GameService.Graphics.World.Entities.Remove(_northBb);
            GameService.Graphics.World.Entities.Remove(_eastBb);
            GameService.Graphics.World.Entities.Remove(_southBb);
            GameService.Graphics.World.Entities.Remove(_westBb);
        }

    }

}
