using Riders.Netplay.Messages.External;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Player = Sewer56.SonicRiders.API.Player;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct
{
    /// <summary>
    /// Contains information about all Tweakbox components (e.g. custom gears, custom characters) to be synced over the network.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public struct TweakboxData
    {
        /// <summary>
        /// Custom character data to be shared across the network.
        /// </summary>
        public CustomCharacterData CharacterData;

        /// <summary>
        /// Custom gear data.
        /// </summary>
        public CustomGearData GearData;

        /// <summary>
        /// Retrieves the approximate size of data to be sent over the network.
        /// </summary>
        public unsafe int GetDataSize()
        {
            var dataSize = GearData.GetDataSize();
            dataSize += CharacterData.GetDataSize();
            return dataSize;
        }

        /// <summary>
        /// Writes the contents of this packet to the game memory.
        /// </summary>
        /// <returns>True on success, else false.</returns>
        public unsafe void ToGame()
        {
            CustomGearData.ToGame(GearData);
            CustomCharacterData.ToGame(CharacterData);
        }

        /// <summary>
        /// Retrieves gear information from the game.
        /// </summary>
        public static unsafe TweakboxData FromGame()
        {
            var data = new TweakboxData()
            {
                GearData = CustomGearData.FromGame(),
                CharacterData = CustomCharacterData.FromGame()
            };

            return data;
        }

        /// <summary>
        /// Deserializes the contents of this message from a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            GearData.FromStream(ref bitStream);
            CharacterData.FromStream(ref bitStream);
        }

        /// <summary>
        /// Serializes the contents of this message to a stream.
        /// </summary>
        /// <param name="bitStream">The stream.</param>
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            GearData.ToStream(ref bitStream);
            CharacterData.ToStream(ref bitStream);
        }
    }
}