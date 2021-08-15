namespace Riders.Tweakbox.Services.Texture.Headers;

public unsafe struct DdsHeader
{
    // Not in actual DDS header, just for convenience.
    public uint Magic;

    // Actual DDS header.
    public uint DwSize;
    public DdsFlags DwFlags;
    public uint DwHeight;
    public uint DwWidth;
    public uint DwPitchOrLinearSize;
    public uint DwDepth;
    public uint DwMipMapCount;
    public fixed uint DwReserved1[11];
    public DdsPixelFormat DdsPf;
    public uint DwCaps;
    public uint DwCaps2;
    public uint DwCaps3;
    public uint DwCaps4;
    public uint DwReserved2;
}
