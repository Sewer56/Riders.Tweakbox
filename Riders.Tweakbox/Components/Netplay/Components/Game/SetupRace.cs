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
                *State.NumberOfRacers = (byte)Socket.State.GetPlayerCount();

            if (Socket.TryGetComponent(out CharacterSelect charSelect))
                charSelect.LastSync.ToGameOnlyCharacter();

            if (Socket.TryGetComponent(out Attack attack))
                attack.Reset();

            if (Socket.TryGetComponent(out Race race))
                race.Reset();
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet) { }
    }
}
