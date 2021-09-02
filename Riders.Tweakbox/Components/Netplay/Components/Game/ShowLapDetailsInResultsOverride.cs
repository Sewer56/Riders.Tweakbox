using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct;
using Riders.Tweakbox.Components.Netplay.Components.Game.Structs;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Components.Netplay.Components.Game;

public class ShowLapDetailsInResultsOverride : INetplayComponent
{
    public Socket Socket { get; set; }

    public CommonState State { get; set; }

    /// <summary>
    /// [Host] Contains lap data for all the players.
    /// </summary>
    private LapCounter[] _lapSync = new LapCounter[Constants.MaxNumberOfPlayers];

    private PauseDialogOverride _dialogOverride;
    private RaceLapSync _lapSyncComponent;

    public ShowLapDetailsInResultsOverride(Socket socket)
    {
        Socket = socket;
        State = socket.State;
        if (Socket.TryGetComponent(out _dialogOverride))
            _dialogOverride.AdditionalResultsItems += AddThingsToResults;

        if (Socket.TryGetComponent(out _lapSyncComponent))
            _lapSyncComponent.OnUpdateLocalLapCounter += OnSetLocalLapCounter;
    }

    public void Dispose()
    {
        if (_dialogOverride != null)
            _dialogOverride.AdditionalResultsItems -= AddThingsToResults;

        if (_lapSyncComponent != null)
            _lapSyncComponent.OnUpdateLocalLapCounter -= OnSetLocalLapCounter;
    }

    private void OnSetLocalLapCounter(int playerindex, in LapCounter counter) => _lapSync[playerindex] = counter;

    private unsafe void AddThingsToResults()
    {
        // Copy lap data.
        var numPlayers = *Sewer56.SonicRiders.API.State.NumberOfRacers;
        Span<IndexedLapCounter> counters = stackalloc IndexedLapCounter[numPlayers];
        for (int x = 0; x < numPlayers; x++)
            counters[x] = new IndexedLapCounter(x, _lapSync[x]);

        // Sort by time and l ap.
        counters.Sort((x, y) =>
        {
            if (x.Counter.Counter != y.Counter.Counter)
                return x.Counter.Counter.CompareTo(y.Counter.Counter);
            
            return x.Counter.Timer > y.Counter.Timer ? 1 : 0;
        });

        // Display
        for (var x = 0; x < counters.Length; x++)
        {
            var counter = counters[x];
            var timer   = (Timer)(counter.Counter.Timer);
            var client  = State.GetClientInfo(counter.Index, out int offset);
            if (client.NumPlayers > 1)
                ImGui.Text($"{x}. {client.Name}({offset}) - {timer.Minutes:00}:{timer.Seconds:00}:{timer.Milliseconds:00}");
            else
                ImGui.Text($"{x}. {client.Name} - {timer.Minutes:00}:{timer.Seconds:00}:{timer.Milliseconds:00}");
        }
    }

    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
    {
        // Check message type.
        if (packet.MessageType != MessageType.LapCounters)
            return;

        // Get message.
        var lapCounters = packet.GetMessage<LapCountersPacked>();

        // Index of first player to fill.
        int playerIndex = Socket.GetSocketType() switch
        {
            SocketType.Host => ((HostState)State).ClientMap.GetPlayerData(source).PlayerIndex,
            SocketType.Client => State.NumLocalPlayers,
            _ => throw new ArgumentOutOfRangeException()
        };

        var counters = lapCounters.Elements;
        var numCounters = lapCounters.NumElements;

        for (int x = 0; x < numCounters; x++)
            _lapSync[playerIndex + x] = counters[x];
    }

    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}
