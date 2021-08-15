using System;
using System.Threading.Tasks;
using DearImguiSharp;
using Riders.Tweakbox.API.Application.Commands.v1.User;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;
using Sewer56.SonicRiders.API;
using Constants = Sewer56.Imgui.Misc.Constants;
namespace Riders.Tweakbox.Controllers;

/// <summary>
/// Owned by <see cref="Netplay"/>
/// </summary>
public class NetplayController : IController
{
    /// <summary>
    /// The current socket instance, either a <see cref="Client"/> or <see cref="Host"/>.
    /// </summary>
    public Socket Socket;

    /// <summary>
    /// The config associated with this controller.
    /// </summary>
    public NetplayEditorConfig Config { get; private set; }

    /// <summary>
    /// Provides access to the Tweakbox API.
    /// </summary>
    public TweakboxApi Api { get; set; }

    public NetplayController(NetplayEditorConfig config)
    {
        Config = config;
        Event.AfterEndScene += OnEndScene;
        Task.Run(InitializeApi);
    }


    /// <summary>
    /// True if Netplay Mode is currently Active
    /// </summary>
    public bool IsActive() => Socket != null;

    /// <summary>
    /// True is currently connected, else false.
    /// </summary>
    public bool IsConnected() => Socket != null && Socket.IsConnected();

    /// <summary>
    /// Creates the <see cref="Api"/> object for this class.
    /// </summary>
    public async Task InitializeApi()
    {
        Api = new TweakboxApi(Config.Data.ServerSettings.Host);

        // For convenience, silently try authenticate.
        var serverSettings = Config.Data.ServerSettings;
        if (!string.IsNullOrEmpty(serverSettings.Username) && !string.IsNullOrEmpty(serverSettings.Password))
        {
            Log.WriteLine("Silently Authenticating");
            var authResult = await Api.TryAuthenticate(serverSettings.Username, serverSettings.Password);
            Log.WriteLine(authResult.IsT1
                ? $"Failed to Silently Authenticate:\n{string.Join('\n', authResult.AsT1.Errors)}"
                : "Successful Silent Authenticate");
        }
    }

    /// <summary>
    /// Authenticates with the API.
    /// </summary>
    public async Task Authenticate()
    {
        var serverSettings = Config.Data.ServerSettings;
        if (!string.IsNullOrEmpty(serverSettings.Username) && !string.IsNullOrEmpty(serverSettings.Password))
        {
            var authResult = await Api.TryAuthenticate(serverSettings.Username, serverSettings.Password);
            if (authResult.IsT1)
                Shell.AddDialog("Failed to Authenticate", $"Here's what went wrong:\n{string.Join('\n', authResult.AsT1.Errors)}");
        }
        else
        {
            Shell.AddDialog("Login Failed", "Username and/or password was empty.");
        }
    }

    /// <summary>
    /// Registers a new account with the API.
    /// </summary>
    public async Task Register()
    {
        var serverSettings = Config.Data.ServerSettings;
        if (!string.IsNullOrEmpty(serverSettings.Username) &&
            !string.IsNullOrEmpty(serverSettings.Password) &&
            !string.IsNullOrEmpty(serverSettings.Email))
        {
            var authResult = (await Api.IdentityApi.Register(new UserRegistrationRequest()
            {
                Email = serverSettings.Email,
                UserName = serverSettings.Username,
                Password = serverSettings.Password,

            })).AsOneOf();

            if (authResult.IsT1)
                Shell.AddDialog("Registration Failed", $"Here's what went wrong:\n{string.Join('\n', authResult.AsT1.Errors)}");
            else
            {
                Shell.AddDialog("Successfully Registered", "Check your email for some extra nifty details!");
                await Authenticate();
            }
        }
        else
        {
            Shell.AddDialog("Register Failed", "Username, password and/or email were empty.");
        }
    }

    /// <summary>
    /// Allows you to connect to an arbitrary server without using the values
    /// from the default configuration.
    /// </summary>
    /// <param name="address">The IP Address</param>
    /// <param name="port">The Port</param>
    /// <param name="hasPassword">Whether the lobby has a password.</param>
    /// <returns></returns>
    public async Task<bool> ConnectAsync(string address, int port, bool hasPassword)
    {
        string password = String.Empty;

        // Get Password if Lobby Defines one is Needed.
        if (hasPassword)
        {
            // TODO: Query for Password
            var inputData = new TextInputData(NetplayEditorConfig.TextLength);
            await Shell.AddDialogAsync("Enter Password", (ref bool opened) =>
            {
                inputData.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                if (ImGui.Button("Ok", Constants.Zero))
                    opened = false;
            });

            password = inputData;
        }

        var configCopy = Mapping.Mapper.Map<NetplayEditorConfig>(Config); // Deep Copy
        var data = configCopy.Data;
        data.ClientSettings.Password = password;
        data.ClientSettings.Port = port;
        data.ClientSettings.IP = address;

        Socket = new Client(configCopy, this, Api);
        IoC.Get<NetplayMenu>().IsEnabled() = true;
        return true;
    }

    /// <summary>
    /// Updates on every frame of the game.
    /// </summary>
    private void OnEndScene()
    {
        if (Socket != null)
        {
            Socket.Update();
            Socket.OnFrame();
        }
    }
}
