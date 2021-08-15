using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc.Extensions;
using Riders.Tweakbox.Misc.Graphics;
using Sewer56.NumberUtilities.Matrices;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.NumberUtilities.Vectors;
using Sewer56.SonicRiders.Functions;
using static Sewer56.SonicRiders.API.Misc;
using static Sewer56.SonicRiders.Functions.Functions;
namespace Riders.Tweakbox.Controllers;

public unsafe class CenteredWidescreenHackController : IController
{
    private static CenteredWidescreenHackController _controller;
    private TweakboxConfig _config;
    private AspectConverter _aspectConverter = new AspectConverter(4 / 3f);

    private IHook<RenderTexture2DFnPtr> _renderTexture2dHook;
    private IHook<RenderPlayerIndicatorFnPtr> _renderPlayerIndicatorHook;

    private float _originalAspectRatio2dResX = *AspectRatio2dResolutionX;

    public CenteredWidescreenHackController(TweakboxConfig config)
    {
        _config = config;

        _controller = this;
        _renderTexture2dHook = Functions.RenderTexture2D.HookAs<RenderTexture2DFnPtr>(typeof(CenteredWidescreenHackController), nameof(RenderTexture2DPtr)).Activate();
        _renderPlayerIndicatorHook = Functions.RenderPlayerIndicator.HookAs<RenderPlayerIndicatorFnPtr>(typeof(CenteredWidescreenHackController), nameof(RenderPlayerIndicatorPtr)).Activate();
        _config.Data.AddPropertyUpdatedHandler(PropertyUpdated);
    }

    private void PropertyUpdated(string propertyname)
    {
        if (propertyname == nameof(_config.Data.WidescreenHack))
        {
            _renderTexture2dHook.Toggle(_config.Data.WidescreenHack);
            _renderPlayerIndicatorHook.Toggle(_config.Data.WidescreenHack);

            // Enable/Disable Widescreen Hooks
            if (!_config.Data.WidescreenHack)
                *AspectRatio2dResolutionX = _originalAspectRatio2dResX;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int RenderPlayerIndicatorPtr(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10) => _controller.RenderPlayerIndicator(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);

    private int RenderPlayerIndicator(int a1, int a2, int a3, int a4, int horizontalOffset, int a6, int a7, int a8, int a9, int a10)
    {
        var actualAspect = *ResolutionX / (float)*ResolutionY;
        var relativeAspect = (AspectConverter.GetRelativeAspect(actualAspect));

        // Get new screen width.
        var maximumX = AspectConverter.GameCanvasWidth * relativeAspect;
        var borderLeft = (_aspectConverter.GetBorderWidthX(actualAspect, AspectConverter.GameCanvasHeight) / 2);

        // Scale to new size of screen and offset (our RenderTexture2D Hook will re-add this offset!) 
        horizontalOffset = (int)(((horizontalOffset / AspectConverter.GameCanvasWidth) * maximumX) - borderLeft);

        return _renderPlayerIndicatorHook.OriginalFunction.Value.Invoke(a1, a2, a3, a4, horizontalOffset, a6, a7, a8, a9, a10);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int RenderTexture2DPtr(int isQuad, Vector3* vertices, int numVertices, float opacity) => _controller.RenderTexture2D(isQuad, vertices, numVertices, opacity);

    private int RenderTexture2D(int isQuad, Vector3* vertices, int numVertices, float opacity)
    {
        float Project(float original, float leftBorderOffset) => (leftBorderOffset + original);

        // Update horizontal aspect.
        var currentAspectRatio = (float)*ResolutionX / *ResolutionY;
        *AspectRatio2dResolutionX = AspectConverter.GameCanvasWidth * (currentAspectRatio / (AspectConverter.OriginalGameAspect));

        // Get offset to shift vertices by.
        var actualAspect = *ResolutionX / (float)*ResolutionY;
        var leftBorderOffset = (_aspectConverter.GetBorderWidthX(actualAspect, *ResolutionY) / 2);

        // Try hack drawn 2d elements
        // Reimplemented based on inspecting RenderHud2dTextureInternal (0x004419D0) in disassembly.
        var vertexIsVector3 = (int*)0x17E51F8;
        if (*vertexIsVector3 == 1)
        {
            if (numVertices >= 4)
            {
                int numMatrices = ((numVertices - 4) >> 2) + 1;
                var matrix = (Matrix4x3<float, Float>*)vertices;
                int totalMatVertices = numMatrices * 4;

                for (int x = 0; x < numMatrices; x++)
                {
                    matrix->X.X = Project(matrix->X.X, leftBorderOffset);
                    matrix->Y.X = Project(matrix->Y.X, leftBorderOffset);
                    matrix->Z.X = Project(matrix->Z.X, leftBorderOffset);
                    matrix->W.X = Project(matrix->W.X, leftBorderOffset);

                    matrix += 1; // Go to next matrix.
                }

                var extraVertices = numVertices - totalMatVertices;
                var vertex = (Vector5<float, Float>*)matrix;
                for (int x = 0; x < extraVertices; x++)
                {
                    vertex->X = Project(vertex->X, leftBorderOffset);
                    vertex += 1;
                }
            }
        }
        else
        {
            if (numVertices >= 4)
            {
                int numMatrices = ((numVertices - 4) >> 2) + 1;
                var matrix = (Matrix4x5<float, Float>*)vertices;
                int totalMatVertices = numMatrices * 4;

                /*
                    The format of this matrix is strange
                    X X X X
                    Y Y Y Y
                    ? ? ? ?
                    ? ? ? ?
                    ? ? ? ?
                */

                for (int x = 0; x < numMatrices; x++)
                {
                    matrix->X.X = Project(matrix->X.X, leftBorderOffset);
                    matrix->Y.X = Project(matrix->Y.X, leftBorderOffset);
                    matrix->Z.X = Project(matrix->Z.X, leftBorderOffset);
                    matrix->W.X = Project(matrix->W.X, leftBorderOffset);
                    matrix += 1; // Go to next matrix.
                }

                var extraVertices = numVertices - totalMatVertices;
                var vertex = (Vector5<float, Float>*)matrix;
                for (int x = 0; x < extraVertices; x++)
                {
                    vertex->X = Project(vertex->X, leftBorderOffset);
                    vertex += 1;
                }
            }
        }

        return _renderTexture2dHook.OriginalFunction.Value.Invoke(isQuad, vertices, numVertices, opacity);
    }
}
