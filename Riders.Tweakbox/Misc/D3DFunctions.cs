using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Pointers;
using Sewer56.SonicRiders.Internal.DirectX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace Riders.Tweakbox.Misc
{
    public static class D3DFunctions
    {
        public static IFunction<D3DXCreateTextureFromFileInMemoryExPtr> CreateTexture;
        public static IFunction<SetTexturePtr> SetTexture;

        public static IFunction<ComReleasePtr> AddRefTexture;
        public static IFunction<ComReleasePtr> ReleaseTexture;

        static D3DFunctions()
        {
            var hooks = IoC.GetSingleton<IReloadedHooks>();
            var d3dx9Handle = PInvoke.LoadLibrary("d3dx9_25.dll");
            var dx9Hook = Sewer56.SonicRiders.API.Misc.DX9Hook.Value;
            CreateTexture  = hooks.CreateFunction<D3DXCreateTextureFromFileInMemoryExPtr>((long)Native.GetProcAddress(d3dx9Handle.DangerousGetHandle(), "D3DXCreateTextureFromFileInMemoryEx"));
            SetTexture     = hooks.CreateFunction<SetTexturePtr>((long) dx9Hook.DeviceVTable[(int)IDirect3DDevice9.SetTexture].FunctionPointer);
            AddRefTexture  = hooks.CreateFunction<ComReleasePtr>((long) dx9Hook.Texture9VTable[(int)IDirect3DTexture9.AddRef].FunctionPointer);
            ReleaseTexture = hooks.CreateFunction<ComReleasePtr>((long) dx9Hook.Texture9VTable[(int)IDirect3DTexture9.Release].FunctionPointer);
        }

        [FunctionHookOptions(PreferRelativeJump = true)]
        [Function(CallingConventions.Stdcall)]
        public struct D3DXCreateTextureFromFileInMemoryExPtr
        {
            public FuncPtr<BlittablePointer<byte>, BlittablePointer<byte>, int, int, int, int, Usage, Format, Pool, int, int,
                RawColorBGRA, BlittablePointer<byte>, BlittablePointer<PaletteEntry>, BlittablePointer<BlittablePointer<byte>>, int> Ptr;
        }

        [FunctionHookOptions(PreferRelativeJump = true)]
        [Function(CallingConventions.Stdcall)]
        public struct ComReleasePtr { public FuncPtr<IntPtr, IntPtr> Value; }

        [FunctionHookOptions(PreferRelativeJump = true)]
        [Function(CallingConventions.Stdcall)]
        public struct SetTexturePtr { public FuncPtr<IntPtr, int, BlittablePointer<Reloaded.Hooks.Definitions.Structs.Void>, IntPtr> Value; }
    }
}
