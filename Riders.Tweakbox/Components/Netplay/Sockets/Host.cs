using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Server;
using Riders.Netplay.Messages.Reliable.Structs.Server.Messages;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;
using Riders.Tweakbox.Components.Netplay.Sockets.Components;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        public string Password { get; private set; }
        public TaskTracker Tracker { get; private set; } = IoC.GetConstant<TaskTracker>();
        public PlayerMap<PlayerState> PlayerMap { get; private set; } = new PlayerMap<PlayerState>();
        private int _frameCounter = 0;

        public Host(int port, string password)
        {
            Debug.WriteLine($"[Tweakbox] Hosting Server on {port} with password {password}");

            // TODO: Implement Connection
            Password = password;
            Manager.Start(port);
        }

        public override unsafe void Dispose()
        {
            Tracker.TitleSequence->TaskData->Colour = MenuColour.Blue;
            Manager.DisconnectAll();
            base.Dispose();
        }

        public override bool IsHost() => true;
        public override void Update()
        {
            _frameCounter += 1;
            switch (Tracker.LastTask)
            {
                case Tasks.CharacterSelect:
                    UpdateCharacterSelect();
                    break;
                case Tasks.CourseSelect:
                    UpdateCourseSelect();
                    break;
                case Tasks.RaceRules:
                    UpdateRaceRules();
                    break;
                case Tasks.Race:
                    UpdateRace();
                    break;
            }
        }

        private void UpdateRace()
        {

        }

        private void UpdateCharacterSelect()
        {
            var task = Tracker.CharacterSelect;
            var data = PlayerMap.GetData();

            // Get All Chara Loops in Player Order
            var allCharaLoops = new CharaSelectLoop[Constants.MaxNumberOfPlayers];
            allCharaLoops[0]  = CharaSelectLoop.FromGame(task);

            foreach (var dat in data) 
                allCharaLoops[dat.Host.PlayerIndex] = dat.Custom.GetCharaLoop();

            // Apply Sync Message to Self
            var charaLoopSpan = allCharaLoops.AsSpan();
            new CharaSelectSync(charaLoopSpan.Slice(1).ToArray()).ToGame(task);

            // Send personalized message to each peer with everyone except them.
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var excludeIndex = PlayerMap.GetPlayerData(peer).PlayerIndex;
                var selectSync  = new CharaSelectSync(allCharaLoops.Where((loop, x) => x != excludeIndex).ToArray());
                var syncMessage = new MenuSynchronizationCommand(selectSync);
                var message     = new ReliablePacket() { MenuSynchronizationCommand = syncMessage };
                peer.Send(message.Serialize(), DeliveryMethod.ReliableOrdered);
            }
        }

        private void UpdateRaceRules()
        {
            var task = Tracker.RaceRules;

            // Calculate Sync Data based off of Client Info
            var allData = PlayerMap.GetCustomData();
            var loop = new RuleSettingsLoop();
            foreach (var data in allData)
                loop = loop.Add(data.GetRuleSettingsLoop());

            var sync = RuleSettingsSync.FromGame(task);
            sync.Merge(loop);
            sync.ToGame(task);

            // Send to all peers.
            var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(sync) };
            Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);
        }

        private unsafe void UpdateCourseSelect()
        {
            var task = Tracker.CourseSelect;
           
            // Exit if character disconnected.
            if (task->TaskStatus == CourseSelectTaskState.Closing)
            {
                Dispose();
                return;
            }

            // Calculate Sync Data based off of Client Info
            var allData = PlayerMap.GetCustomData();
            var loop    = new CourseSelectLoop();
            foreach (var data in allData)
                loop = loop.Add(data.GetCourseLoop());

            var sync = CourseSelectSync.FromGame(task);
            sync.Merge(loop);
            sync.ToGame(task);

            var serverMessage = new ReliablePacket() { MenuSynchronizationCommand = new MenuSynchronizationCommand(sync) };
            Manager.SendToAll(serverMessage.Serialize(), DeliveryMethod.ReliableOrdered);

            // Cleanup for next frame.
            // Nothing to do here.
        }

        private void UpdateMenuColour()
        {
            if (Manager.ConnectedPeersCount > 0)
                Tracker.TitleSequence->TaskData->Colour = MenuColour.Red;
            else
                Tracker.TitleSequence->TaskData->Colour = MenuColour.Blue;
        }

        /// <summary>
        /// Sends a personalized player map (excluding the player). 
        /// </summary>
        public void UpdatePlayerMap()
        {
            foreach (var peer in Manager.ConnectedPeerList)
            {
                var message = PlayerMap.ToMessage(peer);
                var serverMessage = new ServerMessage(message);
                var reliableMessage = new ReliablePacket() { ServerMessage = serverMessage };
                peer.Send(reliableMessage.Serialize(), DeliveryMethod.ReliableUnordered);
            }
        }

        /// <summary>
        /// Sends game data to a peer one time.
        /// </summary>
        private void SendGameData(NetPeer peer)
        {
            var gameData = GameData.FromGame();
            var serverMessage = new ReliablePacket() { GameData = gameData };
            peer.Send(serverMessage.Serialize(), DeliveryMethod.ReliableUnordered);
        }

        private void HandleMenuSyncMessage(NetPeer peer, MenuSynchronizationCommand syncCommand)
        {
            var data = PlayerMap.GetCustomData(peer);
            data.SetSyncCommand(syncCommand.Command);
        }

        private void HandleServerMessage(NetPeer peer, ServerMessage serverMessage)
        {
            var message = serverMessage.Message;
            switch (message)
            {
                case ClientSetPlayerData clientSetPlayerData:
                    PlayerMap.AddOrUpdatePlayerData(peer, clientSetPlayerData.Data);
                    UpdatePlayerMap();
                    break;
            }
        }

        #region Events / Overrides
        public override void HandleReliablePacket(NetPeer peer, ReliablePacket packet)
        {
            if (packet.MenuSynchronizationCommand != null) 
                HandleMenuSyncMessage(peer, packet.MenuSynchronizationCommand.Value);

            if (packet.ServerMessage != null)
                HandleServerMessage(peer, packet.ServerMessage.Value);
        }

        public override void HandleUnreliablePacket(NetPeer peer, UnreliablePacket packet)
        {
            // TODO: [Host] Race / Handle Unreliable Packets
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            // Wait for user data acknowledgement.
            if (!TryWaitForMessage(peer, CheckIfUserData))
            {
                peer.Disconnect();
                return;
            }

            // Player map updated in handler.
            UpdateMenuColour();
            SendGameData(peer);
            bool CheckIfUserData(Packet packet) => packet.Reliable?.ServerMessage?.MessageKind == ServerMessageType.ClientSetPlayerData;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PlayerMap.RemovePeer(peer);
            UpdatePlayerMap();
            UpdateMenuColour();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // Update latency.
            var data = PlayerMap.GetCustomData(peer);
            if (data != null)
                data.Latency = latency;
        }

        public override void OnConnectionRequest(ConnectionRequest request)
        {
            // Reject if not in course select menu.
            if (Tracker.LastTask != Tasks.CourseSelect)
                request.Reject();

            if (!PlayerMap.HasEmptySlots())
                request.Reject();

            // Accept on password.
            request.AcceptIfKey(Password);
        }
        #endregion
    }
}
