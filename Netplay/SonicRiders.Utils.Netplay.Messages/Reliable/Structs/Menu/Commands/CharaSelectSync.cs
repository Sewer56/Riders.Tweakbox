using System.Diagnostics;
using System.Linq;
using MessagePack;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public struct CharaSelectSync : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectSync;

        /// <summary>
        /// List of loops, in player order.
        /// Excludes self, starts with player 2.
        /// </summary>
        [Key(0)]
        public CharaSelectLoop[] Sync;

        public CharaSelectSync(CharaSelectLoop[] sync) => Sync = sync;

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static CharaSelectSync FromBytes(BufferedStreamReader reader) => Utilities.DesrializeMessagePack<CharaSelectSync>(reader);

        /// <summary>
        /// Contains a start or exit flag.
        /// </summary>
        public bool ContainsExit() => Sync.Any(x => x.IsExitingMenu);

        /// <summary>
        /// Applies the current task to the game.
        /// </summary>
        /// <param name="task">The individual character select task.</param>
        public unsafe void ToGame(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (task == null) 
                return;

            if (IsStartingRace(task))
                return;

            ResetMenu(task);
            for (var index = 0; index < Sync.Length; index++)
            {
                var sync = Sync[index];
                sync.ToGame(task, index + 1);
            }

            var statuses = task->TaskData->PlayerStatuses;
            var fixedArrayPtr = new FixedArrayPtr<byte>((ulong)statuses, 4);
            if (fixedArrayPtr.All(x => x > (int)PlayerStatus.SetReady || x == (int)PlayerStatus.Inactive) && task->TaskStatus != CharacterSelectTaskState.LoadingStage)
                task->TaskData->AreYouReadyEnabled = true;
            else
                task->TaskData->AreYouReadyEnabled = false;
        }

        /// <summary>
        /// True if the race is currently being started, else false.
        /// </summary>
        public unsafe bool IsStartingRace(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            return (task->TaskStatus == 0 || task->TaskStatus == CharacterSelectTaskState.LoadingStage) && task->TaskData->SelectionIsPerformed == 1;
        }

        /// <summary>
        /// Resets game menu values for other players (in case of player disconnection).
        /// </summary>
        private unsafe void ResetMenu(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            for (int x = 1; x < 4; x++)
            {
                task->TaskData->PlayerMenuSelections[x] = 0;
                task->TaskData->PlayerStatuses[x] = (byte)PlayerStatus.Inactive;
            }
        }
    }
}