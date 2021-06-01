using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Menus;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Riders.Tweakbox.Configs;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
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
        
        public TweakboxApi Api              => Controller.Api;
        public NetplayController Controller => Owner.Controller;
        public NetplayEditorConfig  Config  => Owner.Config;

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
                await Controller.ConnectAsync(result.Address, result.Port, result.HasPassword);
                IsEnabled() = false;
            }
            catch (Exception e)
            {
                Shell.AddDialog("Join Server Failed", $"{e.Message}\n{e.StackTrace}");
            }
        }
    }
}
