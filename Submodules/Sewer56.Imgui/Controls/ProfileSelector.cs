using System;
using System.IO;
using System.Linq;
using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Utilities;
using static DearImguiSharp.ImGuiInputTextFlags;
using static Sewer56.Imgui.Utilities.FileSystemWatcherFactory.FileSystemWatcherEvents;
namespace Sewer56.Imgui.Controls;

/// <summary>
/// Represents a profile selector widget, which actively monitors all available config files and allows the user to select one.
/// </summary>
public unsafe class ProfileSelector
{
    private const string NewDialogId = "New Profile";
    private const string DeleteDialogId = "Delete?";

    /// <summary>
    /// Contains the path to the config directory.
    /// </summary>
    public string Directory { get; private set; }

    /// <summary>
    /// Contains the directory to the current configuration.
    /// </summary>
    public string CurrentConfiguration { get; private set; }

    /// <summary>
    /// Gets the path of the default configuration.
    /// </summary>
    public string DefaultConfiguration => $"{Directory}/Default{_configExtension}";

    /// <summary>
    /// List of all available configuration files.
    /// </summary>
    public string[] Configurations { get; private set; }

    private TextInputData _inputData = new TextInputData(48);
    private byte[] _newConfigBytes;
    private Func<string[]> _getConfigFiles;
    private Func<byte[]> _getCurrentConfigBytes;
    private Action<byte[]> _loadConfig;

    private FileSystemWatcher _configWatcher;
    private string _configExtension;

    /// <summary>
    /// Represents a profile selector widget.
    /// </summary>
    /// <param name="directory">The directory for which to load/save profiles.</param>
    /// <param name="getConfigFiles">A function that obtains all config file names.</param>
    /// <param name="loadConfig">Executed when a new configuration is to be read. Parameter is config data.</param>
    /// <param name="extension">Name of the configuration extension.</param>
    /// <param name="newConfigBytes">The bytes used for a new configuration, typically the default configuration.</param>
    /// <param name="getCurrentConfigBytes">Gets the bytes for the current configuration.</param>
    public ProfileSelector(string directory, string extension, byte[] newConfigBytes, Func<string[]> getConfigFiles, Action<byte[]> loadConfig, Func<byte[]> getCurrentConfigBytes)
    {
        Directory = directory;
        CurrentConfiguration = $"{Directory}/Default{extension}";

        _getConfigFiles = getConfigFiles;
        _loadConfig = loadConfig;
        _newConfigBytes = newConfigBytes;
        _getCurrentConfigBytes = getCurrentConfigBytes;
        _configExtension = extension;

        _configWatcher = FileSystemWatcherFactory.CreateGeneric(Directory, OnConfigsUpdated, Changed | Created | Deleted | Renamed, true, $"*{_configExtension}");
        if (!File.Exists(CurrentConfiguration))
            File.WriteAllBytes(CurrentConfiguration, newConfigBytes);

        OnConfigsUpdated();
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
        var path = $"{Directory}/{name}{_configExtension}";
        File.WriteAllBytes(path, data);
        _loadConfig(data);
        CurrentConfiguration = path;
    }

    /// <summary>
    /// Saves the current configuration.
    /// </summary>
    public void Save() => Save(_getCurrentConfigBytes(), CurrentConfiguration);

    /// <summary>
    /// Saves the current configuration with some custom data.
    /// </summary>
    public void Save(byte[] data, string path) => File.WriteAllBytes(CurrentConfiguration, data);

    /// <summary>
    /// Deletes the current configuration if it exists.
    /// </summary>
    public void Delete()
    {
        if (File.Exists(CurrentConfiguration))
        {
            File.Delete(CurrentConfiguration);
            OnConfigsUpdated();
        }
    }

    /// <summary>
    /// Renders the UI control to the screen.
    /// </summary>
    public void Render()
    {
        ImGui.TextWrapped("Profile Selector");
        var currentConfigName = Path.GetFileName(CurrentConfiguration);
        var currentConfigNames = Configurations.Select(Path.GetFileName).ToArray();

        Reflection.MakeControlComboBox("Current Profile", currentConfigName, currentConfigName, currentConfigNames, currentConfigNames,
            x =>
            {
                CurrentConfiguration = Configurations[currentConfigNames.IndexOf(y => y == x)];
            });

        if (ImGui.Button("New", Constants.DefaultVector2))
            ImGui.OpenPopupStr(NewDialogId, 0);

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Delete", Constants.DefaultVector2))
            ImGui.OpenPopupStr(DeleteDialogId, 0);

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Load", Constants.DefaultVector2))
        {
            if (!string.IsNullOrEmpty(CurrentConfiguration) && File.Exists(CurrentConfiguration))
                _loadConfig(File.ReadAllBytes(CurrentConfiguration));
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Save", Constants.DefaultVector2))
            Save();

        ImGui.SameLine(0, Constants.Spacing);
        ImGui.SeparatorEx((int)ImGuiSeparatorFlags.ImGuiSeparatorFlagsVertical);
        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Reset", Constants.DefaultVector2))
            _loadConfig(_newConfigBytes);

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Save as Default", Constants.DefaultVector2))
            Save(_getCurrentConfigBytes(), DefaultConfiguration);

        if (ImGui.BeginPopupModal(NewDialogId, ref Constants.NullReference<bool>(), (int)ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            RenderNewDialog();

        if (ImGui.BeginPopupModal(DeleteDialogId, ref Constants.NullReference<bool>(), (int)ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
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

        ImGui.InputText("", _inputData.Pointer, (IntPtr)_inputData.SizeOfData, (int)ImGuiInputTextFlagsCallbackCharFilter, _inputData.FilterValidPathCharacters, IntPtr.Zero);
        ImGui.Spacing();

        if (ImGui.Button("Create", Constants.ButtonSizeThin))
        {
            New(_inputData);
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine(0, Constants.Spacing);
        if (ImGui.Button("Cancel", Constants.ButtonSizeThin))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private void OnConfigsUpdated()
    {
        Configurations = _getConfigFiles();
        if (string.IsNullOrEmpty(CurrentConfiguration) || !File.Exists(CurrentConfiguration))
            CurrentConfiguration = Configurations.FirstOrDefault();
    }
}
