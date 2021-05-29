namespace Riders.Tweakbox.Misc
{
    /// <summary>
    /// Stores a collection of <see cref="Patch"/>es.
    /// </summary>
    public struct PatchCollection
    {
        /// <summary>
        /// The individual patches.
        /// </summary>
        public Patch[] Patches { get; private set; }

        public PatchCollection(Patch[] patches, bool changePermission = false)
        {
            Patches = patches;
            
            if (changePermission)
                ChangePermission();
        }

        /// <summary>
        /// Changes permission for the memory regions covered by the patches.
        /// </summary>
        public unsafe void ChangePermission()
        {    
            foreach (var patch in Patches) 
                patch.ChangePermission();
        }

        /// <summary>
        /// Enables or disables the patches.
        /// </summary>
        /// <param name="enable">True to enable, false to disable.</param>
        public unsafe void Set(bool enable)
        {
            if (enable)
                Enable();
            else
                Disable();
        }

        /// <summary>
        /// Enables all Patches
        /// </summary>
        public unsafe void Enable()
        {
            for (var x = 0; x < Patches.Length; x++)
                Patches[x].Enable();
        }

        /// <summary>
        /// Disables all patches.
        /// </summary>
        public unsafe void Disable()
        {
            for (var x = 0; x < Patches.Length; x++)
                Patches[x].Disable();
        }
    }
}
