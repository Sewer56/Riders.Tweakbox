using MessagePack;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public class HostPlayerData
    {
        [Key(0)]
        public string Name { get; set; }

        /// <summary>
        /// Index of the individual player.
        /// This corresponds to the indices in <see cref="UnreliablePacket"/> and <see cref="BoostTornadoAttackPacked"/>.
        /// Ignore if received from client.
        /// </summary>
        [Key(1)]
        public int PlayerIndex { get; set; }

        /// <summary>
        /// Copies data submitted by the client.
        /// </summary>
        public void UpdateFromClient(HostPlayerData data)
        {
            Name = data.Name;
        }
    }
}
