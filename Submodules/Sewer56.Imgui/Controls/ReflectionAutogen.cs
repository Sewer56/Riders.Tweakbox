

using System;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using DearImguiSharp;

namespace Sewer56.Imgui.Controls
{
    public unsafe partial class Reflection
    {
        #region Make Controls
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Byte* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Byte value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Byte* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Byte value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Byte* value, string name, float speed, Byte* min, Byte* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Byte* value, string name, float speed, Byte min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Byte* value, string name, float speed, Byte min, Byte max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Byte value, string name, float speed, ref Byte min, ref Byte max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Byte value, string name, float speed, Byte min, Byte max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Byte* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Byte value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU8, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(SByte* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref SByte value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(SByte* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref SByte value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(SByte* value, string name, float speed, SByte* min, SByte* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(SByte* value, string name, float speed, SByte min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(SByte* value, string name, float speed, SByte min, SByte max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref SByte value, string name, float speed, ref SByte min, ref SByte max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref SByte value, string name, float speed, SByte min, SByte max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(SByte* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref SByte value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS8, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Int16* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Int16 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Int16* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Int16 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int16* value, string name, float speed, Int16* min, Int16* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int16* value, string name, float speed, Int16 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int16* value, string name, float speed, Int16 min, Int16 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int16 value, string name, float speed, ref Int16 min, ref Int16 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int16 value, string name, float speed, Int16 min, Int16 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Int16* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Int16 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS16, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(UInt16* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref UInt16 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(UInt16* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref UInt16 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt16* value, string name, float speed, UInt16* min, UInt16* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt16* value, string name, float speed, UInt16 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt16* value, string name, float speed, UInt16 min, UInt16 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt16 value, string name, float speed, ref UInt16 min, ref UInt16 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt16 value, string name, float speed, UInt16 min, UInt16 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(UInt16* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref UInt16 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU16, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Int32* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Int32 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Int32* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Int32 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int32* value, string name, float speed, Int32* min, Int32* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int32* value, string name, float speed, Int32 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int32* value, string name, float speed, Int32 min, Int32 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int32 value, string name, float speed, ref Int32 min, ref Int32 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int32 value, string name, float speed, Int32 min, Int32 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Int32* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Int32 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS32, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(UInt32* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref UInt32 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(UInt32* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref UInt32 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt32* value, string name, float speed, UInt32* min, UInt32* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt32* value, string name, float speed, UInt32 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt32* value, string name, float speed, UInt32 min, UInt32 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt32 value, string name, float speed, ref UInt32 min, ref UInt32 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt32 value, string name, float speed, UInt32 min, UInt32 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(UInt32* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref UInt32 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Int64* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Int64 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Int64* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Int64 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int64* value, string name, float speed, Int64* min, Int64* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int64* value, string name, float speed, Int64 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Int64* value, string name, float speed, Int64 min, Int64 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int64 value, string name, float speed, ref Int64 min, ref Int64 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Int64 value, string name, float speed, Int64 min, Int64 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Int64* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Int64 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeS64, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(UInt64* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref UInt64 value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(UInt64* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref UInt64 value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt64* value, string name, float speed, UInt64* min, UInt64* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt64* value, string name, float speed, UInt64 min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(UInt64* value, string name, float speed, UInt64 min, UInt64 max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt64 value, string name, float speed, ref UInt64 min, ref UInt64 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref UInt64 value, string name, float speed, UInt64 min, UInt64 max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(UInt64* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref UInt64 value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeU64, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Single* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Single value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Single* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Single value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Single* value, string name, float speed, Single* min, Single* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Single* value, string name, float speed, Single min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Single* value, string name, float speed, Single min, Single max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Single value, string name, float speed, ref Single min, ref Single max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Single value, string name, float speed, Single min, Single max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Single* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Single value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeFloat, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(Double* value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static bool MakeControl(ref Double value, string name)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), 1.0F, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(Double* value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        public static bool MakeControl(ref Double value, string name, float speed)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Double* value, string name, float speed, Double* min, Double* max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, speed, (IntPtr) min, (IntPtr) max, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Double* value, string name, float speed, Double min)
        {
            var minPtr = &min;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, speed, (IntPtr) minPtr, IntPtr.Zero, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(Double* value, string name, float speed, Double min, Double max)
        {
            var minPtr = &min;
            var maxPtr = &max;
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, speed, (IntPtr) minPtr, (IntPtr) maxPtr, null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Double value, string name, float speed, ref Double min, ref Double max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="min">The minimum value allowed.</param>
        /// <param name="max">The maximum value allowed.</param>
        public static bool MakeControl(ref Double value, string name, float speed, Double min, Double max)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), speed, (IntPtr) Unsafe.AsPointer(ref min), (IntPtr) Unsafe.AsPointer(ref max), null, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(Double* value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) value, speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="speed">The speed of the slider.</param>
        /// <param name="format">The C style format of the string displayed.</param>
        public static bool MakeControl(ref Double value, string name, float speed, string format)
        {
            return ImGui.DragScalar(name, (int)ImGuiDataType.ImGuiDataTypeDouble, (IntPtr) Unsafe.AsPointer(ref value), speed, IntPtr.Zero, IntPtr.Zero, format, 1);
        }
                
        #endregion
    }
}