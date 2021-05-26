﻿using System;
using System.Diagnostics;
using DearImguiSharp;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Tweakbox.Components.Netplay.Components.Game;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Misc;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayLobbyMenu : ComponentBase
    {
        public NetplayMenu Owner { get; set; }
        public NetplayController Controller => Owner.Controller;
        public NetplayEditorConfig Config => Owner.Config;

        /// <inheritdoc />
        public override string Name { get; set; } = "Netplay Lobby";

        public NetplayLobbyMenu(NetplayMenu netplayMenu)
        {
            Owner = netplayMenu;
        }

        /// <inheritdoc />
        public override void Render()
        {
            if (Controller.Socket == null)
                return;

            if (ImGui.Begin(Name, ref IsEnabled(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
                RenderConnectedWindow();

            ImGui.End();
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
            ImGui.Text($"Packet Loss Percent: {socket.Manager.Statistics.PacketLossPercent:0.000}%");

            ImGui.Text($"Including UDP + IP Overhead");
            ImGui.Text($"Upload: {socket.Bandwidth.KBytesSentWithOverhead * 8:####0.0}kbps");
            ImGui.Text($"Download: {socket.Bandwidth.KBytesReceivedWithOverhead * 8:####0.0}kbps");
            ImGui.Separator();

            ImGui.Text($"IP + UDP Overhead");
            ImGui.Text($"Upload: {socket.Bandwidth.KBytesPacketOverheadSent * 8:####0.0}kbps");
            ImGui.Text($"Download: {socket.Bandwidth.KBytesPacketOverheadReceived * 8:####0.0}kbps");
        }

        [Conditional("DEBUG")]
        internal void RenderDebugOptions()
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
                    RenderHybridBufferDetails(buffers, socket.State.GetPlayerCount());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void RenderHybridBufferDetails(IJitterBuffer<UnreliablePacket>[] buffers, int playerCount)
        {
            for (int x = 0; x < playerCount; x++)
            {
                var buffer = (HybridJitterBuffer<UnreliablePacket>)buffers[x];
                RenderDefaultBufferInfo(buffer.Buffer, x);
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
            ImGui.DragFloat($"Jitter Ramp Up Percentile", ref AdaptiveJitterBufferConstants.JitterRampUpPercentile, 0.001f, 0f, 1f, null, 0);
            ImGui.DragFloat($"Jitter Ramp Down Percentile", ref AdaptiveJitterBufferConstants.JitterRampDownPercentile, 0.001f, 0f, 1f, null, 0);
            for (int x = 0; x < playerCount; x++)
            {
                var buffer = (AdaptiveJitterBuffer<UnreliablePacket>) buffers[x];
                RenderDefaultBufferInfo(buffer.Buffer, x);
            }
        }

        private static void RenderDefaultBufferInfo(JitterBuffer<UnreliablePacket> buffer, int playerIndex)
        {
            var bufferedPackets = buffer.BufferSize;
            ImGui.DragInt($"Num Buf Pkt [P{playerIndex}]", ref bufferedPackets, 0.1f, 0, 60, null, 0);
            ImGui.Checkbox($"Low Latency Mode", ref buffer.LowLatencyMode);
            ImGui.Text($"Num in Window: {buffer.GetNumPacketsInWindow()}");
            ImGui.Text($"Num in Buf: {buffer.PacketCount}");
            buffer.SetBufferSize(bufferedPackets);
        }
    }
}
