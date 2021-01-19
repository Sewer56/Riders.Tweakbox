using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Kernel32;
using Reloaded.Memory.Sources;
using Riders.Tweakbox.Components.Tweaks;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Graphics;
using Sewer56.NumberUtilities.Matrices;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.NumberUtilities.Vectors;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX.Direct3D9;
using static Sewer56.SonicRiders.API.Misc;
using static Vanara.PInvoke.User32_Gdi;

namespace Riders.Tweakbox.Controllers
{
    public unsafe class GraphicsController : IController
    {
        private static GraphicsController _controller;

        /// <summary>
        /// Pointer to the latest created DirectX 9 device.
        /// </summary>
        public IntPtr Dx9Device { get; private set; }

        /// <summary>
        /// Allows you to call the Direct3D Reset function.
        /// </summary>
        public DX9Hook.Reset Reset { get; private set; }

        /// <summary>
        /// Intercepts the Direct3D Reset function.
        /// </summary>
        public IHook<DX9Hook.Reset> ResetHook { get; private set; }

        /// <summary>
        /// Presentation parameters passed to Direct3D on last device reset.
        /// </summary>
        public PresentParameters LastPresentParameters;

        private TweaksEditorConfig _config = IoC.Get<TweaksEditorConfig>();

        // Hooks
        private IHook<DX9Hook.CreateDevice> _createDeviceHook;
        private IHook<Functions.RenderTexture2DFnPtr> _renderTexture2dHook;
        private IHook<Functions.RenderPlayerIndicatorFnPtr> _renderPlayerIndicatorHook;

        // Utilities
        private AspectConverter _aspectConverter = new AspectConverter(4 / 3f);
        private float _originalAspectRatio2dResX = *AspectRatio2dResolutionX;

        public GraphicsController()
        {
            _controller = this;
            _createDeviceHook  = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.Direct3D9VTable.CreateFunctionHook<DX9Hook.CreateDevice>((int)IDirect3D9.CreateDevice, CreateDeviceHook).Activate();

            Reset     = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.DeviceVTable.CreateWrapperFunction<DX9Hook.Reset>((int)IDirect3DDevice9.Reset);
            ResetHook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value.DeviceVTable.CreateFunctionHook<DX9Hook.Reset>((int)IDirect3DDevice9.Reset, ResetImpl);

            var renderTexture2dPtr = (delegate* unmanaged[Stdcall]<int, Vector3*, int, float, int>)&RenderTexture2DPtr;
            _renderTexture2dHook = Functions.RenderTexture2DPtr.Hook(new Functions.RenderTexture2DFnPtr() { Value = renderTexture2dPtr }).Activate();

            var renderPlayerIndicatorPtr = (delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, int>)&RenderPlayerIndicatorPtr;
            _renderPlayerIndicatorHook = Functions.RenderPlayerIndicatorPtr.Hook(new Functions.RenderPlayerIndicatorFnPtr() { Value = renderPlayerIndicatorPtr }).Activate();

            // Patch window style if borderless is set
            _config.ConfigUpdated += OnConfigUpdated;
            
        }

        /// <inheritdoc />
        public void Disable()
        {
            _createDeviceHook.Disable();
            _renderTexture2dHook.Disable();
            _renderPlayerIndicatorHook.Disable();
        }

        /// <inheritdoc />
        public void Enable()
        {
            _createDeviceHook.Enable();
            _renderTexture2dHook.Enable();
            _renderPlayerIndicatorHook.Enable();
        }

        private void OnConfigUpdated()
        {
            ref var style = ref Unsafe.AsRef<WindowStyles>((void*) 0x005119EC);
            if (_config.Data.Borderless)
                _config.RemoveBorder(ref style);
            else
                _config.AddBorder(ref style);
        }

        // Hook Implementation
        private IntPtr ResetImpl(IntPtr device, ref PresentParameters presentParameters)
        {
            Log.WriteLine($"Reset! Device: {device}, Local Device {Dx9Device}", LogCategory.Default);
            LastPresentParameters = presentParameters;
            return ResetHook.OriginalFunction(device, ref presentParameters);
        }

        private IntPtr CreateDeviceHook(IntPtr direct3dpointer, uint adapter, DeviceType deviceType, IntPtr hFocusWindow, CreateFlags behaviorFlags, ref PresentParameters presentParameters, int** ppReturnedDeviceInterface)
        {
            if (_config.Data.D3DDeviceFlags)
            {
                behaviorFlags &= ~CreateFlags.Multithreaded;
                behaviorFlags |= CreateFlags.DisablePsgpThreading;
            }

            if (!presentParameters.Windowed)
                Native.ShowCursor(true);

            // Disable VSync
            if (_config.Data.DisableVSync)
            {
                presentParameters.PresentationInterval = PresentInterval.Immediate;
                presentParameters.FullScreenRefreshRateInHz = 0;
            }

            LastPresentParameters = presentParameters;
            var result = _createDeviceHook.OriginalFunction(direct3dpointer, adapter, deviceType, hFocusWindow, behaviorFlags, ref presentParameters, ppReturnedDeviceInterface);
            Dx9Device = (IntPtr)(*ppReturnedDeviceInterface);
            return result;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int RenderPlayerIndicatorPtr(int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10) => _controller.RenderPlayerIndicator(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);

        private int RenderPlayerIndicator(int a1, int a2, int a3, int a4, int horizontalOffset, int a6, int a7, int a8, int a9, int a10)
        {
            if (_config.Data.WidescreenHack)
            {
                var actualAspect = *ResolutionX / (float)*ResolutionY;
                var relativeAspect = (AspectConverter.GetRelativeAspect(actualAspect));

                // Get new screen width.
                var maximumX = AspectConverter.GameCanvasWidth * relativeAspect;
                var borderLeft = (_aspectConverter.GetBorderWidthX(actualAspect, AspectConverter.GameCanvasHeight) / 2);

                // Scale to new size of screen and offset (our RenderTexture2D Hook will re-add this offset!) 
                horizontalOffset = (int)(((horizontalOffset / AspectConverter.GameCanvasWidth) * maximumX) - borderLeft);
            }

            return _renderPlayerIndicatorHook.OriginalFunction.Value.Invoke(a1, a2, a3, a4, horizontalOffset, a6, a7, a8, a9, a10);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int RenderTexture2DPtr(int isQuad, Vector3* vertices, int numVertices, float opacity) => _controller.RenderTexture2D(isQuad, vertices, numVertices, opacity);

        private int RenderTexture2D(int isQuad, Vector3* vertices, int numVertices, float opacity)
        {
            float Project(float original, float leftBorderOffset) => (leftBorderOffset + original);

            if (_config.Data.WidescreenHack)
            {
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
            }
            else
            {
                *AspectRatio2dResolutionX = _originalAspectRatio2dResX;
            }

            return _renderTexture2dHook.OriginalFunction.Value.Invoke(isQuad, vertices, numVertices, opacity);
        }
    }
}
