using MessagePack;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages.Structs
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public class ClientPlayerData
    {
        [Key(0)]
        public string Name { get; set; }

        public ClientPlayerData(string name) => Name = name;
    }
}
