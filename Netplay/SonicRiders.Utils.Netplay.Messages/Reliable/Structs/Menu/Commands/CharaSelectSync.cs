using System;
using System.Linq;
using MessagePack;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Misc.Interfaces;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    [Equals(DoNotAddEqualityOperators = true)]
    public struct CharaSelectSync : IMenuSynchronizationCommand, IBitPackedArray<CharaSelectLoop, CharaSelectSync>
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectSync;

        /// <inheritdoc />
        public CharaSelectLoop[] Elements { get; set; }

        /// <inheritdoc />
        public int NumElements { get; set; }

        /// <inheritdoc />
        public bool IsPooled { get; set; }

        public CharaSelectSync(CharaSelectLoop[] sync)
        {
            Elements = sync;
            NumElements = sync.Length;
            IsPooled = false;
        }

        public Span<byte> ToBytes(Span<byte> buffer) => AsInterface().Serialize(buffer);
        public static CharaSelectSync FromBytes(BufferedStreamReader reader) => new CharaSelectSync().AsInterface().Deserialize(reader);

        /// <summary>
        /// Applies the current struct to game data, but only the character data.
        /// </summary>
        public unsafe void ToGameOnlyCharacter()
        {
            for (var index = 0; index < Elements.Length; index++)
            {
                var sync = Elements[index];
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
            for (var index = 0; index < Elements.Length; index++)
            {
                var sync = Elements[index];
                sync.ToGame(task, index + 1);
            }

            // Check player's own status & others' statuses.
            var ownStatus = (PlayerStatus) task->TaskData->PlayerStatuses[0];
            if (IsJoinedAndReady(ownStatus) && Elements.All(x => IsJoinedAndReady(x.Status) && task->TaskStatus != CharacterSelectTaskState.LoadingStage))
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

        /// <inheritdoc />
        public IBitPackedArray<CharaSelectLoop, CharaSelectSync> AsInterface() => this;
    }
}