using MessagePack;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    /// <summary>
    /// Client -> Host
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public struct CharaSelectLoop : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectLoop;
        public byte[] ToBytes() => Struct.GetBytes(this);

        /// <summary>
        /// The character selected by the current player.
        /// </summary>
        [Key(0)]
        public byte Character;

        /// <summary>
        /// Current board selected by the player.
        /// </summary>
        [Key(1)]
        public byte Board;

        /// <summary>
        /// Current status of the player.
        /// </summary>
        [Key(2)]
        public PlayerStatus Status;

        public CharaSelectLoop(byte character, byte board, PlayerStatus status)
        {
            Character = character;
            Board = board;
            Status = status;
        }

        /// <summary>
        /// Applies the current struct to the game, but only the non-menu data.
        /// </summary>
        public unsafe void ToGameOnlyCharacter(int index)
        {
            ref var player = ref Player.Players[index];
            player.Character = (Characters)Character;
            player.ExtremeGear = (ExtremeGear)Board;

            player.IsAiLogic  = PlayerType.CPU;
            player.IsAiVisual = PlayerType.Human; // This is necessary for Super Sonic

            // Replace character with SS if necessary.
            if (player.ExtremeGear == ExtremeGear.ChaosEmerald)
                player.Character = Characters.SuperSonic;
        }

        /// <summary>
        /// Applies the current task to the game.
        /// </summary>
        /// <param name="task">The individual menu task.</param>
        /// <param name="index">The character index.</param>
        public unsafe void ToGame(Task<CharacterSelect, CharacterSelectTaskState>* task, int index)
        {
            if (task == null)
                return;

            ToGameOnlyCharacter(index);
            
            // Menu supports only 4 characters
            if (index <= 3)
            {
                task->TaskData->PlayerMenuSelections[index] = CharacterToSelection(Character);

                switch (Status)
                {
                    case PlayerStatus.Inactive:
                    case PlayerStatus.Active:
                    case PlayerStatus.GearSelect:
                    case PlayerStatus.GearDescription:
                        task->TaskData->PlayerStatuses[index] = (byte) Status;
                        break;
                    // Set to an invalid state.
                    // This will look as if the other person has selected the character but 
                    // will not load the model. This is good for now, as it will reduce pressure off us
                    // to implement code to render the model.
                    case PlayerStatus.SetReady:
                    case PlayerStatus.Ready:
                        task->TaskData->PlayerStatuses[index] = 6;
                        break;
                }

                task->TaskData->PlayerMenuSelections[index] = CharacterToSelection(Character);
            }
        }

        /// <summary>
        /// Gets the loop data from the current game instance.
        /// </summary>
        /// <param name="task">The individual menu task.</param>
        public static unsafe CharaSelectLoop FromGame(Task<CharacterSelect, CharacterSelectTaskState>* task)
        {
            if (task == null)
                return new CharaSelectLoop();

            var data = task->TaskData;
            return new CharaSelectLoop()
            {
                Character = (byte)Player.Players[0].Character,
                Board = (byte)Player.Players[0].ExtremeGear,
                Status = (PlayerStatus)data->PlayerStatuses[0]
            };
        }

        /// <summary>
        /// Converts a character index to a menu selection index.
        /// </summary>
        private byte CharacterToSelection(byte character)
        {
            if (character <= (int) Characters.Shadow)
                return character;

            // Super Sonic is not available in the menu.
            return (byte) (character - 1);
        }
    }
}