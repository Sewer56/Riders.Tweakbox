using System;
using DearImguiSharp;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayMenu : ComponentBase<NetplayConfig>, IComponent
    {
        public NetplayController Controller = IoC.GetConstant<NetplayController>();
        public override string Name { get; set; } = "Netplay Menu";
        
        /// <inheritdoc />
        public NetplayMenu(IO io) : base(io, io.NetplayConfigFolder, io.GetNetplayConfigFiles)
        {

        }

        public override void Disable() => Controller.Disable();
        public override void Enable() => Controller.Enable();

        public override void Render()
        {
            if (ImGui.Begin("Netplay Window", ref IsEnabled(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            {
                ProfileSelector.Render();
                RenderNetplayWindow();
            }

            ImGui.End();
        }

        public unsafe void RenderNetplayWindow()
        {
            if (Controller.Socket != null)
            {
                RenderCommonWindow();
            }
            else
            {
                RenderHostJoinWindow();
            }
        }

        private void RenderCommonWindow()
        {
            var client = Controller.Socket;
            foreach (var player in client.State.PlayerInfo)
                ImGui.Text($"{player.Name} | {player.PlayerIndex} | {player.Latency}ms");

            if (ImGui.Button("Disconnect", Constants.ButtonSize))
                Controller.Socket?.Dispose();

            if (ImGui.TreeNodeStr("Bandwidth Statistics"))
            {
                RenderBandwidthUsage(client);
                ImGui.TreePop();
            }
        }

        private void RenderBandwidthUsage(Socket socket)
        {
            ImGui.Text($"Upload: {socket.Bandwidth.KBytesSent:####0.0}kbps");
            ImGui.Text($"Download: {socket.Bandwidth.KBytesReceived:####0.0}kbps");
            ImGui.Text($"Does not include UDP + IP Overhead");
        }

        private void RenderHostJoinWindow()
        {
            ref var data = ref Config.Data;
            if (ImGui.TreeNodeStr("Join a Server"))
            {
                data.ClientIP.Render("IP Address", ImGuiInputTextFlags.ImGuiInputTextFlagsCallbackCharFilter, data.ClientIP.FilterIPAddress);
                data.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                Reflection.MakeControl(ref data.ClientPort, "Port");

                if (ImGui.Button("Connect", Constants.DefaultVector2))
                    Connect();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Host"))
            {
                data.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                Reflection.MakeControl(ref data.HostPort, "Port");

                if (ImGui.Button("Host", Constants.DefaultVector2))
                    HostServer();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Player Settings"))
            {
                data.PlayerName.Render(nameof(data.PlayerName));
                Reflection.MakeControl(ref data.ShowPlayers, "Show Player Overlay");

                ImGui.TreePop();
            }

            ImGui.Spacing();

            if (data.ShowPlayers)
                RenderPlayerMenu();
        }

        private void HostServer()
        {
            try
            {
                ref var data = ref Config.Data;
                Controller.Socket = new Host(data.HostPort, data.Password.Text, Controller);
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
                ref var data = ref Config.Data;
                Controller.Socket = new Client(data.ClientIP.Text, data.ClientPort, data.Password.Text, Controller);
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
