using System;
using System.Threading.Tasks;
using DearImguiSharp;
using Riders.Netplay.Messages.Helpers.Interfaces;
using Riders.Tweakbox.API.Application.Commands.v1.User;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;
using Sewer56.Imgui.Shell.Interfaces;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayMenu : ComponentBase<NetplayEditorConfig>
    {
        public NetplayController Controller = IoC.Get<NetplayController>();
        public new NetplayEditorConfig Config => base.Config;
        public override string Name   { get; set; } = "Netplay Menu";
        
        // Sub-menus.
        public NetplayLobbyMenu LobbyMenu { get; set; }
        public NetplayServerBrowserMenu ServerBrowserMenu { get; set; }
        public TweakboxApi Api { get; private set; }

        private Task _loginTask    = Task.CompletedTask;
        private Task _registerTask = Task.CompletedTask;
        
        /// <inheritdoc />
        public NetplayMenu(IO io) : base(io, io.NetplayConfigFolder, io.GetNetplayConfigFiles, IO.JsonConfigExtension)
        {
            LobbyMenu = new NetplayLobbyMenu(this);
            ServerBrowserMenu = new NetplayServerBrowserMenu(this);
            Task.Run(CreateApiObject);
        }

        public override void Render()
        {
            // All menus decide if they should render in their respective render function(s).
            LobbyMenu.Render();
            ServerBrowserMenu.Render();

            if (Controller.Socket != null)
                return;

            if (ImGui.Begin(Name, ref IsEnabled(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
                RenderMainMenu();

            ImGui.End();
        }

        private unsafe void RenderMainMenu()
        {
            ProfileSelector.Render();
            ref var data = ref Config.Data;

            RenderAccountNode(data);
            RenderJoinByIpNode(data);
            RenderHostServerNode(data);

            if (ImGui.TreeNodeStr("Settings"))
            {
                RenderPlayerSettingsNode(data);
                RenderNatPunchSettingsNode(data);
                RenderCentralServerSettingsNode(data);

                ImGui.TreePop();
            }

            LobbyMenu.RenderDebugOptions();
            ImGui.Spacing();
        }

        private void RenderAccountNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("Account")) 
                return;

            ref var serverSettings = ref data.ServerSettings;
            if (ImGui.TreeNodeStr("Login"))
            {
                serverSettings.Username.Render("Username");
                serverSettings.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);

                if (Api.IsAuthenticated)
                {
                    ImGui.TextWrapped("You are currently signed in.");
                    if (ImGui.Button("Logout", Constants.ButtonSize))
                        Api.SignOut();
                }
                else
                {
                    if (_loginTask.IsCompleted)
                    {
                        if (ImGui.Button("Login", Constants.ButtonSize))
                            _loginTask = Task.Run(Authenticate);
                    }
                    else
                    {
                        ImGui.TextWrapped("Working On It");
                    }
                }

                ImGui.TreePop();
            }
            else if (ImGui.TreeNodeStr("Register"))
            {
                serverSettings.Email.Render("Email");
                serverSettings.Username.Render("Username");
                serverSettings.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);

                if (_registerTask.IsCompleted)
                {
                    if (ImGui.Button("Register", Constants.ButtonSize))
                        _registerTask = Task.Run(Register);
                }
                else
                {
                    ImGui.TextWrapped("Working On It");
                }
                
                ImGui.TreePop();
            }

            ImGui.TextWrapped("Registering an account allows you to participate in ranked matches " +
                              "as well as being able to track your individual races.");

            ImGui.TextWrapped("Currently this functionality is not yet implemented.");
            ImGui.TreePop();
        }

        private void RenderHostServerNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("Host Server")) 
                return;

            ref var hostData = ref data.HostSettings;

            hostData.Name.Render("Server Name");
            hostData.SocketSettings.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
            ImGui.DragInt("Port", ref hostData.SocketSettings.Port, 0.1f, 0, ushort.MaxValue, null, 0);

            ImGui.Checkbox("Reduced Non-Essential Tick Rate", ref hostData.ReducedTickRate);
            Tooltip.TextOnHover("Only use when hosting 8 player lobby and upload speed is less than 1Mbit/s.\n" +
                                "Reduces the send-rate of non-essential elements such as players' amount of air, rings, flags and other misc. content.");

            if (ImGui.Button("Host", Constants.DefaultVector2))
                HostServer();

            ImGui.TreePop();
        }

        private void RenderJoinByIpNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("Join Game")) 
                return;

            if (!ServerBrowserMenu.IsEnabled())
            {
                ImGui.TextWrapped("Find existing games using the online service:");
                if (ImGui.Button("Open Server Browser", Constants.Zero))
                {
                    ServerBrowserMenu.IsEnabled() = true;
                    Task.Run(() => ServerBrowserMenu.Refresh());
                    return;
                }
            }

            ref var clientData = ref data.ClientSettings;
            ImGui.TextWrapped("Alternatively, you can join by Direct IP:");
            clientData.IP.Render("IP Address", ImGuiInputTextFlags.ImGuiInputTextFlagsCallbackCharFilter, clientData.IP.FilterIPAddress);
            clientData.Password.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
            ImGui.DragInt("Port", ref clientData.Port, 0.1f, 0, ushort.MaxValue, null, 0);

            if (ImGui.Button("Connect", Constants.DefaultVector2))
                Connect();

            ImGui.TreePop();
        }

        private void RenderCentralServerSettingsNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("Central Server Settings"))
                return;

            ref var serverSettings = ref data.ServerSettings;
            serverSettings.Host.Render("Central Server");
            Tooltip.TextOnHover("Address of the central server used for server browser, account system and matchmaking.");

            if (ImGui.Button("Apply", Constants.ButtonSize))
                Task.Run(CreateApiObject);

            ImGui.TreePop();
        }

        private static void RenderNatPunchSettingsNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("NAT Punch/Traversal Server")) 
                return;

            ref var punchingServer = ref data.PunchingServer;
            Reflection.MakeControl(ref punchingServer.IsEnabled, "Enabled");
            Tooltip.TextOnHover("Uses a third party server to try to bypass your router's firewall to establish a connection.\nHighly likely but not guaranteed to work. Use if you are unable or do not know how to port forward.");

            if (punchingServer.IsEnabled)
            {
                punchingServer.Host.Render("Host Server");
                ImGui.DragInt("Port", ref punchingServer.Port, 0.1f, 0, ushort.MaxValue, null, 0);

                ImGui.DragInt("Server Timeout", ref punchingServer.ServerTimeout, 0.1f, 0, ushort.MaxValue, null, 0);
                Tooltip.TextOnHover("(Milliseconds) Timeout for connecting to the third party server.");

                ImGui.DragInt("Punch Timeout", ref punchingServer.PunchTimeout, 0.1f, 0, ushort.MaxValue, null, 0);
                Tooltip.TextOnHover("(Milliseconds) Timeout for trying to hole punch past the server firewall.\nIf you have trouble issues connecting, increasing this value might help.");
            }

            ImGui.TreePop();
        }

        private static unsafe void RenderPlayerSettingsNode(NetplayEditorConfig.Internal data)
        {
            if (!ImGui.TreeNodeStr("Player Settings")) 
                return;

            ref var playerSettings = ref data.PlayerSettings;
            playerSettings.PlayerName.Render(nameof(playerSettings.PlayerName));

            ImGui.DragInt("Number of Players", ref playerSettings.LocalPlayers, 0.1f, 0,
                Riders.Netplay.Messages.Misc.Constants.MaxNumberOfLocalPlayers, null, 0);
            Tooltip.TextOnHover("Default: 1\n" +
                                "Number of local players playing online.\n" +
                                "Setting this value to 0 makes you a spectator.");

            ImGui.DragInt("Max Number of Cameras", ref playerSettings.MaxNumberOfCameras, 0.1f, 0,
                Riders.Netplay.Messages.Misc.Constants.MaxNumberOfLocalPlayers, null, 0);
            Tooltip.TextOnHover(
                "Overrides the number of cameras, allowing you to spectate other players while in online multiplayer.\n" +
                "0 = Automatic\n" +
                "1 = Single Screen\n" +
                "2 = Split-Screen\n" +
                "3-4 = 4-way Split Screen.");

            bool openBufferSettings = ImGui.TreeNodeStr("Buffer Settings");
            Tooltip.TextOnHover("Advanced users only. Changing the defaults is not recommended.");
            if (openBufferSettings)
            {
                var bufferSettings = playerSettings.BufferSettings;
                fixed (JitterBufferType* type = &bufferSettings.Type)
                {
                    Reflection.MakeControlEnum(type, "Jitter Buffer Type");
                    Tooltip.TextOnHover("Sets the buffer implementation used to smoothen out other players.\n" +
                                        "Default: Smoothness 5*, Delay 1*.\n" +
                                        "Adaptive: Smoothness 3*. Delay 5*.  Suffers on ping spikes.\n" +
                                        "Hybrid: Smoothness 4.5*. Delay 4*. Recommended.");
                }

                Reflection.MakeControl(ref bufferSettings.DefaultBufferSize, "Default Buffer Size");
                Tooltip.TextOnHover("This value adjusts automatically during gameplay. This is just the \"safe\" starting value.");

                Reflection.MakeControl(ref bufferSettings.NumJitterValuesSample, "Number of Samples");
                Tooltip.TextOnHover("Number of samples before updating the buffer size during gameplay. Values of at least 90 are recommended.");

                if (bufferSettings.Type == JitterBufferType.Adaptive)
                    Reflection.MakeControl(ref bufferSettings.MaxRampDownAmount, "Max Ramp Down Amount");

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        private async Task Register()
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

        private async Task Authenticate()
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

        private async Task CreateApiObject()
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

        private void HostServer()
        {
            try
            {
                Controller.Socket = new Host(Config, Controller, Api);
                ServerBrowserMenu.IsEnabled() = false;
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
                Controller.Socket = new Client(Config, Controller, Api);
                ServerBrowserMenu.IsEnabled() = false;
            }
            catch (Exception e)
            {
                Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
