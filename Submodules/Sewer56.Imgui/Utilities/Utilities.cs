﻿using System;
using System.Numerics;
using DearImguiSharp;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Controls.Extensions;

namespace Sewer56.Imgui.Utilities
{
    public static class Utilities
    {
        /// <summary/>
        public static FinalizedImVec4 ToImVec(this Vector4 vector)
        {
            return ToImVec(vector, new FinalizedImVec4());
        }

        /// <summary/>
        public static FinalizedImVec4 ToImVec(this Vector4 vector, FinalizedImVec4 imVec4)
        {
            imVec4.X = vector.X;
            imVec4.Y = vector.Y;
            imVec4.Z = vector.Z;
            imVec4.W = vector.W;
            return imVec4;
        }

        /// <summary/>
        public static Vector4 ToVector(this ImVec4 vector) => new Vector4(vector.X, vector.Y, vector.Z, vector.W);

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

        /// <summary>
        /// Converts a hex RGBA colour into a <see cref="ImVec4"/>.
        /// </summary>
        public static ImVec4 HexToFloat(uint hex)
        {
            return new ImVec4()
            {
                X = ((hex >> 24) & 0xFF) / (float) byte.MaxValue * 1.00f,
                Y = ((hex >> 16) & 0xFF) / (float) byte.MaxValue * 1.00f,
                Z = ((hex >> 8) & 0xFF)  / (float) byte.MaxValue * 1.00f,
                W = (hex & 0xFF) / (float) byte.MaxValue * 1.00f,
            };
        }
    }
}
