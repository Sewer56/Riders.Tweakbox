namespace Riders.Netplay.Messages.Misc.Interfaces
{
    public interface IBitPackable<T> : Sewer56.BitStream.Interfaces.IBitPackable<T> where T : new() { }
}