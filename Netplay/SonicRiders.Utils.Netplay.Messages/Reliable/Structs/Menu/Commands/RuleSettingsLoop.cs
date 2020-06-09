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
        public byte ExitingMenu;

        public RuleSettingsLoop Add(RuleSettingsLoop other)
        {
            return new RuleSettingsLoop()
            {
                DeltaAir = (byte) (DeltaAir + other.DeltaAir),
                DeltaPit = (byte) (DeltaPit + other.DeltaPit),
                DeltaItem = (byte) (DeltaItem + other.DeltaItem),
                DeltaLevel = (byte) (DeltaLevel + other.DeltaLevel),
                DeltaAnnouncer = (byte) (DeltaAnnouncer + other.DeltaAnnouncer),
                DeltaLapCounter = (byte) (DeltaLapCounter + other.DeltaLapCounter),
                DeltaMenuSelectionX = (byte) (DeltaMenuSelectionX + other.DeltaMenuSelectionX),
                DeltaMenuSelectionY = (byte) (DeltaMenuSelectionY + other.DeltaMenuSelectionY),
                ExitingMenu = (byte)(ExitingMenu + other.ExitingMenu)
            };
        }
    }
}