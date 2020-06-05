using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct RuleSettingsSync : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.RuleSettingsSync;
        public byte[] ToBytes() => Struct.GetBytes(this);

        public byte MenuSelectionX;
        public byte MenuSelectionY;
        public byte LapCounter;
        public byte Announcer;
        public byte Level;
        public byte Item;
        public byte Pit;
        public byte Air;
    }
}