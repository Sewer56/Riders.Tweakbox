using System;
using System.Collections.Generic;
using Riders.Tweakbox.Services.Interfaces;
using Riders.Tweakbox.Services.Texture.Animation;
using Riders.Tweakbox.Services.Texture.Structs;
namespace Riders.Tweakbox.Services.Texture;

public class AnimatedTextureService : ISingletonService
{
    private Dictionary<IntPtr, AnimatedTexture> _animatedTextures = new Dictionary<IntPtr, AnimatedTexture>();

    /// <summary>
    /// Starts tracking an <see cref="AnimatedTexture"/> returned from <see cref="TextureService.TryGetData"/>.
    /// </summary>
    /// <param name="firstTexReference">Native address of the first loaded texture corresponding to the <see cref="animated"/></param>
    /// <param name="animated">The animated texture from the <see cref="TextureInfo"/> struct returned by <see cref="TextureService.TryGetData"/></param>
    public unsafe void TrackAnimatedTexture(void* firstTexReference, Animation.AnimatedTexture animated)
    {
        if (animated != null)
        {
            animated.Preload(firstTexReference);
            _animatedTextures[(IntPtr)firstTexReference] = animated;
        }
    }

    /// <summary>
    /// Stops tracking an animated texture and releases all unmanaged resources.
    /// </summary>
    /// <param name="firstTexReference">Native address of the first loaded texture corresponding to the <see cref="AnimatedTexture"/></param>
    public unsafe void ReleaseAnimatedTexture(void* firstTexReference)
    {
        _animatedTextures.Remove((IntPtr)firstTexReference, out var value);
        value?.Dispose();
    }

    /// <summary>
    /// Tries to get a native pointer to the animated Texture.
    /// </summary>
    /// <param name="firstTexReference"></param>
    /// <param name="currentFrame"></param>
    /// <param name="newTexture"></param>
    /// <returns></returns>
    public unsafe bool TryGetAnimatedTexture(void* firstTexReference, int currentFrame, out void* newTexture)
    {
        newTexture = default;
        if (!_animatedTextures.TryGetValue((IntPtr)firstTexReference, out var animated))
            return false;

        newTexture = animated.GetTextureForFrame(currentFrame);
        return true;
    }
}
