using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
namespace Riders.Netplay.Messages.Reliable.Structs.Server;

/// <summary>
/// Stores the version string for the current version of Tweakbox.
/// To obtain the version, use something like `typeof(Example).Assembly.GetName().Version` where the type is a Tweakbox class.
/// </summary>
public struct VersionInformation : IReliableMessage
{
    /*
        For backwards compatibility reasons do NOT extend this class.
        For adding additional elements, create a version called VersionInformationEx
        and extend that class as freely as needed.

        Before doing anything, a check using this struct will be made.
    */

    /// <summary>
    /// Version of tweakbox.
    /// </summary>
    public string TweakboxVersion;

    public VersionInformation(string tweakboxVersion) => TweakboxVersion = tweakboxVersion;

    /// <inheritdoc />
    public void Dispose() { }

    /// <summary>
    /// Verifies the current version against the other version.
    /// </summary>
    public bool Verify(VersionInformation other) => other.TweakboxVersion.Equals(TweakboxVersion);

    /// <inheritdoc />
    public MessageType GetMessageType() => MessageType.Version;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => TweakboxVersion = bitStream.ReadString();

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream => bitStream.WriteString(TweakboxVersion);
}
