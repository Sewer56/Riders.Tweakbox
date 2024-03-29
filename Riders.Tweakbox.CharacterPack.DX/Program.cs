﻿using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Riders.Tweakbox.Interfaces;
using System;

namespace Riders.Tweakbox.CharacterPack.DX
{
    public class Program : IMod
    {
        /// <summary>
        /// Your mod if from ModConfig.json, used during initialization.
        /// </summary>
        private const string MyModId = "Riders.Tweakbox.CharacterPack.DX";

        /// <summary>
        /// Used for writing text to the console window.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private IModLoader _modLoader;

        private CustomCharacterPack _customCharacterPack;

        public void StartEx(IModLoaderV1 loader, IModConfigV1 config)
        {
            _modLoader = (IModLoader)loader;
            _logger = (ILogger)_modLoader.GetLogger();

            /* Your mod code starts here. */
            _modLoader.GetController<ITweakboxApi>().TryGetTarget(out var api);
            var modImpl = api.Register($"{config.ModId}.{config.ModVersion}");
            _customCharacterPack = new CustomCharacterPack(_modLoader.GetDirectoryForModId(MyModId), modImpl);
            _modLoader.OnModLoaderInitialized += AfterAllModsLoaded;
        }

        private void AfterAllModsLoaded() => _customCharacterPack.SetCharacterStats();

        /* Mod loader actions. */
        public void Suspend() { }

        public void Resume() { }

        public void Unload() { }

        /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
            If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
        */
        public bool CanUnload() => false;
        public bool CanSuspend() => false;

        /* Automatically called by the mod loader when the mod is about to be unloaded. */
        public Action Disposing { get; }

        /* This is a dummy for R2R (ReadyToRun) deployment.
           For more details see: https://github.com/Reloaded-Project/Reloaded-II/blob/master/Docs/ReadyToRun.md
        */
        public static void Main() { }
    }
}
