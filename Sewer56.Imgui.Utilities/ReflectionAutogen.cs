

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using DearImguiSharp;

namespace Sewer56.Imgui.Utilities
{
    public unsafe partial class Reflection
    {
        #region Make Controls
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Byte* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Byte value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(SByte* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref SByte value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Int16* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Int16 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(UInt16* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref UInt16 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Int32* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Int32 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(UInt32* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref UInt32 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Int64* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Int64 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(UInt64* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref UInt64 value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Single* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Single value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Double* value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref Double value, string name)
        {
            ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

                
        #endregion
    }
}