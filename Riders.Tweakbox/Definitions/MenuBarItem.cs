using System.Collections.Generic;
using DearImguiSharp;
using Riders.Tweakbox.Definitions.Interfaces;

namespace Riders.Tweakbox.Definitions
{
    /// <summary>
    /// Represents a single menu of Dear ImGui's main menu bar.
    /// </summary>
    public class MenuBarItem : IComponent
    {
        public string Name { get; set; }
        public List<IComponent> Components { get; } = new List<IComponent>();
        private bool _isEnabled = true;

        public MenuBarItem(string name, IList<IComponent> components)
        {
            Name = name;
            Components.AddRange(components);
        }

        public void Disable()
        {
            foreach (var component in Components)
                component.Disable();
        }

        public void Enable()
        {
            foreach (var component in Components)
                component.Enable();
        }

        public void Render(ref bool compEnabled)
        {
            if (!compEnabled) 
                return;

            if (ImGui.BeginMenu(Name, true))
            {
                foreach (var comp in Components)
                    ImGui.MenuItemBoolPtr(comp.Name, "", ref comp.IsEnabled(), comp.IsAvailable());

                ImGui.EndMenu();
            }

            foreach (var comp in Components)
                if (comp.IsEnabled() && comp.IsAvailable())
                    comp.Render();
        }

        public ref bool IsEnabled() => ref _isEnabled;
        public bool IsAvailable() => true;
        public void Render() { }
    }
}
