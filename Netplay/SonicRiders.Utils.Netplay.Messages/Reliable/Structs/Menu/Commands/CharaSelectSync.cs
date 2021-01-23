using System.Linq;
using MessagePack;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
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
        public static CharaSelectSync FromBytes(BufferedStreamReader reader) => Utilities.DeserializeMessagePack<CharaSelectSync>(reader);

        /// <summary>
        /// Applies the current struct to game data, but only the character data.
        /// </summary>
        public unsafe void ToGameOnlyCharacter()
        {
            for (var index = 0; index < Sync.Length; index++)
            {
                var sync = Sync[index];
                sync.ToGameOnlyCharacter(index + 1);
            }
        }

        /// <summary>
        /// Applies the current task to the game.
        /// </summary>
        /// <param name="task">The individual character select task.</param>
        public unsafe void ToGame(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (task == null) 
                return;

            ResetMenu(task);
            for (var index = 0; index < Sync.Length; index++)
            {
                var sync = Sync[index];
                sync.ToGame(task, index + 1);
            }

            // Check player's own status & others' statuses.
            var ownStatus = (PlayerStatus) task->TaskData->PlayerStatuses[0];
            if (IsJoinedAndReady(ownStatus) && Sync.All(x => IsJoinedAndReady(x.Status) && task->TaskStatus != CharacterSelectTaskState.LoadingStage))
                task->TaskData->AreYouReadyEnabled = true;
            else
                task->TaskData->AreYouReadyEnabled = false;

            bool IsJoinedAndReady(PlayerStatus status) => status > PlayerStatus.SetReady || status == PlayerStatus.Inactive;
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