using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Utilities;
using System;
using System.Numerics;
using DearImguiSharp;

namespace Sewer56.Imgui.Controls
{
    /// <summary>
    /// A control used for rendering an image.
    /// </summary>
    public class ImageRenderer : IDisposable
    {
        private FinalizedImVec2 _imageSize      = new FinalizedImVec2() { X = 100, Y = 100 };
        private FinalizedImVec2 _imageUv0       = new FinalizedImVec2() { X = 0, Y = 0 };
        private FinalizedImVec2 _imageUv1       = new FinalizedImVec2() { X = 1, Y = 1 };
        private FinalizedImVec4 _imageTintCol   = new FinalizedImVec4() { X = 1, Y = 1, Z = 1, W = 1 };
        private FinalizedImVec4 _imageBorderCol = new FinalizedImVec4() { X = 1, Y = 1, Z = 1, W = 0.5f };

        public void Dispose()
        {
            _imageSize?.Dispose();
            _imageUv0?.Dispose();
            _imageUv1?.Dispose();
            _imageTintCol?.Dispose();
            _imageBorderCol?.Dispose();
        }

        public void SetImageSize(Vector2 size) => size.ToImVec(_imageSize);

        public void SetUv0(Vector2 uv) => uv.ToImVec(_imageUv0);

        public void SetUv1(Vector2 uv) => uv.ToImVec(_imageUv1);

        public void SetTintColour(Vector4 colour) => colour.ToImVec(_imageTintCol);

        public void SetBorderColour(Vector4 colour) => colour.ToImVec(_imageBorderCol);

        public void Render(IntPtr texturePtr) => ImGui.Image(texturePtr, _imageSize, _imageUv0, _imageUv1, _imageTintCol, _imageBorderCol);
    }
}
