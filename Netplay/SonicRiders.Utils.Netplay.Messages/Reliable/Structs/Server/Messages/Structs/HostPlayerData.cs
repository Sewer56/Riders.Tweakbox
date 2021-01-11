using MessagePack;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public class HostPlayerData
    {
        [Key(0)]
        public string Name = "Unknown";

        /// <summary>
        /// The type of client.
        /// </summary>
        [Key(1)]
        public ClientKind ClientType = ClientKind.Client;

        /// <summary>
        /// Index of the individual player.
        /// This corresponds to the indices in <see cref="UnreliablePacket"/> and <see cref="BoostTornadoAttackPacked"/>.
        /// Ignore if received from client.
        /// </summary>
        [Key(2)]
        public int PlayerIndex = -1;

        /// <summary>
        /// Contains the current ping of the individual player.
        /// </summary>
        [Key(3)]
        public int Latency = 999;

        /// <summary>
        /// Copies data submitted by the client.
        /// </summary>
        public void UpdateFromClient(HostPlayerData data)
        {
            this.Name = data.Name;
            this.ClientType = data.ClientType;
            this.Latency = data.Latency;
        }
    }
}
