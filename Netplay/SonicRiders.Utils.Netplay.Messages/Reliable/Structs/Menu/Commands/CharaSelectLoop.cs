using MessagePack;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums;
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
    }
}