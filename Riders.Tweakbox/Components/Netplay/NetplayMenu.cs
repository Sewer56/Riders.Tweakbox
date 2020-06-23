using System;
using DearImguiSharp;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayMenu : IComponent
    {
        public NetplayImguiConfig Config    = IoC.GetConstant<NetplayImguiConfig>();
        public NetplayController Controller = IoC.GetConstant<NetplayController>();
        public string Name { get; set; } = "Netplay Menu";
        private bool _isEnabled;
        
        public ref bool IsEnabled() => ref _isEnabled;
        public void Disable() => Controller.Disable();
        public void Enable() => Controller.Enable();

        public void Render()
        {
            if (ImGui.Begin("Netplay Window", ref _isEnabled, (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            {
                RenderNetplayWindow();
            }

            ImGui.End();
        }

        public unsafe void RenderNetplayWindow()
        {
            if (Controller.Socket != null)
            {
                if (Controller.Socket.IsHost())
                {
                    RenderHostWindow();
                }
                else
                {
                    RenderClientWindow();
                }
            }
            else
            {
                RenderHostJoinWindow();
            }
        }

        private void RenderClientWindow()
        {
            var client = (Client)Controller.Socket;
            foreach (var player in client.State.PlayerInfo)
            {
                ImGui.Text($"{player.Name} | {player.PlayerIndex}");
            }

            if (ImGui.Button("Disconnect", Constants.ButtonSize))
                Disconnect();
        }

        private void RenderHostWindow()
        {
            var host = (Host)Controller.Socket;
            foreach (var player in host.State.PlayerMap.GetPlayerData())
            {
                ImGui.Text($"{player.Name} | {player.PlayerIndex}");
            }

            if (ImGui.Button("Disconnect", Constants.ButtonSize))
                Disconnect();
        }

        private void Disconnect()
        {
            Controller.Socket?.Dispose();
        }

        private void RenderHostJoinWindow()
        {
            if (ImGui.TreeNodeStr("Join a Server"))
            {
                Config.ClientIP.Render("IP Address", ImGuiInputTextFlags.ImGuiInputTextFlagsCallbackCharFilter, Config.ClientIP.FilterIPAddress);
                Config.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                Reflection.MakeControl(ref Config.ClientPort, "Port");

                if (ImGui.Button("Connect", Constants.DefaultVector2))
                    Connect();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Host"))
            {
                Config.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                Reflection.MakeControl(ref Config.HostPort, "Port");

                if (ImGui.Button("Host", Constants.DefaultVector2))
                    HostServer();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Player Settings"))
            {
                Config.PlayerName.Render(nameof(NetplayConfigFile.PlayerName));
                Reflection.MakeControl(ref Config.ShowPlayers, "Show Player Overlay");

                ImGui.TreePop();
            }

            ImGui.Spacing();
            if (ImGui.Button("Save Settings", Constants.DefaultVector2))
                Config.Save();

            if (Config.ShowPlayers)
                RenderPlayerMenu();
        }

        private void HostServer()
        {
            try
            {
                Controller.Socket = new Host(Config.HostPort, Config.Password.Text, Controller);
            }
            catch (Exception e)
            {
                Shell.AddDialog("Host Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }

        private void Connect()
        {
            try
            {
                Controller.Socket = new Client(Config.ClientIP.Text, Config.ClientPort, Config.Password.Text, Controller);
            }
            catch (Exception e)
            {
                Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }

        public void RenderPlayerMenu()
        {

        }
    }
}
