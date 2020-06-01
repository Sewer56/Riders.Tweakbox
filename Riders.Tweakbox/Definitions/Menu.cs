using System.Collections.Generic;
using DearImguiSharp;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Definitions.Structures;

namespace Riders.Tweakbox.Definitions
{
    /// <summary>
    /// Represents a single menu of Dear ImGui's main menu bar.
    /// </summary>
    public class Menu : IComponent
    {
        public string Name { get; set; }
        public bool IsEnabled = true;
        public List<EnabledTuple<IComponent>> Components { get; } = new List<EnabledTuple<IComponent>>();

        public Menu(string name, IList<IComponent> components)
        {
            Name = name;
            foreach (var comp in components)
                Components.Add(new EnabledTuple<IComponent>(false, comp));
        }

        public void Disable()
        {
            foreach (var component in Components)
                component.Value.Disable();
        }

        public void Enable()
        {
            foreach (var component in Components)
                component.Value.Enable();
        }

        public void Render(ref bool compEnabled)
        {
            if (!compEnabled) 
                return;

            if (ImGui.BeginMenu(Name, true))
            {
                foreach (var comp in Components)
                    ImGui.MenuItemBoolPtr(comp.Value.Name, "", ref comp.Enabled, true);

                ImGui.EndMenu();
            }

            foreach (var comp in Components)
                comp.Value.Render(ref comp.Enabled);
        }
    }
}
