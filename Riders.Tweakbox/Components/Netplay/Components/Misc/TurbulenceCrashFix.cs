using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using System;
using Sewer56.NumberUtilities.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;
using Sewer56.Hooks.Utilities.Enums;
using Riders.Tweakbox.Controllers;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc;

public unsafe class TurbulenceCrashFix : INetplayComponent
{
    public Socket Socket { get; set; }

    public TurbulenceCrashFix(Socket socket)
    {
        Socket = socket;
        EventController.CheckIfSkipTurbChecks += CheckIfSkipTurbChecks;
    }

    public void Dispose()
    {
        EventController.CheckIfSkipTurbChecks -= CheckIfSkipTurbChecks;
    }

    private unsafe Enum<AsmFunctionResult> CheckIfSkipTurbChecks(Player* player)
    {
        var playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndex(player);
        return !Socket.State.IsLocal(playerIndex);
    }

    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
