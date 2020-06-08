using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using DearImguiSharp;
using EnumsNET;
using Sewer56.NumberUtilities.Primitives.Interfaces;

namespace Sewer56.Imgui.Utilities
{
    public unsafe partial class Reflection
    {
        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(Vector3* value, string name)
        {
            ImGui.Custom.DragFloat3(name, value, 1.0f);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(ref bool value, string name)
        {
            ImGui.Checkbox(name, ref value);
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        public static void MakeControl(bool* value, string name)
        {
            ImGui.Checkbox(name, ref Unsafe.AsRef<bool>(value));
        }

        /// <summary>
        /// Creates a ComboBox given a set of values, names, and the name of the current item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the ComboBox</param>
        /// <param name="currentValue">The currently selected item.</param>
        /// <param name="currentName">Name of the current item.</param>
        /// <param name="values">The available items.</param>
        /// <param name="names">The names of the available items.</param>
        /// <param name="itemSelected">Executed when a new item is selected. Returns the selected value.</param>
        public static void MakeControlComboBox<T>(string name, T currentValue, string currentName, IReadOnlyList<T> values, IReadOnlyList<string> names, Action<T> itemSelected)
        {
            if (ImGui.BeginCombo(name, currentName, 0))
            {
                for (int x = 0; x < values.Count; x++)
                {
                    bool isSelected = currentValue.Equals(values[x]);
                    if (ImGui.SelectableBool(names[x], isSelected, 0, Constants.DefaultVector2))
                    {
                        currentValue = values[x];
                        itemSelected(currentValue);
                    }

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
        }

        /// <summary>
        /// Adds a Dear Imgui Control to the scene for a specified type.
        /// </summary>
        /// <param name="value">The value to bind to the UI.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="itemWidth">Width of each item if enum is a set of flags.</param>
        public static void MakeControlEnum<T>(T* value, string name, int itemWidth = 120) where T : unmanaged, Enum
        {
            var values  = Enums.GetValues<T>();
            var names   = Enums.GetNames<T>();
            var isFlags = typeof(T).IsDefined(typeof(FlagsAttribute), false);
            var currentItemName = Enums.AsString(typeof(T), *value);

            if (isFlags)
            {
                MakeControlEnumFlags<T>(value, values, names, itemWidth);
            }
            else
            {
                MakeControlEnumComboBox<T>(name, currentItemName, value, values, names);
            }
        }

        private static void MakeControlEnumFlags<T>(T* currentValue, IReadOnlyList<T> values, IReadOnlyList<string> names, int itemWidth) where T : unmanaged, Enum
        {
            var wrapperUtility = new ContentWrapper(itemWidth);
            
            for (int x = 0; x < names.Count; x++)
            {
                var hasFlag = currentValue->HasAnyFlags(values[x]);
                if (ImGui.Checkbox(names[x], ref hasFlag))
                    *currentValue = FlagEnums.ToggleFlags(*currentValue, values[x]);

                wrapperUtility.AfterPlaceItem(x == names.Count - 1);
            }
        }

        private static void MakeControlEnumComboBox<T>(string name, string currentItemName, T* currentValue, IReadOnlyList<T> values, IReadOnlyList<string> names) where T : unmanaged, Enum
        {
            if (ImGui.BeginCombo(name, currentItemName, 0))
            {
                for (int x = 0; x < values.Count; x++)
                {
                    bool isSelected = Enums.EqualsUnsafe(*currentValue, values[x]);
                    if (ImGui.SelectableBool(names[x], isSelected, 0, Constants.DefaultVector2))
                        *currentValue = values[x];

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
        }
    }
}
