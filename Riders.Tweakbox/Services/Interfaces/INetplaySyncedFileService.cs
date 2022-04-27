namespace Riders.Tweakbox.Services.Interfaces;

public interface INetplaySyncedFileService : IFileService
{
    /// <summary>
    /// Unique identifier for this service (for Netplay Serialization).
    /// </summary>
    public int Id { get; }
}