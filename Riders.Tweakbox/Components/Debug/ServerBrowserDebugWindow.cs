using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using DearImguiSharp;
using MapsterMapper;
using Riders.Tweakbox.API.Application.Commands.v1;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.API.Application.Commands.v1.User;
using Riders.Tweakbox.Components.Netplay.Menus;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Shell;
using Constants = Sewer56.Imgui.Misc.Constants;

namespace Riders.Tweakbox.Components.Debug
{
    public class ServerBrowserDebugWindow : ComponentBase
    {
        /// <inheritdoc />
        public override string Name { get; set; } = "Server Browser Test Window";

        public ServerBrowserMenu BrowserMenu;
        public int NumServers = 50;

        /// <inheritdoc />
        public ServerBrowserDebugWindow()
        {
            BrowserMenu = new ServerBrowserMenu(FakeConnect, FakeRefresh);
            GenerateData(NumServers);
            BrowserMenu.IsEnabled() = true;
        }

        /// <inheritdoc />
        public override void Render()
        {
            ImGui.PushItemWidth(ImGui.GetFontSize() * - 12);
            if (ImGui.Begin(Name, ref IsEnabled(), (int) ImGuiWindowFlags.ImGuiWindowFlagsAlwaysAutoResize))
            {
                BrowserMenu.Render();

                ImGui.SliderInt("Number of Servers", ref NumServers, 1, 1000, null, 0);
                if (ImGui.Button("Generate", Constants.ButtonSize))
                    GenerateData(NumServers);

                if (!BrowserMenu.IsEnabled() && ImGui.Button("Show Menu", Constants.ButtonSize))
                    BrowserMenu.IsEnabled() = true;
            }

            ImGui.End();
            ImGui.PopItemWidth();
        }

        private void GenerateData(int numServers)
        {
            var faker = GetPostServerRequestFaker();
            BrowserMenu.SetResults(Mapping.Mapper.Map<List<GetServerResultEx>>(faker.Generate(numServers)));
        }

        // Sample Events
        private Task FakeConnect(GetServerResultEx arg)
        {
            Shell.AddDialog("Connecting", "Just try to pretend this code is\n" +
                                          "connecting to a server, okay?");
            return Task.CompletedTask;
        }

        private Task FakeRefresh()
        {
            return Task.Run(() => GenerateData(NumServers));
        }

        #region Test Data Generation
        private static Random _random = new Random();
        private static Faker<GetServerResult> GetPostServerRequestFaker()
        {
            return new Faker<GetServerResult>()
                .StrictMode(true)
                .RuleFor(x => x.Port, x => x.Random.Int(0, 65535))
                .RuleFor(x => x.Name, x => x.Internet.UserName() + "'s Game")
                .RuleFor(x => x.HasPassword, x => x.Random.Bool(0.5f))
                .RuleFor(x => x.Type, x => x.PickRandom<MatchTypeDto>())
                .RuleFor(x => x.Country, x => x.PickRandom<CountryDto>())
                .RuleFor(x => x.Mods, GetRandomModString)
                .RuleFor(x => x.Address, x => x.Internet.IpAddress().ToString())
                .RuleFor(x => x.Players, MakeValidPlayerData);
        }

        private static List<ServerPlayerInfoResult> MakeValidPlayerData(Faker faker, GetServerResult request)
        {
            var playerFaker = GetPlayerInfoResult();
            var numPlayers  = faker.Random.Int(0, request.Type.GetNumTeams() * request.Type.GetNumPlayersPerTeam());
            var result      = new List<ServerPlayerInfoResult>();

            for (int x = 0; x < numPlayers; x++)
                result.Add(playerFaker.Generate());

            return result;
        }

        private static Faker<ServerPlayerInfoResult> GetPlayerInfoResult()
        {
            return new Faker<ServerPlayerInfoResult>()
                .StrictMode(true)
                .RuleFor(x => x.Name, x => x.Internet.UserName())
                .RuleFor(x => x.Latency, x => x.Random.Int(0, 180));
        }

        private static string[] _sampleMods =
        {
            "ToiletEdition1.0.0",
            "ToiletEdition1.1.0",
            "ToiletEdition2.0.0",
            "TE1.2.5",
            "TE1.3.0",
            "TE1.4.0",
            "DX1.0.0",
            "DX1.0.1",
            "DX2.0.0",
            "DX3.0.0",
        };

        private static string GetRandomModString()
        {
            var number = _random.Next(0, 3);
            var uniqueIntegers = GetUniqueIntegers(0, _sampleMods.Length, number);
            var builder = new StringBuilder(128);

            for (int x = 0; x < number; x++)
            {
                var index = uniqueIntegers[x];
                builder.Append(_sampleMods[index]);

                if (x != number - 1)
                    builder.Append(GetServerResult.ModsDelimiter);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets a list of unique integers between the specified min and max value.
        /// Note: Optimized for tiny collections only.
        /// </summary>
        /// <param name="min">Inclusive</param>
        /// <param name="max">Exclusive</param>
        /// <param name="number">Number of IDs to generate.</param>
        private static List<int> GetUniqueIntegers(int min, int max, int number)
        {
            var ids = new List<int>(number);
            while (ids.Count < number)
            {
                var random = _random.Next(min, max);
                if (!ids.Contains(random))
                    ids.Add(random);
            }

            return ids;
        }
        #endregion
    }
}
