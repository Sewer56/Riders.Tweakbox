using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Misc
{
    public class LogWindow : IComponent
    {
        private bool _isEnabled;

        /// <inheritdoc />
        public string Name { get; set; } = "Log Configuration";

        /// <inheritdoc />
        public ref bool IsEnabled() => ref _isEnabled;

        /// <inheritdoc />
        public void Disable() { }

        /// <inheritdoc />
        public void Enable() { }

        /// <inheritdoc />
        public unsafe void Render()
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                Reflection.MakeControlEnum((LogCategory*) Unsafe.AsPointer(ref Log.EnabledCategories), "Enabled Categories");
            }

            ImGui.End();
        }
    }
}
