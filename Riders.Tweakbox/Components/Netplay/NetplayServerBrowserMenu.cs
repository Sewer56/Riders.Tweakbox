using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Refit;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Menus;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Controls;
using Sewer56.Imgui.Shell;

namespace Riders.Tweakbox.Components.Netplay
{
    /// <summary>
    /// Abstracts the functionality to power the server browser.
    /// </summary>
    public class NetplayServerBrowserMenu : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Server Browser";

        public ServerBrowserMenu BrowserMenu;
        public NetplayMenu Owner;
        
        public TweakboxApi Api              => Owner.Api;
        public NetplayController Controller => Owner.Controller;
        public NetplayEditorConfig     Config     => Owner.Config;

        // Re-route to internal window.
        public override ref bool IsEnabled() => ref BrowserMenu.IsEnabled();

        public NetplayServerBrowserMenu(NetplayMenu owner)
        {
            Owner = owner;
            BrowserMenu = new ServerBrowserMenu(Connect, Refresh);
        }

        public override void Render()
        {
            BrowserMenu.Render();
        }

        internal async Task Refresh()
        {
            Log.WriteLine("Fetching Server List");
            var serverResult = (await Api.BrowserApi.GetAll()).AsOneOf();

            if (serverResult.IsT1)
            {
                Shell.AddDialog("Failed to Get Server List", $"Error: \n{string.Join('\n', serverResult.AsT1.Errors)}");
                return;
            }

            var results = Mapping.Mapper.Map<List<GetServerResultEx>>(serverResult.AsT0.Results);
            BrowserMenu.SetResults(results);
            Log.WriteLine("Fetch Success");
        }

        internal async Task Connect(GetServerResultEx result)
        {
            try
            {
                string password = Config.Data.ClientSettings.Password;

                // Get Password if Lobby Defines one is Needed.
                if (result.HasPassword)
                {
                    // TODO: Query for Password
                    var inputData = new TextInputData(NetplayEditorConfig.TextLength);
                    await Shell.AddDialogAsync("Enter Password", (ref bool opened) =>
                    {
                        inputData.Render("Password", ImGuiInputTextFlags.ImGuiInputTextFlagsPassword);
                    });

                    password = inputData;
                }

                var configCopy = Mapping.Mapper.Map<NetplayEditorConfig>(Config); // Deep Copy
                var data = configCopy.Data;
                data.ClientSettings.Password = password;
                data.ClientSettings.Port = result.Port;
                data.ClientSettings.IP = result.Address;

                Controller.Socket = new Client(Config, Controller, Api);
                IsEnabled() = false;
            }
            catch (Exception e)
            {
                Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
