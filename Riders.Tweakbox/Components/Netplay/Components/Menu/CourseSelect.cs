using System.Collections.Concurrent;
using System.Diagnostics;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Queue;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Tweakbox.Components.Netplay.Helpers;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

namespace Riders.Tweakbox.Components.Netplay.Components.Menu
{
    public unsafe class CourseSelect : INetplayComponent
    {
        /// <inheritdoc />
        public Socket Socket { get; set; }
        public EventController Event { get; set; }
        public MenuChangedManualEvents Delta { get; set; } = new MenuChangedManualEvents();

        /// <summary> Sync data for course select. </summary>
        private Volatile<Timestamped<CourseSelectSync>> _sync = new Volatile<Timestamped<CourseSelectSync>>(new Timestamped<CourseSelectSync>());

        /// <summary> [Host Only] Changes in course select since the last time they were sent to the clients. </summary>
        private ConcurrentQueue<Timestamped<CourseSelectLoop>> _loopQueue => _queue.Get<Timestamped<CourseSelectLoop>>();
        private Timestamped<bool> _receivedSetStageFlag = false;

        private MessageQueue _queue = new MessageQueue();

        public CourseSelect(Socket owner, EventController @event)
        {
            Socket = owner;
            Event = @event;
            Event.OnCourseSelect            += OnCourseSelect;
            Event.AfterCourseSelect         += Delta.Update;
            Event.OnCourseSelectSetStage    += OnCourseSelectSetStage;
            Delta.OnCourseSelectUpdated     += CourseSelectUpdated;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Event.OnCourseSelect            -= OnCourseSelect;
            Event.AfterCourseSelect         -= Delta.Update;
            Event.OnCourseSelectSetStage    -= OnCourseSelectSetStage;
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
                _sync       = new Volatile<Timestamped<CourseSelectSync>>(sync);
                Synchronize(task);
            }
            else
            {
                Synchronize(task);
                Delta.Set(task);
            }
        }

        private void OnCourseSelectSetStage()
        {
            // Reset flag if old (e.g. received after entering charaselect).
            if (_receivedSetStageFlag.IsDiscard(128))
                _receivedSetStageFlag = false;

            // If set stage was invoked by us, send to other clients.
            if (!_receivedSetStageFlag)
            {
                var level = (byte) *Sewer56.SonicRiders.API.State.Level;
                if (Socket.GetSocketType() == SocketType.Host)
                    Socket.SendToAllAndFlush(new ReliablePacket(new CourseSelectSetStage(level)), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)} / Host] Sending Stage Set Flag", LogCategory.Menu);
                else
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket(new CourseSelectSetStage(level)), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)} / Client] Sending Stage Set Flag", LogCategory.Menu);
            }

            _receivedSetStageFlag = false;
        }

        private void CourseSelectUpdated(CourseSelectLoop loop, Task<Sewer56.SonicRiders.Structures.Tasks.CourseSelect, CourseSelectTaskState>* task)
        {
            if (Socket.GetSocketType() == SocketType.Host)
                Socket.SendToAllAndFlush(new ReliablePacket(CourseSelectSync.FromGame(task)), DeliveryMethod.ReliableOrdered);
            else
            {
                loop.Undo(task);
                if (Socket.GetSocketType() == SocketType.Client) // Spectator gets no input.
                    Socket.SendAndFlush(Socket.Manager.FirstPeer, new ReliablePacket(loop), DeliveryMethod.ReliableOrdered);
            }
        }

        /// <inheritdoc />
        public void HandlePacket(Packet<NetPeer> packet)
        {
            if (!packet.TryGetPacket<ReliablePacket>(Socket.State.MaxLatency, out var reliable)) 
                return;

            var command = reliable.MenuSynchronizationCommand;
            if (!command.HasValue)
                return;

            switch (command.Value.Command)
            {
                case CourseSelectLoop courseSelectLoop:
                    _loopQueue.Enqueue(courseSelectLoop);
                    break;
                case CourseSelectSetStage courseSelectSetStage:
                    Log.WriteLine($"[{nameof(CourseSelect)}] Received Stage Set Flag", LogCategory.Menu);
                    *Sewer56.SonicRiders.API.State.Level = (Levels) courseSelectSetStage.StageId;
                    _receivedSetStageFlag = true;
                    if (Socket.GetSocketType() == SocketType.Host)
                        Socket.SendToAllExcept(packet.Source, new ReliablePacket(courseSelectSetStage), DeliveryMethod.ReliableOrdered, $"[{nameof(CourseSelect)}] Is Host, Rebroadcasting Stage Set Flag");
                    
                    break;
                case CourseSelectSync courseSelectSync:
                    _sync = new Volatile<Timestamped<CourseSelectSync>>(courseSelectSync);
                    break;
            }
        }

        /// <summary>
        /// Synchronizes the course select state with the currently reported state.
        /// </summary>
        private unsafe void Synchronize(Task<Sewer56.SonicRiders.Structures.Tasks.CourseSelect, CourseSelectTaskState>* task)
        {
            if (!_sync.HasValue)
                return;

            var sync = _sync.Get();
            if (!sync.IsDiscard(Socket.State.MaxLatency))
                sync.Value.ToGame(task);
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
    }
}
