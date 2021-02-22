namespace Riders.Netplay.Messages.Misc.Interfaces
{
    /// <summary>
    /// Used for packets and other messages that contain a sequence number.
    /// </summary>
    public interface ISequenced
    {
        /// <summary>
        /// Maximum sequence value.
        /// This value should be constant.
        /// </summary>
        public int MaxValue { get; }

        /// <summary>
        /// The current sequence number.
        /// </summary>
        public int SequenceNo { get; }
    }
}