using System;
using System.Windows.Forms;
using DearImguiSharp;
using Reloaded.WPF.Animations.FrameLimiter;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Controls.Extensions;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell;
using static DearImguiSharp.ImGuiInputTextFlags;
using static DearImguiSharp.ImGuiStyleVar;
using static DearImguiSharp.ImGuiWindowFlags;

namespace Riders.Tweakbox.Components.Netplay.Menus
{
    /// <summary>
    /// Service which provides a chatbox to interact with the current Netplay session.
    /// </summary>
    public class ChatMenu
    {
        private const int _bufferCapacity = 1000;

        public string Name { get; set; } = "Chat Menu";
        
        private TextInputData _textBox = new TextInputData(ChatMessage.MaxLength, 1);
        private CircularBuffer<string> _messages;

        private bool _isOpen = true;
        private Func<string> _getPlayerName;
        private Action<string> _sendMessage;
        private Func<bool> _shouldShow;
        private FinalizedImVec2 _chatlogDimensions = new FinalizedImVec2() { X = 0, Y = -20 };

        public ChatMenu(Func<string> getPlayerName, Action<string> sendMessage, Func<bool> shouldShow)
        {
            _getPlayerName = getPlayerName;
            _sendMessage = sendMessage;
            _shouldShow = shouldShow;

            Clear();
            Shell.AddCustom(ShellRender);
        }

        /// <summary>
        /// Clears all the messages in the window.
        /// </summary>
        public void Clear() => _messages = new CircularBuffer<string>(_bufferCapacity);

        private bool ShellRender()
        {
            if (!_shouldShow())
                return true;

            if (ImGui.Begin(Name, ref _isOpen, 0))
                RenderChatMenu();

            ImGui.End();
            return true;
        }

        /// <summary>
        /// Adds a message to the log.
        /// </summary>
        /// <param name="source">The source of the message, such as a player name.</param>
        /// <param name="message">The message.</param>
        public void AddMessage(string source, string message) => _messages.PushBack(FormatMessage(source, message));

        /// <summary>
        /// Adds a message to the log.
        /// </summary>
        /// <param name="text">The raw message.</param>
        public void AddMessageUnformatted(string text) => _messages.PushBack(text);

        /// <summary>
        /// Formats a message to the format used by the chat box.
        /// </summary>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string FormatMessage(string source, string message) => $"[{source}] {message}";

        private void AddMessage(string message) => _messages.PushBack(message);

        private void RenderChatMenu()
        {
            // Render Message Log
            RenderMessageLog();

            // Render Basic Text
            ImGui.SetNextItemWidth(-50);
            ImGui.BeginGroup();
            bool send = _textBox.Render("", ImGuiInputTextFlagsEnterReturnsTrue);
            if (send)
                ImGui.ActivateItem(ImGui.GetItemID());

            ImGui.SameLine(0, 10);
            bool buttonSend = ImGui.Button("Send", Constants.Zero);
            send = buttonSend || send;
            ImGui.EndGroup();
            
            if (send)
                SendMessage();
        }

        private void SendMessage()
        {
            var message = _textBox.ToString();
            if (string.IsNullOrEmpty(message))
                return;

            AddMessage(FormatMessage(_getPlayerName(), message));
            _sendMessage(message);
            _textBox.Clear();
        }

        private void RenderMessageLog()
        {
            // Setup
            ImGui.BeginChildStr("Frame", _chatlogDimensions, false, (int)(ImGuiWindowFlagsHorizontalScrollbar));
            ImGui.PushStyleVarVec2((int)ImGuiStyleVarItemSpacing, Constants.Zero);

            // Log
            using var clipper = CreateClipper();
            ImGui.ImGuiListClipperBegin(clipper, _messages.Size, 0);
            while (ImGui.ImGuiListClipperStep(clipper))
            {
                for (int x = clipper.DisplayStart; x < clipper.DisplayEnd; x++)
                    ImGui.Text(_messages[x]);
            }
            
            ImGui.ImGuiListClipperEnd(clipper);

            // Autoscroll
            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            // Cleanup
            ImGui.PopStyleVar(1);
            ImGui.EndChild();
        }

        private ImGuiListClipper CreateClipper() => new ImGuiListClipper()
        {
            DisplayEnd = 0,
            DisplayStart = 0,
            ItemsCount = 0,
            ItemsFrozen = 0,
            ItemsHeight = 0,
            StartPosY = 0,
            StepNo = 0
        };
    }
}
