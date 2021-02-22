using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu
{
    public struct RuleSettingsSync : IReliableMessage, IEquatable<RuleSettingsSync>
    {
        public byte MenuSelectionX;
        public byte MenuSelectionY;
        public byte LapCounter;
        public byte Announcer;
        public byte Level;
        public byte Item;
        public byte Pit;
        public byte AirLost;
        public byte ExitingMenu;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.RuleSettingsSync;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            MenuSelectionX = bitStream.Read<byte>();
            MenuSelectionY = bitStream.Read<byte>();
            LapCounter = bitStream.Read<byte>();
            Announcer = bitStream.Read<byte>();
            Level = bitStream.Read<byte>();
            Item = bitStream.Read<byte>();
            Pit = bitStream.Read<byte>();
            AirLost = bitStream.Read<byte>();
            ExitingMenu = bitStream.Read<byte>();
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.Write(MenuSelectionX);
            bitStream.Write(MenuSelectionY);
            bitStream.Write(LapCounter);
            bitStream.Write(Announcer);
            bitStream.Write(Level);
            bitStream.Write(Item);
            bitStream.Write(Pit);
            bitStream.Write(AirLost);
            bitStream.Write(ExitingMenu);
        }

        public static unsafe RuleSettingsSync FromGame(Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (task == null)
                return new RuleSettingsSync();

            var data = task->TaskData;
            bool isExiting = data->MenuState == MenuState.Exit && task->TaskStatus == RaceRulesTaskState.Exiting;

            return new RuleSettingsSync()
            {
                MenuSelectionX = data->CurrentHorizontalSelection,
                MenuSelectionY = data->CurrentVerticalSelection,
                LapCounter = data->TotalLaps,
                Announcer = (byte) (data->Announcer),
                Level = (byte)(data->Level),
                Item = (byte)(data->Item),
                Pit = (byte)(data->Pit),
                AirLost = (byte)(data->AirLostAction),
                ExitingMenu = (byte)(isExiting ? 1 : 0)
            };
        }

        public RuleSettingsLoop Delta(RuleSettingsSync after)
        {
            return new RuleSettingsLoop()
            {
                DeltaMenuSelectionX = (sbyte) (after.MenuSelectionX - MenuSelectionX),
                DeltaMenuSelectionY = (sbyte) (after.MenuSelectionY - MenuSelectionY),
                DeltaAir = (sbyte) (after.AirLost - AirLost),
                DeltaLevel = (sbyte) (after.Level - Level),
                DeltaPit = (sbyte) (after.Pit - Pit),
                DeltaAnnouncer = (sbyte) (after.Announcer - Announcer),
                DeltaItem = (sbyte) (after.Item - Item),
                DeltaLapCounter = (sbyte) (after.LapCounter - LapCounter),
                ExitingMenu = (byte) (after.ExitingMenu - ExitingMenu)
            };
        }

        public RuleSettingsSync Merge(RuleSettingsLoop loop)
        {
            MenuSelectionX = (byte) (MenuSelectionX + loop.DeltaMenuSelectionX);
            MenuSelectionY = (byte) (MenuSelectionY + loop.DeltaMenuSelectionY);
            LapCounter = (byte) (LapCounter + loop.DeltaLapCounter);
            Announcer = (byte) (Announcer + loop.DeltaAnnouncer);
            Level = (byte) (Level + loop.DeltaLevel);
            Item = (byte) (Item + loop.DeltaItem);
            Pit = (byte) (Pit + loop.DeltaPit);
            AirLost = (byte) (AirLost + loop.DeltaAir);
            ExitingMenu = (byte)(ExitingMenu + loop.ExitingMenu);
            return this;
        }

        public unsafe void ToGame(Task<RaceRules, RaceRulesTaskState>* task)
        {
            if (task == null)
                return;

            var data = task->TaskData;
            data->CurrentHorizontalSelection = MenuSelectionX;
            data->CurrentVerticalSelection = MenuSelectionY;
            data->TotalLaps = LapCounter;
            data->Announcer = Announcer;
            data->Level = Level;
            data->Item = Item;
            data->Pit = Pit;
            data->AirLostAction = (AirLostActions) AirLost;

            if (ExitingMenu > 0)
            {
                if (task->TaskStatus != RaceRulesTaskState.Closed)
                    task->TaskStatus = RaceRulesTaskState.Exiting;
                
                if (data->MenuState != MenuState.Closed)
                    data->MenuState = MenuState.Exit;

                // Save current settings.
                Functions.RuleSettingsSaveCurrentSettings.GetWrapper()(task->TaskData);
            }
        }

        #region Autogenerated
        public bool Equals(RuleSettingsSync other)
        {
            return MenuSelectionX == other.MenuSelectionX && MenuSelectionY == other.MenuSelectionY && LapCounter == other.LapCounter && Announcer == other.Announcer && Level == other.Level && Item == other.Item && Pit == other.Pit && AirLost == other.AirLost && ExitingMenu == other.ExitingMenu;
        }

        public override bool Equals(object obj)
        {
            return obj is RuleSettingsSync other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MenuSelectionX.GetHashCode();
                hashCode = (hashCode * 397) ^ MenuSelectionY.GetHashCode();
                hashCode = (hashCode * 397) ^ LapCounter.GetHashCode();
                hashCode = (hashCode * 397) ^ Announcer.GetHashCode();
                hashCode = (hashCode * 397) ^ Level.GetHashCode();
                hashCode = (hashCode * 397) ^ Item.GetHashCode();
                hashCode = (hashCode * 397) ^ Pit.GetHashCode();
                hashCode = (hashCode * 397) ^ AirLost.GetHashCode();
                hashCode = (hashCode * 397) ^ ExitingMenu.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}