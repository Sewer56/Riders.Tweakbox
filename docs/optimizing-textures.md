# Optimizing Textures

These are just very basic guidelines towards optimizing textures for efficiency, such that they can load fast, run fast and use little memory.

The optimizations here can also be manually applied by my [texture optimisation tool](https://github.com/Sewer56/DolphinImageOptimizer).

My general advice is to make very high resolution textures and then simply scale them down to multiple target resolutions using an automated tool like mine.

## Optimizing for Size
It is recommended that you scale your texture resolutions using powers of 2 compared to the original game.

The original game was optimized around displaying on a 480p screen, as such, if you are targeting a 960p screen and your texture was `128x128`, your texture should be `256x256`.

**As a general rule of thumb:**  
- For up to 720p target 2x original Resolution (960p)  
- For up to 1440p target 4x original Resolution (1920p)  
- For up to 5K target 8x original Resolution (3840p)  

Ideally, you should provide separate downloads for the first two options above.

It is also important you maintain aspect ratio of the original textures and use powers of 2 for width and height.

## Texture Compression

While Tweakbox supports PNGs, it is recommended that you only use them for testing as they are slow to load and use a lot of memory. 

You should use instead use DDS files with native texture compression formats.
Unfortunately, as Riders runs on DirectX 9, support for efficient, high quality texture formats is limited. 

The best candidates, DXT1 & DXT5 can be a tiny bit blocky and produce color banding where there are sporadic changes in colour (e.g. rainbow gradient). Uncompressed textures on the other hand (PNG, R8G8B8A8) are very memory inefficient.

### Recommendation:
- UI elements: Use uncompressed textures (DDS w/ R8G8B8A8).
- Stage elements: Use DXT1 (no transparency) & DXT5 (transparency) where applicable.

Uncompressed textures are very large, so consider using [my optimisation tool](https://github.com/Sewer56/DolphinImageOptimizer) with the custom DDS.LZ4 format supported by Tweakbox.

## Memory Usage
- Doubling your resolution (256x256 -> 512x512) increases memory usage by 4 times.

### Texture Formats
- R8G8B8A8 is uncompressed and uses 4 bytes per pixel.
- DXT5 uses 1 byte per pixel.   (75% reduction)
- DXT1 uses 0.5 byte per pixel. (87.5% reduction)

PNGs are automatically decoded to R8G8B8A8, and thus use 4 bytes.