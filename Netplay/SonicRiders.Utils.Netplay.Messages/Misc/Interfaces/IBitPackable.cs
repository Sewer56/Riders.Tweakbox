namespace Riders.Netplay.Messages.Misc.Interfaces
{
    public interface IBitPackable<T> : Sewer56.BitStream.Interfaces.IBitPackable<T> where T : new()
    {
        /// <summary>
        /// Gets the size of an individual buffer entry in bits.
        /// </summary>
        public int GetSizeOfEntry();
    }
}