using MessagePack;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    [Equals(DoNotAddEqualityOperators = true)]
    [MessagePackObject()]
    public struct CharaSelectSync : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectSync;

        [Key(0)]
        public CharaSelectLoop[] Sync;

        public CharaSelectSync(CharaSelectLoop[] sync) => Sync = sync;

        public byte[] ToBytes() => MessagePackSerializer.Serialize(this);
        public static CharaSelectSync FromBytes(BufferedStreamReader reader) => Utilities.DesrializeMessagePack<CharaSelectSync>(reader);
    }
}