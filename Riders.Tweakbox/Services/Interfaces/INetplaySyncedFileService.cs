namespace Riders.Tweakbox.Services.Interfaces;

// TODO: Implement stuff that inherits this, i.e. verify files match from host -> client and/or upload/download if necessary.
public interface INetplaySyncedFileService : IFileService
{
    /// <summary>
    /// Unique identifier for this service (for Netplay Serialization).
    /// </summary>
    public int Id { get; }
}