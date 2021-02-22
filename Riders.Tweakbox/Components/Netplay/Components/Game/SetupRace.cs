using System;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using CharacterSelect = Riders.Tweakbox.Components.Netplay.Components.Menu.CharacterSelect;

namespace Riders.Tweakbox.Components.Netplay.Components.Game
{
    public unsafe class SetupRace : INetplayComponent
    {
        private static Patch _disableGrandPrixOverridePlayerCount = new Patch((IntPtr)0x0050BC34, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        
        public SetupRace(Socket socket, EventController @event)
        {
            Socket = socket;
            Event  = @event;

            Event.OnSetupRace += OnSetupRace;
            _disableGrandPrixOverridePlayerCount.Enable();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.OnSetupRace -= OnSetupRace;
            _disableGrandPrixOverridePlayerCount.Disable();
        }

        private unsafe void OnSetupRace(Task<TitleSequence, TitleSequenceTaskState>* task)
        {
            if (task->TaskData->RaceMode != RaceMode.TagMode)
                *State.NumberOfRacers = Socket.State.GetPlayerCount();

            if (Socket.TryGetComponent(out CharacterSelect charSelect))
                charSelect.LastSync.ToGameOnlyCharacter(Socket.State.NumLocalPlayers);

            if (Socket.TryGetComponent(out Attack attack))
                attack.Reset();

            if (Socket.TryGetComponent(out Race race))
                race.Reset();

            if (Socket.TryGetComponent(out RacePlayerEventSync raceEvent))
                raceEvent.Reset();

            if (Socket.TryGetComponent(out RaceLapSync lap))
                lap.Reset();

            // Reset number of frames.
            Socket.State.FrameCounter = 0;

            // Calculate Number of Cameras depending on Local Players
            var totalPlayers = Socket.State.GetPlayerCount();
            var numCameras   = Socket.Config.Data.MaxNumberOfCameras.Value;
            if (numCameras > 0)
            {
                while (numCameras > totalPlayers)
                    numCameras--;
            }
            else
            {
                // 1 Camera or Local Num of Players
                numCameras = Math.Max(1, Socket.State.NumLocalPlayers);
            }

            *State.NumberOfCameras = numCameras;
            if (numCameras > 1)
                *State.HasMoreThanOneCamera = 1;
            else
                *State.HasMoreThanOneCamera = 0;

            *State.NumberOfHumanRacers = totalPlayers;
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
