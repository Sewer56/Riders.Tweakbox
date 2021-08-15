using Riders.Tweakbox.Configs.Json;
namespace Riders.Tweakbox.Configs;

public class TextureInjectionConfig : JsonConfigBase<TextureInjectionConfig, TextureInjectionConfig.Internal>
{
    public class Internal
    {
        public bool DumpTextures = false;
        public bool LoadTextures = false;

        public DumpingMode DumpingMode = DumpingMode.All;
        public int DeduplicationMaxFiles = 2;
    }

    public enum DumpingMode
    {
        All = 0,
        OnlyNew = 1,
        Deduplicate = 2,
    }
}
