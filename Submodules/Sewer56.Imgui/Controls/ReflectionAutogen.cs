

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