using Riders.Netplay.Messages.Misc.Interfaces;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;
namespace Riders.Netplay.Messages.Reliable.Structs.Menu;

[Equals(DoNotAddEqualityOperators = true)]
public struct CharaSelectSync : IReliableMessage, IBitPackedArray<CharaSelectLoop, CharaSelectSync>
{
    /// <inheritdoc />
    public CharaSelectLoop[] Elements { get; set; }

    /// <inheritdoc />
    public int NumElements { get; set; }

    /// <inheritdoc />
    public bool IsPooled { get; set; }

    public CharaSelectSync(CharaSelectLoop[] loops)
    {
        Elements = loops;
        NumElements = loops.Length;
        IsPooled = false;
    }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.CharaSelectSync;

    /// <inheritdoc />
    public void Set(CharaSelectLoop[] elements, int numElements = -1) => this.Set<CharaSelectSync, CharaSelectLoop, CharaSelectSync>(elements, numElements);

    /// <inheritdoc />
    public CharaSelectSync Create(CharaSelectLoop[] elements) => this.Create<CharaSelectSync, CharaSelectLoop, CharaSelectSync>(elements);

    /// <inheritdoc />
    public CharaSelectSync CreatePooled(int numElements) => this.CreatePooled<CharaSelectSync, CharaSelectLoop, CharaSelectSync>(numElements);
    public void ToPooled(int numElements) => this = CreatePooled(numElements);

    public CharaSelectSync Clone() => this.Clone<CharaSelectSync, CharaSelectLoop, CharaSelectSync>();

    /// <inheritdoc />
    public void Dispose() => IBitPackedArray<CharaSelectLoop, CharaSelectSync>.Dispose(ref this);

    /// <summary>
    /// Applies the current struct to game data, but only the character data.
    /// </summary>
    public unsafe void ToGameOnlyCharacter(int numLocalPlayers, int totalNumPlayers)
    {
        for (var index = numLocalPlayers; index < totalNumPlayers; index++)
        {
            var sync = Elements[index];
            sync.ToGameOnlyCharacter(index);
        }
    }

    /// <summary>
    /// Applies the current task to the game.
    /// </summary>
    /// <param name="task">The individual character select task.</param>
    /// <param name="numLocalPlayers">Number of local players playing on this PC.</param>
    /// <param name="totalNumPlayers">Total number of players in this game.</param>
    public unsafe void ToGame(Task<CharacterSelect, CharacterSelectTaskState>* task, int numLocalPlayers, int totalNumPlayers)
    {
        if (task == null)
            return;

        ResetMenu(task, numLocalPlayers);
        for (var index = numLocalPlayers; index < totalNumPlayers; index++)
        {
            var loop = Elements[index];
            loop.ToGame(task, index);
        }

        // Check player's own status & others' statuses.
        var readyEnabled = task->TaskData->AreYouReadyEnabled;
        var newReadyEnabled = (AllJoinedAndReady(totalNumPlayers) && task->TaskStatus != CharacterSelectTaskState.LoadingStage);
        task->TaskData->AreYouReadyEnabled = newReadyEnabled;

        if (readyEnabled && !newReadyEnabled)
        {
            // Set state from 5 (ready to start race) to 4 (ready) so that controls aren't blocked.
            for (int x = 0; x < numLocalPlayers; x++)
                task->TaskData->PlayerStatuses[x] = (byte)PlayerStatus.SetReady;
        }
    }

    /// <summary>
    /// Resets game menu values for other players (in case of player disconnection).
    /// </summary>
    private unsafe void ResetMenu(Task<CharacterSelect, CharacterSelectTaskState>* task, int numLocalPlayers)
    {
        for (int x = numLocalPlayers; x < 4; x++)
        {
            task->TaskData->PlayerMenuSelections[x] = 0;
            task->TaskData->PlayerStatuses[x] = (byte)PlayerStatus.Inactive;
        }
    }

    /// <summary>
    /// True if all players are joined and ready, else false.
    /// </summary>
    private bool AllJoinedAndReady(int totalNumPlayers)
    {
        for (int x = 0; x < totalNumPlayers; x++)
        {
            var element = Elements[x];
            if (!IsJoinedAndReady(element.Status))
                return false;
        }

        return true;

        bool IsJoinedAndReady(PlayerStatus status) => status > PlayerStatus.SetReady;
    }

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        this.Dispose();
        this = IBitPackedArray<CharaSelectLoop, CharaSelectSync>.FromStream(ref bitStream);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => IBitPackedArray<CharaSelectLoop, CharaSelectSync>.ToStream(this, ref bitStream);
}
