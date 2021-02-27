using System;
using System.Diagnostics;
using DearImguiSharp;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Tweakbox.Components.Netplay.Components.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayMenu : ComponentBase<NetplayConfig>, IComponent
    {
        public NetplayController Controller = IoC.Get<NetplayController>();
        public override string Name { get; set; } = "Netplay Menu";
        
        /// <inheritdoc />
        public NetplayMenu(IO io) : base(io, io.NetplayConfigFolder, io.GetNetplayConfigFiles, IO.JsonConfigExtension)
        {

        }

        public override void Render()
        {
            if (ImGui.Begin("Netplay Window", ref IsEnabled(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            {
                RenderNetplayWindow();
            }

            ImGui.End();
        }

        public unsafe void RenderNetplayWindow()
        {
            if (Controller.Socket != null)
                RenderConnectedWindow();
            else
                RenderDisconnectedWindow();
        }

        private void RenderConnectedWindow()
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

            RenderDebugOptions();
        }

        private void RenderBandwidthUsage(Socket socket)
        {
            ImGui.Text($"Including UDP + IP Overhead");
            ImGui.Text($"Upload: {socket.Bandwidth.KBytesSentWithOverhead * 8:####0.0}kbps");
            ImGui.Text($"Download: {socket.Bandwidth.KBytesReceivedWithOverhead * 8:####0.0}kbps");
            ImGui.Separator();

            ImGui.Text($"IP + UDP Overhead");
            ImGui.Text($"Upload: {socket.Bandwidth.KBytesPacketOverheadSent * 8:####0.0}kbps");
            ImGui.Text($"Download: {socket.Bandwidth.KBytesPacketOverheadReceived * 8:####0.0}kbps");
        }

        private void RenderDisconnectedWindow()
        {
            ProfileSelector.Render();
            ref var data = ref Config.Data;

            if (ImGui.TreeNodeStr("Player Settings"))
            {
                ref var playerSettings = ref data.PlayerSettings;
                playerSettings.PlayerName.Render(nameof(playerSettings.PlayerName));

                ImGui.DragInt("Number of Players", ref playerSettings.LocalPlayers, 0.1f, 0, Riders.Netplay.Messages.Misc.Constants.MaxNumberOfLocalPlayers, null);
                Tooltip.TextOnHover("Default: 1\n" +
                                    "Number of local players playing online.\n" +
                                    "Setting this value to 0 makes you a spectator.");

                ImGui.DragInt("Max Number of Cameras", ref playerSettings.MaxNumberOfCameras, 0.1f, 0, Riders.Netplay.Messages.Misc.Constants.MaxNumberOfLocalPlayers, null);
                Tooltip.TextOnHover("Overrides the number of cameras, allowing you to spectate other players while in online multiplayer.\n" +
                                    "0 = Automatic\n" +
                                    "1 = Single Screen\n" +
                                    "2 = Split-Screen\n" +
                                    "3-4 = 4-way Split Screen.");

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Join a Server"))
            {
                ref var clientData = ref data.ClientSettings;

                clientData.IP.Render("IP Address", ImGuiInputTextFlags.ImGuiInputTextFlagsCallbackCharFilter, clientData.IP.FilterIPAddress);
                clientData.SocketSettings.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                ImGui.DragInt("Port", ref clientData.SocketSettings.Port, 0.1f, 0, ushort.MaxValue, null);
                
                if (ImGui.Button("Connect", Constants.DefaultVector2))
                    Connect();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("Host"))
            {
                ref var hostData = ref data.HostSettings;

                hostData.SocketSettings.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                ImGui.DragInt("Port", ref hostData.SocketSettings.Port, 0.1f, 0, ushort.MaxValue, null);

                ImGui.Checkbox("Reduced Non-Essential Tick Rate", ref hostData.ReducedTickRate);
                Tooltip.TextOnHover("Only use when hosting 8 player lobby and upload speed is less than 1Mbit/s.\n" +
                                    "Reduces the send-rate of non-essential elements such as players' amount of air, rings, flags and other misc. content.");

                if (ImGui.Button("Host", Constants.DefaultVector2))
                    HostServer();

                ImGui.TreePop();
            }

            if (ImGui.TreeNodeStr("NAT Punch/Traversal Server"))
            {
                ref var punchingServer = ref data.PunchingServer;
                Reflection.MakeControl(ref punchingServer.IsEnabled, "Enabled");
                Tooltip.TextOnHover("Uses a third party server to try to bypass your router's firewall to establish a connection.\nHighly likely but not guaranteed to work. Use if you are unable or do not know how to port forward.");

                if (punchingServer.IsEnabled)
                {
                    punchingServer.Host.Render("Host Server");
                    ImGui.DragInt("Port", ref punchingServer.Port, 0.1f, 0, ushort.MaxValue, null);
                    
                    ImGui.DragInt("Server Timeout", ref punchingServer.ServerTimeout, 0.1f, 0, ushort.MaxValue, null);
                    Tooltip.TextOnHover("(Milliseconds) Timeout for connecting to the third party server.");

                    ImGui.DragInt("Punch Timeout", ref punchingServer.PunchTimeout, 0.1f, 0, ushort.MaxValue, null);
                    Tooltip.TextOnHover("(Milliseconds) Timeout for trying to hole punch past the server firewall.\nIf you have trouble issues connecting, increasing this value might help.");
                }

                ImGui.TreePop();
            }

            RenderDebugOptions();
            ImGui.Spacing();
        }

        private void HostServer()
        {
            try
            {
                Controller.Socket = new Host(Config, Controller);
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
                Controller.Socket = new Client(Config, Controller);
            }
            catch (Exception e)
            {
                Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }

        [Conditional("DEBUG")]
        private void RenderDebugOptions()
        {
            ref var data = ref Config.Data;
            var badInternet = data.BadInternet;
            if (!ImGui.TreeNodeStr("Debug")) 
                return;

            Reflection.MakeControl(ref badInternet.IsEnabled, "Simulate Bad Internet");
            if (badInternet.IsEnabled)
            {
                Reflection.MakeControl(ref badInternet.MinLatency, "Min Latency");
                Reflection.MakeControl(ref badInternet.MaxLatency, "Max Latency");
                Reflection.MakeControl(ref badInternet.PacketLoss, "Packet Loss Percent");
            }

            if (Controller.Socket == null)
            {
                ImGui.TreePop();
                return;
            }

            // Render jitter buffer info.
            ImGui.Separator();
            RenderJitterBufferDetails(Controller.Socket);
            badInternet.Apply(Controller.Socket.Manager);
            ImGui.TreePop();
        }

        private static void RenderJitterBufferDetails(Socket socket)
        {
            if (!socket.TryGetComponent(out Race race)) 
                return;

            var buffers = race.JitterBuffers;
            var jitterBufferType = buffers[0].GetBufferType();
            ImGui.Text($"{jitterBufferType} Jitter Buffer Stats");

            switch (jitterBufferType)
            {
                case JitterBufferType.Simple:
                    RenderDefaultBufferDetails(buffers, socket.State.GetPlayerCount());
                    break;
                case JitterBufferType.Adaptive:
                    RenderAdaptiveBufferDetails(buffers, socket.State.GetPlayerCount());
                    break;
                case JitterBufferType.Hybrid:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void RenderDefaultBufferDetails(IJitterBuffer<UnreliablePacket>[] buffers, int playerCount)
        {
            for (int x = 0; x < playerCount; x++)
            {
                var buffer = (JitterBuffer<UnreliablePacket>)buffers[x];
                RenderDefaultBufferInfo(buffer, x);
            }
        }

        private static void RenderAdaptiveBufferDetails(IJitterBuffer<UnreliablePacket>[] buffers, int playerCount)
        {
            ImGui.DragFloat($"Jitter Ramp Up Percentile", ref AdaptiveJitterBufferConstants.JitterRampUpPercentile, 0.001f, 0f, 1f, null, 1f);
            ImGui.DragFloat($"Jitter Ramp Down Percentile", ref AdaptiveJitterBufferConstants.JitterRampDownPercentile, 0.001f, 0f, 1f, null, 1f);
            for (int x = 0; x < playerCount; x++)
            {
                var buffer = (AdaptiveJitterBuffer<UnreliablePacket>) buffers[x];
                RenderDefaultBufferInfo(buffer.Buffer, x);
            }
        }

        private static void RenderDefaultBufferInfo(JitterBuffer<UnreliablePacket> buffer, int playerIndex)
        {
            var bufferedPackets = buffer.BufferSize;
            ImGui.DragInt($"Num Buf Pkt [P{playerIndex}]", ref bufferedPackets, 0.1f, 0, 60, null);
            ImGui.Checkbox($"Low Latency Mode", ref buffer.LowLatencyMode);
            ImGui.Text($"Num in Window: {buffer.GetNumPacketsInWindow()}");
            ImGui.Text($"Num in Buf: {buffer.PacketCount}");
            buffer.SetBufferSize(bufferedPackets);
        }
    }
}
