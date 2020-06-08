using DearImguiSharp;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Definitions.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayMenu : IComponent
    {
        public NetplayImguiConfig Config    = IoC.GetConstant<NetplayImguiConfig>();
        public NetplayController Controller = IoC.GetConstant<NetplayController>();
        public string Name { get; set; } = "Netplay Menu";
        private bool _isEnabled;
        
        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() => IoC.GetConstant<NetplayController>().Disable();
        public void Enable() => IoC.GetConstant<NetplayController>().Enable();

        public void Render()
        {
            if (ImGui.Begin("Netplay Window", ref _isEnabled, 0))
            {
                RenderNetplayWindow();
            }

            ImGui.End();
        }

        public unsafe void RenderNetplayWindow()
        {
            if (ImGui.TreeNodeStr("Join a Server"))
            {
                Config.ClientIP.Render("IP Address", ImGuiInputTextFlags.ImGuiInputTextFlagsCallbackCharFilter, Config.ClientIP.FilterIPAddress);
                Reflection.MakeControl(ref Config.ClientPort, "Port");

                if (ImGui.Button("Connect", Constants.DefaultVector2))
                    Connect();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Host"))
            {
                Config.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                Reflection.MakeControl(ref Config.HostPort, "Port");

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Player Settings"))
            {
                Config.PlayerName.Render(nameof(NetplayConfig.PlayerName));
                Reflection.MakeControl(ref Config.ShowPlayers, "Show Player Overlay");

                ImGui.TreePop();
            }

            ImGui.Spacing();
            if (ImGui.Button("Save Settings", Constants.DefaultVector2))
                Config.Save();

            if (Config.ShowPlayers) 
                RenderPlayerMenu();
        }

        private void Connect()
        {

        }

        public void RenderPlayerMenu()
        {

        }
    }
}
