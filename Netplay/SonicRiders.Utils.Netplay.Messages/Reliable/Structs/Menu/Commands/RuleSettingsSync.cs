using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

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
        public byte AirLost;
        public byte ExitingMenu;

        public static unsafe RuleSettingsSync FromGame(Task<RaceRules, RaceRulesTaskState>* task)
        {
            var data = task->TaskData;
            bool isExiting = data->MenuState == MenuState.Exit && task->TaskStatus == RaceRulesTaskState.Closed;

            return new RuleSettingsSync()
            {
                MenuSelectionX = data->CurrentHorizontalSelection,
                MenuSelectionY = data->CurrentVerticalSelection,
                LapCounter = data->TotalLaps,
                Announcer = (byte) (data->Announcer ? 1 : 0),
                Level = (byte)(data->Level ? 1 : 0),
                Item = (byte)(data->Item ? 1 : 0),
                Pit = (byte)(data->Pit ? 1 : 0),
                AirLost = (byte)(data->AirLostAction),
                ExitingMenu = (byte)(isExiting ? 1 : 0)
            };
        }

        public void Merge(RuleSettingsLoop loop)
        {
            MenuSelectionX += loop.DeltaMenuSelectionX;
            MenuSelectionY += loop.DeltaMenuSelectionY;
            LapCounter += loop.DeltaLapCounter;
            Announcer += loop.DeltaAnnouncer;
            Level += loop.DeltaLevel;
            Item += loop.DeltaItem;
            Pit += loop.DeltaPit;
            AirLost += loop.DeltaAir;
        }

        public unsafe void ToGame(Task<RaceRules, RaceRulesTaskState>* task)
        {
            var data = task->TaskData;
            data->CurrentHorizontalSelection = MenuSelectionX;
            data->CurrentVerticalSelection = MenuSelectionY;
            data->TotalLaps = LapCounter;
            data->Announcer = Announcer >= 1;
            data->Level = Level >= 1;
            data->Item = Item >= 1;
            data->Pit = Pit >= 1;
            data->AirLostAction = (AirLostActions) AirLost;
        }
    }
}