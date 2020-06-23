using BitStreams;

namespace Riders.Netplay.Messages.Misc.Interfaces
{
    /// <summary>
    /// Declares a structure that can be packed inside of a byte.
    /// </summary>
    public interface IBitPackable<TParent> where TParent : unmanaged
    {
        /// <summary>
        /// Gets the size of an individual buffer entry in bits.
        /// </summary>
        public int GetSizeOfEntry();

        /// <summary>
        /// Creates an instance of the target structure given a byte.
        /// </summary>
        TParent FromStream(BitStream stream);

        /// <summary>
        /// Converts a struct instance into an individual byte.
        /// </summary>
        void ToStream(BitStream stream);
    }
}