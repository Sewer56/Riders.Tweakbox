using System;
using System.Numerics;
using DearImguiSharp;

namespace Sewer56.Imgui.Utilities
{
    public static class Utilities
    {
        /// <summary>
        /// Runs a function that accepts a vector and returns the result.
        /// </summary>
        public static Vector2 RunVectorFunction(Action<ImVec2> function)
        {
            var vec = new ImVec2();
            function(vec);
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// Retrieves the content region of the window.
        /// </summary>
        public static Vector2 GetWindowRightCornerPosition() => RunVectorFunction(ImGui.GetWindowPos) + RunVectorFunction(ImGui.GetWindowSize);

        /// <summary/>
        public static ImVec4 ToImVec(this Vector4 vector)
        {
            var vec = new ImVec4
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
                W = vector.W
            };

            return vec;
        }

        /// <summary/>
        public static ImVec2 ToImVec(this Vector2 vector)
        {
            var vec = new ImVec2
            {
                X = vector.X,
                Y = vector.Y
            };

            return vec;
        }
    }
}
