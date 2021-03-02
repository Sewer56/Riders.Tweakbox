using System.Collections.Generic;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Reliable.Structs;
using Riders.Netplay.Messages.Reliable.Structs.Menu;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Constants = Riders.Netplay.Messages.Misc.Constants;

namespace Riders.Tweakbox.Components.Netplay.Components.Menu
{
    public unsafe class CourseSelect : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public MenuChangedManualEvents Delta { get; set; } = new MenuChangedManualEvents();

        /// <summary> Sync data for course select. </summary>
        private Timestamped<CourseSelectSync> _sync;

        /// <summary> [Host Only] Changes in course select since the last time they were sent to the clients. </summary>
        private readonly Queue<Timestamped<CourseSelectLoop>> _loopQueue = new Queue<Timestamped<CourseSelectLoop>>(Constants.MaxNumberOfPlayers * 4);
        private Timestamped<bool> _receivedSetStageFlag = false;

        public CourseSelect(Socket owner, EventController @event)
        {
            Socket = owner;
            Event = @event;
            Event.OnCourseSelect            += OnCourseSelect;
            Event.AfterCourseSelect         += Delta.Update;
            Event.OnEnterCharacterSelect    += EnterCharacterSelect;
            Delta.OnCourseSelectUpdated     += CourseSelectUpdated;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.OnCourseSelect            -= OnCourseSelect;
            Event.AfterCourseSelect         -= Delta.Update;
            Event.OnEnterCharacterSelect    -= EnterCharacterSelect;
            Delta.OnCourseSelectUpdated     -= CourseSelectUpdated;
        }

        private unsafe void OnCourseSelect(Task<Sewer56.SonicRiders.Structures.Tasks.CourseSelect, CourseSelectTaskState>* task)
        {
            // For host, first set delta and then get changes from other clients, such that if net change is 0, no packet is sent.
            // For others, sync first and then set delta, so only user changes are picked up.
            if (Socket.GetSocketType() == SocketType.Host)
            {
                Delta.Set(task);
                var sync    = CourseSelectSync.FromGame(task).Merge(GetLoop());
                _sync       = new Timestamped<CourseSelectSync>(sync);
                Synchronize(task);
            }
            else
            {
                Synchronize(task);
                Delta.Set(task);
            }
        }

        private void EnterCharacterSelect()
        {
            // Reset flag if old (e.g. received after entering charaselect).
            if (_receivedSetStageFlag.IsDiscard(Socket.State.MaxLatency))
                _receivedSetStageFlag = false;

            // If set stage was invoked by us, send to other clients.
            if (!_receivedSetStageFlag)
            {
                var level = (byte) *Sewer56.SonicRiders.API.State.Level;
                if (Socket.GetSocketType() == SocketType.Host)
                    Socket.SendToAllAndFlush(ReliablePacket.Create(new CourseSelectSetStage(level)), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)} / Host] Sending Stage Set Flag", LogCategory.Menu);
                else
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(new CourseSelectSetStage(level)), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)} / Client] Sending Stage Set Flag", LogCategory.Menu);
            }

            _receivedSetStageFlag = false;
        }

        private void CourseSelectUpdated(CourseSelectLoop loop, Task<Sewer56.SonicRiders.Structures.Tasks.CourseSelect, CourseSelectTaskState>* task)
        {
            if (Socket.GetSocketType() == SocketType.Host)
            {
                using var message = CourseSelectSync.FromGame(task);
                Socket.SendToAllAndFlush(ReliablePacket.Create(message), DeliveryMethod.ReliableOrdered);
            }
            else
            {
                loop.Undo(task);
                if (Socket.State.NumLocalPlayers > 0)
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, ReliablePacket.Create(loop), DeliveryMethod.ReliableOrdered);
            }
        }

        /// <inheritdoc />
        public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source)
        {
            switch (packet.MessageType)
            {
                case MessageType.CourseSelectLoop:
                    _loopQueue.Enqueue(packet.GetMessage<CourseSelectLoop>());
                    break;

                case MessageType.CourseSelectSetStage:
                    var value = packet.GetMessage<CourseSelectSetStage>();
                    if (!_receivedSetStageFlag.IsDiscard(Socket.State.MaxLatency))
                        return; // Sorry, another peer beat you to the punch!

                    *Sewer56.SonicRiders.API.State.Level = (Levels)value.StageId;
                    _receivedSetStageFlag = true;
                    if (Socket.GetSocketType() == SocketType.Host)
                        Socket.SendToAllExceptAndFlush(source, ReliablePacket.Create(value), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)}] Is Host, Rebroadcasting Stage Set Flag", LogCategory.Menu);

                    break;

                case MessageType.CourseSelectSync:
                    _sync = packet.GetMessage<CourseSelectSync>();
                    break;
            }
        }

        /// <summary>
        /// Synchronizes the course select state with the currently reported state.
        /// </summary>
        private unsafe void Synchronize(Task<Sewer56.SonicRiders.Structures.Tasks.CourseSelect, CourseSelectTaskState>* task)
        {
            if (!_sync.IsDiscard(Socket.State.MaxLatency))
                _sync.Value.ToGame(task);
        }

        /// <summary>
        /// Empties the course select queue and merges all loops into a single command.
        /// </summary>
        private CourseSelectLoop GetLoop()
        {
            var loop = new CourseSelectLoop();
            while (_loopQueue.TryDequeue(out var result))
            {
                if (result.IsDiscard(Socket.State.MaxLatency))
                    continue;

                loop = loop.Add(result.Value);
            }

            return loop;
        }

        /// <inheritdoc />
        public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
    }
}
