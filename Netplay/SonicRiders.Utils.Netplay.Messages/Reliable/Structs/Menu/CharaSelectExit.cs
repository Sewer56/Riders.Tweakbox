using Riders.Netplay.Messages.Helpers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Menu;

public struct CharaSelectExit : IReliableMessage
{
    /// <summary>
    /// True if starting a race, else exiting the menu.
    /// </summary>
    public ExitKind Type;

    public CharaSelectExit(ExitKind type) : this()
    {
        Type = type;
    }

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public readonly MessageType GetMessageType() => MessageType.CharaSelectExit;

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => bitStream.WriteGeneric(Type, EnumNumBits<ExitKind>.Number);

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => Type = bitStream.ReadGeneric<ExitKind>(EnumNumBits<ExitKind>.Number);
}

public enum ExitKind
{
    Null,
    Exit,
    Start
}
