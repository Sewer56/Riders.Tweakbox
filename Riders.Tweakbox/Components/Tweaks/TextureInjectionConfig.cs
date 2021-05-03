using Riders.Tweakbox.Components.Common;

namespace Riders.Tweakbox.Components.Tweaks
{
    public class TextureInjectionConfig : JsonConfigBase<TextureInjectionConfig, TextureInjectionConfig.Internal>
    {
        public class Internal
        {
            public bool DumpTextures = false;
            public bool LoadTextures = false;
        }
    }
}
