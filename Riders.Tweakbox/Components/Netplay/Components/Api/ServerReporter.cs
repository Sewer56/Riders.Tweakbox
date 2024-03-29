﻿using System;
using System.Collections.Generic;
using System.Threading;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Struct;
using Riders.Tweakbox.API.Application.Commands.v1;
using Riders.Tweakbox.API.Application.Commands.v1.Browser;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.API.SDK;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc.Log;
using Sewer56.SonicRiders.Structures.Enums;
using TweakboxApi = Riders.Tweakbox.Api.TweakboxApi;

namespace Riders.Tweakbox.Components.Netplay.Components.Api;

/// <summary>
/// Handles all server reporting to the API.
/// </summary>
public class ServerReporter : INetplayComponent
{
    /// <inheritdoc />
    public Socket Socket { get; set; }
    private Guid _guid;
    private Timer _timer;
    private int _refreshTimeSeconds = 60;
    private TweakboxApi _tweakboxApi;

    public unsafe ServerReporter(Socket socket, TweakboxApi api)
    {
        Socket = socket;
        _tweakboxApi = api;

        if (Socket.GetSocketType() != SocketType.Host)
            return;

        EventController.OnExitCourseSelect += OnExitCourseSelect;
        EventController.OnEnterCharacterSelect += OnEnterCharaSelect;
        EventController.OnExitCharacterSelect += OnExitCharacterSelect;
        _timer = new Timer(UpdateServerDetails, null, TimeSpan.Zero, TimeSpan.FromSeconds(_refreshTimeSeconds));
    }

    /// <inheritdoc />
    public unsafe void Dispose()
    {
        if (Socket.GetSocketType() != SocketType.Host)
            return;

        EventController.OnExitCourseSelect -= OnExitCourseSelect;
        EventController.OnEnterCharacterSelect -= OnEnterCharaSelect;
        EventController.OnExitCharacterSelect -= OnExitCharacterSelect;
        _timer?.Dispose();
        RemoveServer();
    }

    private void OnExitCharacterSelect() => PostServerRequest(true);
    private void UpdateServerDetails(object? state) => PostServerRequest();
    private async void OnEnterCharaSelect() => RemoveServer();
    private void OnExitCourseSelect() => RemoveServer();

    private async void RemoveServer()
    {
        var config = Socket.Config.Data;
        await Socket.Api.BrowserApi.Delete(_guid, config.HostSettings.Port);
    }

    private async void PostServerRequest(bool force = false)
    {
        if (!force && !Socket.CanJoin)
            return;

        var state = Socket.State;
        var players = new List<ServerPlayerInfoResult>();
        players.AddRange(GetServerInfoFromClient(state.SelfInfo));

        foreach (var player in state.ClientInfo)
            players.AddRange(GetServerInfoFromClient(player));

        var hostSettings = Socket.Config.Data.HostSettings;
        var request = new PostServerRequest()
        {
            Name = hostSettings.Name,
            HasPassword = hostSettings.Password.Text.Length > 0,
            Type = MatchTypeDto.Default, // TODO: Add Other GameModes
            GameMode = GetMode(),
            Port = hostSettings.Port,
            Players = players
        };

        request.SetMods(_tweakboxApi.LoadedMods.ToArray());

        var result = (await Socket.Api.BrowserApi.CreateOrRefresh(request)).AsOneOf();
        if (result.IsT1)
        {
            Log.WriteLine($"Uploading Lobby Data failed for Some Reason: {String.Join('\n', result.AsT1.Errors)}");
        }
        else
        {
            _guid = result.AsT0.Id;
            Socket.HostIp = result.AsT0.Address;
        }
    }

    private unsafe GameModeDto GetMode() => GetMode(Socket.Event.TitleSequence->TaskData->RaceMode);
    private GameModeDto GetMode(RaceMode raceMode)
    {
        return raceMode switch
        {
            RaceMode.FreeRace => GameModeDto.NormalRace,
            RaceMode.TimeTrial => GameModeDto.TimeTrial,
            RaceMode.GrandPrix => GameModeDto.GrandPrix,
            RaceMode.RaceStage => GameModeDto.SurvivalRace,
            RaceMode.BattleStage => GameModeDto.SurvivalBattle,
            RaceMode.TagMode => GameModeDto.Tag,
            _ => throw new ArgumentOutOfRangeException(nameof(raceMode), raceMode, null)
        };
    }

    private List<ServerPlayerInfoResult> GetServerInfoFromClient(ClientData playerData)
    {
        var result = new List<ServerPlayerInfoResult>();

        if (playerData.NumPlayers > 1)
        {
            for (int x = 0; x < playerData.NumPlayers; x++)
            {
                var item = new ServerPlayerInfoResult()
                {
                    Latency = playerData.Latency,
                    Name = $"{playerData.Name}[{x}]"
                };

                result.Add(item);
            }
        }
        else if (playerData.NumPlayers == 1)
        {
            var item = new ServerPlayerInfoResult()
            {
                Latency = playerData.Latency,
                Name = playerData.Name
            };

            result.Add(item);
        }

        return result;
    }

    /// <inheritdoc />
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

    /// <inheritdoc />
    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
