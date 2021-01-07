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
        public static unsafe Vector2 RunVectorFunction(Action<ImVec2> function)
        {
            var vec2 = new Vector2();
            function(new ImVec2(&vec2));
            return vec2;
        }

        /// <summary>
        /// Runs a function that accepts a vector and returns the result.
        /// </summary>
        public static unsafe Vector4 RunVectorFunction(Action<ImVec4> function)
        {
            var vec4 = new Vector4();
            function(new ImVec4(&vec4));
            return vec4;
        }

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
