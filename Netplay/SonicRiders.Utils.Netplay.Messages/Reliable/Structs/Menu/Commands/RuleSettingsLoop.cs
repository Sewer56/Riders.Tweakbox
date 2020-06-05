using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct RuleSettingsLoop : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.RuleSettingsLoop;
        public byte[] ToBytes() => Struct.GetBytes(this);

        public byte DeltaMenuSelectionX;
        public byte DeltaMenuSelectionY;
        public byte DeltaLapCounter;
        public byte DeltaAnnouncer;
        public byte DeltaLevel;
        public byte DeltaItem;
        public byte DeltaPit;
        public byte DeltaAir;
    }
}