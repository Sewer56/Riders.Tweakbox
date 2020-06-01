﻿using System;
using System.IO;
using System.Linq;
using DearImguiSharp;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiInputTextFlags;
using static Riders.Tweakbox.Misc.FileSystemWatcherFactory.FileSystemWatcherEvents;

namespace Riders.Tweakbox.Definitions
{
    /// <summary>
    /// Represents a profile selector widget, which actively monitors all available config files and allows the user to select one.
    /// </summary>
    public unsafe class ProfileSelector
    {
        private const string NewDialogId    = "New Profile";
        private const string DeleteDialogId = "Delete?";
        
        public string Directory { get; private set; }

        private TextInputData _inputData = new TextInputData(48);
        private byte[] _newConfigBytes;
        private Func<string[]> _getConfigFiles;
        private Func<byte[]> _getCurrentConfigBytes;
        private Action<byte[]> _loadConfig;

        private string _currentConfiguration;
        private string[] _configurations;
        private FileSystemWatcher _configWatcher;

        /// <summary>
        /// Represents a profile selector widget.
        /// </summary>
        /// <param name="directory">The directory for which to load/save profiles.</param>
        /// <param name="getConfigFiles">A function that obtains all config file names.</param>
        /// <param name="loadConfig">Executed when a new configuration is to be read. Parameter is config data.</param>
        /// <param name="newConfigBytes">The bytes used for a new configuration, typically the default configuration.</param>
        /// <param name="getCurrentConfigBytes">Gets the bytes for the current configuration.</param>
        public ProfileSelector(string directory, byte[] newConfigBytes, Func<string[]> getConfigFiles, Action<byte[]> loadConfig, Func<byte[]> getCurrentConfigBytes)
        {
            Directory = directory;
            _getConfigFiles = getConfigFiles;
            _loadConfig = loadConfig;
            _newConfigBytes = newConfigBytes;
            _getCurrentConfigBytes = getCurrentConfigBytes;

            _currentConfiguration = $"{Directory}/Default{IO.ConfigExtension}";
            _configWatcher = IO.CreateConfigWatcher(Directory, OnConfigsUpdated, Changed | Created | Deleted | Renamed);
        }

        private void OnConfigsUpdated()
        {
            _configurations = _getConfigFiles();
            if (string.IsNullOrEmpty(_currentConfiguration) || !File.Exists(_currentConfiguration))
                _currentConfiguration = _configurations.FirstOrDefault();
        }

        /// <summary>
        /// Creates a new configuration.
        /// </summary>
        public void New(string name) => New(name, _newConfigBytes);

        /// <summary>
        /// Creates a new configuration with some custom data.
        /// </summary>
        public void New(string name, byte[] data)
        {
            var path = $"{Directory}/{name}{IO.ConfigExtension}";
            File.WriteAllBytes(path, data);
            _loadConfig(data);
            _currentConfiguration = path;
        }

        /// <summary>
        /// Saves the current configuration.
        /// </summary>
        public void Save() => Save(_getCurrentConfigBytes());

        /// <summary>
        /// Saves the current configuration with some custom data.
        /// </summary>
        public void Save(byte[] data) => File.WriteAllBytes(_currentConfiguration, data);

        /// <summary>
        /// Deletes the current configuration if it exists.
        /// </summary>
        public void Delete()
        {
            if (File.Exists(_currentConfiguration))
            {
                File.Delete(_currentConfiguration);
                OnConfigsUpdated();
            }
        }

        /// <summary>
        /// Renders the UI control to the screen.
        /// </summary>
        public void Render()
        {
            ImGui.TextWrapped("Profile Selector");
            var currentConfigName = Path.GetFileName(_currentConfiguration);
            var currentConfigNames = _configurations.Select(Path.GetFileName).ToArray();

            Reflection.MakeControlComboBox("Current Profile", currentConfigName, currentConfigName, currentConfigNames, currentConfigNames,
                x =>
                {
                    _currentConfiguration = _configurations[currentConfigNames.IndexOf(y => y == x)];
                });

            if (ImGui.Button("New", Constants.DefaultVector2))
                ImGui.OpenPopup(NewDialogId);

            ImGui.SameLine(0, Constants.Spacing);
            if (ImGui.Button("Delete", Constants.DefaultVector2))
                ImGui.OpenPopup(DeleteDialogId);

            ImGui.SameLine(0, Constants.Spacing);
            if (ImGui.Button("Load", Constants.DefaultVector2))
            {
                if (!string.IsNullOrEmpty(_currentConfiguration) && File.Exists(_currentConfiguration))
                    _loadConfig(File.ReadAllBytes(_currentConfiguration));
            }

            ImGui.SameLine(0, Constants.Spacing);
            if (ImGui.Button("Save", Constants.DefaultVector2))
                Save();

            if (ImGui.BeginPopupModal(NewDialogId, ref Constants.NullReference<bool>(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
                RenderNewDialog();

            if (ImGui.BeginPopupModal(DeleteDialogId, ref Constants.NullReference<bool>(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
                RenderDeleteDialog();
        }

        private void RenderDeleteDialog()
        {
            ImGui.Text("Taktikal Nook Inkoming!!\nThis operation cannot be undone!\n\n");
            ImGui.Spacing();

            if (ImGui.Button("OK", Constants.ButtonSize))
            {
                Delete();
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine(0, Constants.Spacing);
            if (ImGui.Button("Cancel", Constants.ButtonSize))
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        private void RenderNewDialog()
        {
            ImGui.Text("Name Your Creation!\n");
            ImGui.Spacing();

            ImGui.InputText("", _inputData.Pointer, (IntPtr) _inputData.SizeOfData, (int) ImGuiInputTextFlagsCallbackCharFilter, TextInputData.FilterValidPathCharacters, IntPtr.Zero);
            ImGui.Spacing();

            if (ImGui.Button("Create", Constants.ButtonSizeThin))
            {
                New(_inputData.Text);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine(0, Constants.Spacing);
            if (ImGui.Button("Cancel", Constants.ButtonSizeThin))
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }
}
