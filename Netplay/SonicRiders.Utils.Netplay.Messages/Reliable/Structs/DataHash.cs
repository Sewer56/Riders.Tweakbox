using System;
using K4os.Hash.xxHash;
using Reloaded.Memory.Streams;

namespace Riders.Netplay.Messages.Reliable.Structs
{
    public struct DataHash : IEquatable<DataHash>
    {
        public ulong Hash;
        public DataHash(ulong hash) => Hash = hash;

        /// <summary>
        /// Gets a hash of the current game data.
        /// </summary>
        /// <returns></returns>
        public static DataHash FromGame()
        {
            using (var extendedMemoryStream = new ExtendedMemoryStream())
            {
                GameData.FromGame().ToUncompressedBytes(extendedMemoryStream);
                var data = extendedMemoryStream.ToArray();

                return new DataHash(XXH64.DigestOf(data, 0, data.Length));
            }
        }

        /// <summary>
        /// Gets a hash of the current game data.
        /// </summary>
        /// <returns></returns>
        public static DataHash FromData(byte[] data) => new DataHash(XXH64.DigestOf(data, 0, data.Length));

        /// <summary>
        /// True if external hash matches our game, else false.
        /// </summary>
        public static bool Verify(DataHash external) => external.Equals(FromGame());

        public override int GetHashCode() => base.GetHashCode();
        public bool Equals(DataHash other) => Hash == other.Hash;
    }
}
