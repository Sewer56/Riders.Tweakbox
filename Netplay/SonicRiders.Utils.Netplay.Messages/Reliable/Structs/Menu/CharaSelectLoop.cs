using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;
namespace Riders.Netplay.Messages.Reliable.Structs.Menu;

/// <summary>
/// Client -> Host
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public struct CharaSelectLoop : IReliableMessage, Misc.Interfaces.IBitPackable<CharaSelectLoop>
{
    private const int SizeOfCharacterBits = 5;
    private const int SizeOfBoardBits = 6;
    private const int SizeOfStatusBits = 3;

    /// <summary>
    /// The character selected by the current player.
    /// </summary>
    public byte Character;

    /// <summary>
    /// Current board selected by the player.
    /// </summary>
    public byte Board;

    /// <summary>
    /// Current status of the player.
    /// </summary>
    public PlayerStatus Status;

    public CharaSelectLoop(byte character, byte board, PlayerStatus status)
    {
        Character = character;
        Board = board;
        Status = status;
    }

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.CharaSelectLoop;

    /// <inheritdoc />
    public void FromStream<T>(ref BitStream<T> stream) where T : IByteStream
    {
        Character = stream.Read<byte>(SizeOfCharacterBits);
        Board = stream.Read<byte>(SizeOfBoardBits);
        Status = (PlayerStatus)stream.Read<byte>(SizeOfStatusBits);
    }

    /// <inheritdoc />
    CharaSelectLoop IBitPackable<CharaSelectLoop>.FromStream<T>(ref BitStream<T> stream)
    {
        var loop = new CharaSelectLoop();
        loop.FromStream(ref stream);
        return loop;
    }

    /// <inheritdoc />
    public void ToStream<T>(ref BitStream<T> stream) where T : IByteStream
    {
        stream.Write(Character, SizeOfCharacterBits);
        stream.Write(Board, SizeOfBoardBits);
        stream.Write((byte)Status, SizeOfStatusBits);
    }

    /// <summary>
    /// Applies the current struct to the game, but only the non-menu data.
    /// </summary>
    public unsafe void ToGameOnlyCharacter(int index)
    {
        ref var player = ref Player.Players[index];
        player.Character = (Characters)Character;
        player.ExtremeGear = (ExtremeGear)Board;
        player.CharacterForCamera = player.Character;

        player.IsAiLogic = PlayerType.CPU;
        player.IsAiVisual = PlayerType.CPU;

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
                    task->TaskData->OpenPlayerSlots[index] = 0xFF;
                    task->TaskData->PlayerStatuses[index] = (byte)Status;
                    break;

                case PlayerStatus.Active:
                case PlayerStatus.GearSelect:
                case PlayerStatus.GearDescription:
                    task->TaskData->OpenPlayerSlots[index] = (byte)index;
                    task->TaskData->PlayerStatuses[index] = (byte)Status;
                    break;
                // Set to an invalid state.
                // This will look as if the other person has selected the character but 
                // will not load the model. This is good for now, as it will reduce pressure off us
                // to implement code to render the model.
                case PlayerStatus.SetReady:
                case PlayerStatus.Ready:
                    task->TaskData->OpenPlayerSlots[index] = (byte)index;
                    task->TaskData->PlayerStatuses[index] = 6;
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the loop data from the current game instance.
    /// </summary>
    /// <param name="task">The individual menu task.</param>
    /// <param name="index">Index of the player. Range 0-3.</param>
    public static unsafe CharaSelectLoop FromGame(Task<CharacterSelect, CharacterSelectTaskState>* task, int index)
    {
        if (task == null)
            return new CharaSelectLoop();

        var data = task->TaskData;
        return new CharaSelectLoop()
        {
            Character = (byte)Player.Players[index].Character,
            Board = (byte)Player.Players[index].ExtremeGear,
            Status = (PlayerStatus)data->PlayerStatuses[index]
        };
    }

    /// <summary>
    /// Converts a character index to a menu selection index.
    /// </summary>
    private byte CharacterToSelection(byte character)
    {
        if (character <= (int)Characters.Shadow)
            return character;

        // Super Sonic is not available in the menu.
        return (byte)(character - 1);
    }
}
