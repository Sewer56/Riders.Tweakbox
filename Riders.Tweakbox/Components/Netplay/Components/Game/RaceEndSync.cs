using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteNetLib;
using Reloaded.Hooks.Definitions;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Gameplay;
using static Sewer56.SonicRiders.API.Player;
using static Sewer56.SonicRiders.API.State;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    /// <summary>
    /// Attacks, box pickups, etc.
    /// </summary>
    public unsafe class RaceEndSync : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket            { get; set; }
        public EventController Event    { get; set; }
        public CommonState State        { get; set; }
        
        public RaceEndSync(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;
            State  = socket.State;

            Event.SetGoalRaceFinishTask += OnSetGoalRaceFinishTask;
            Sewer56.SonicRiders.API.Event.AfterSleep += CheckIfAllFinishedRace;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.SetGoalRaceFinishTask -= OnSetGoalRaceFinishTask;
            Sewer56.SonicRiders.API.Event.AfterSleep -= CheckIfAllFinishedRace;
        }

        /* Implementation */
        private unsafe int OnSetGoalRaceFinishTask(IHook<Functions.SetGoalRaceFinishTaskFn> hook, Player* player)
        {
            // Suppress task creation one player finished race.
            if (player->LapCounter > CurrentRaceSettings->Laps)
                return 0;

            return hook.OriginalFunction(player);
        }

        private void CheckIfAllFinishedRace()
        {
            // Set goal race finish task if all players finished racing.
            var allPlayersFinished = Enumerable.Range(0, Constants.MaxNumberOfPlayers)
                                               .Where(x => State.IsHuman(x))
                                               .All(x => Players[x].LapCounter > CurrentRaceSettings->Laps);

            // TODO: Local Multiplayer support.
            if (allPlayersFinished)
                Event.InvokeSetGoalRaceFinishTask(Players.Pointer);
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet) { }
    }
}
