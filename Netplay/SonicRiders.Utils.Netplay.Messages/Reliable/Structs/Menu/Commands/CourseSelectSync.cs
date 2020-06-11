using System;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Structs;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CourseSelectSync : IMenuSynchronizationCommand, IEquatable<CourseSelectSync>
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CourseSelectSync;
        public byte[] ToBytes() => Struct.GetBytes(this);

        /// <summary>
        /// X selection.
        /// </summary>
        public byte SelectionX;

        /// <summary>
        /// Y selection.
        /// </summary>
        public byte SelectionY;

        /// <summary>
        /// The currently highlighted item.
        /// </summary>
        public byte HighlightedItem;

        /// <summary>
        /// List scroll offset.
        /// </summary>
        public byte ListScroll;

        /// <summary>
        /// Submenu selection.
        /// </summary>
        public byte SubmenuSelection;

        /// <summary>
        /// Current Select Tasks State.
        /// </summary>
        public CourseSelectTaskState State;

        /// <summary>
        /// If enabled, is opening character select.
        /// </summary>
        public byte OpenCharacterSelect;

        /// <summary>
        /// Merges the current sync packer with a loop packet.
        /// </summary>
        public unsafe void Merge(CourseSelectLoop loop)
        {
            SubmenuSelection = (byte) (SubmenuSelection + loop.SubmenuDeltaSelection);
            SelectionX = (byte) (SelectionX + loop.DeltaSelectionX);
            SelectionY = (byte) (SelectionY + loop.DeltaSelectionY);
            ListScroll = (byte) (ListScroll + loop.DeltaListScroll);
            HighlightedItem = (byte) (HighlightedItem + loop.DeltaHighlighted);
            OpenCharacterSelect = (byte) (OpenCharacterSelect + loop.OpenCharacterSelect);
            if (loop.OpenSubmenu > 0)
                State = CourseSelectTaskState.HeroBabylonPicker;
            else if (loop.CloseSubmenu > 0)
                State = CourseSelectTaskState.Normal;
            else if (loop.OpenRaceRules > 0)
                State = CourseSelectTaskState.OpeningSettings;
        }

        /// <summary>
        /// Gets the sync instance from the current game given pointer to the task.
        /// </summary>
        public unsafe void ToGame(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (task == null)
                return;

            var data = task->TaskData;
            data->SubmenuSelection = SubmenuSelection;
            data->SelectionHorizontal = SelectionX;
            data->SelectionVertical = SelectionY;
            data->ListScrollOffset = ListScroll;
            data->HighlightedHorizontalItem = HighlightedItem;   

            // Switch menus if necessary.
            switch (task->TaskStatus)
            {
                case CourseSelectTaskState.Normal when State == CourseSelectTaskState.HeroBabylonPicker:
                    task->TaskStatus = CourseSelectTaskState.HeroBabylonPicker;
                    data->SubmenuState = MenuState.Enter;
                    break;
                case CourseSelectTaskState.HeroBabylonPicker when State == CourseSelectTaskState.Normal:
                    task->TaskStatus = CourseSelectTaskState.Normal;
                    data->SubmenuState = MenuState.Exit;
                    break;
                case CourseSelectTaskState.Normal when State == CourseSelectTaskState.OpeningSettings:
                    task->TaskStatus = CourseSelectTaskState.OpeningSettings;
                    data->MenuState = MenuState.Exit;
                    break;
            }

            if (OpenCharacterSelect > 0)
            {
                task->TaskStatus = 0;
                task->TaskData->EnterCharacterSelectFlag = 1;
            }

            // Menu scrolling fixups for laggy clients.
            // TODO: Find limits in game code because we are assuming the menu never gets modded.
            if (data->ListScrollOffset > 3)
                data->ListScrollOffset = 3;

            if ((sbyte) data->SelectionHorizontal - data->ListScrollOffset < 0)
                data->ListScrollOffset = data->SelectionHorizontal;

            if ((sbyte)data->SelectionHorizontal - data->ListScrollOffset > 4)
                data->ListScrollOffset = (byte) (data->SelectionHorizontal - 4);

            if ((sbyte)data->ListScrollOffset < 0)
                data->ListScrollOffset = 0;

            if ((sbyte)data->SelectionHorizontal < 0)
                data->SelectionHorizontal = 0;

            if ((sbyte)data->HighlightedHorizontalItem < 0)
                data->HighlightedHorizontalItem = 0;

            if ((sbyte)data->HighlightedHorizontalItem > 4)
                data->HighlightedHorizontalItem = 4;

            if ((sbyte) data->SelectionHorizontal > 7)
                data->SelectionHorizontal = 7;
        }

        /// <summary>
        /// Gets the sync instance from the current game given pointer to the task.
        /// </summary>
        public static unsafe CourseSelectSync FromGame(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            if (task == null)
                return new CourseSelectSync();

            var sync = new CourseSelectSync()
            {
                ListScroll = task->TaskData->ListScrollOffset,
                SelectionY = task->TaskData->SelectionVertical,
                SelectionX = task->TaskData->SelectionHorizontal,
                SubmenuSelection = task->TaskData->SubmenuSelection,
                HighlightedItem = task->TaskData->HighlightedHorizontalItem
            };

            if (task->TaskStatus == 0 && task->TaskData->EnterCharacterSelectFlag == 1)
                sync.OpenCharacterSelect = 1;

            switch (task->TaskData->SubmenuState)
            {
                case MenuState.Exit when task->TaskStatus == CourseSelectTaskState.HeroBabylonPicker:
                    sync.State = CourseSelectTaskState.Normal;
                    break;
                case MenuState.Enter when task->TaskStatus == CourseSelectTaskState.Normal:
                    sync.State = CourseSelectTaskState.HeroBabylonPicker;
                    break;
                default:
                {
                    switch (task->TaskData->MenuState)
                    {
                        case MenuState.Exit when task->TaskStatus == CourseSelectTaskState.OpeningSettings:
                            sync.State = CourseSelectTaskState.OpeningSettings;
                            break;
                        default:
                            sync.State = task->TaskStatus;
                            break;
                    }
                    break;
                }
            }

            return sync;
        }

        /// <summary>
        /// Gets the difference between the current course select state and the last state.
        /// </summary>
        /// <param name="after">The later state.</param>
        public unsafe CourseSelectLoop GetDelta(CourseSelectSync after)
        {
            bool openSubmenu  = after.State == CourseSelectTaskState.HeroBabylonPicker && State == CourseSelectTaskState.Normal;
            bool closeSubmenu = after.State == CourseSelectTaskState.Normal && State == CourseSelectTaskState.HeroBabylonPicker;
            bool openSettings = after.State == CourseSelectTaskState.OpeningSettings && State == CourseSelectTaskState.Normal;
            bool openCharacterSelect = after.OpenCharacterSelect - OpenCharacterSelect > 0;

            return new CourseSelectLoop()
            {
                DeltaSelectionX = (sbyte) (after.SelectionX - SelectionX),
                DeltaSelectionY = (sbyte) (after.SelectionY - SelectionY),
                DeltaListScroll = (sbyte) (after.ListScroll - ListScroll),
                DeltaHighlighted = (sbyte) (after.HighlightedItem - HighlightedItem),
                SubmenuDeltaSelection = (sbyte) (after.SubmenuSelection - SubmenuSelection),
                CloseSubmenu = (byte) (closeSubmenu ? 1 : 0),
                OpenSubmenu = (byte) (openSubmenu ? 1 : 0),
                OpenRaceRules = (byte) (openSettings ? 1 : 0),
                OpenCharacterSelect = (byte) (openCharacterSelect ? 1 : 0)
            };
        }

        #region Autogenerated
        public bool Equals(CourseSelectSync other)
        {
            return SelectionX == other.SelectionX && SelectionY == other.SelectionY && HighlightedItem == other.HighlightedItem && ListScroll == other.ListScroll && SubmenuSelection == other.SubmenuSelection && State == other.State && OpenCharacterSelect == other.OpenCharacterSelect;
        }

        public override bool Equals(object obj)
        {
            return obj is CourseSelectSync other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SelectionX.GetHashCode();
                hashCode = (hashCode * 397) ^ SelectionY.GetHashCode();
                hashCode = (hashCode * 397) ^ HighlightedItem.GetHashCode();
                hashCode = (hashCode * 397) ^ ListScroll.GetHashCode();
                hashCode = (hashCode * 397) ^ SubmenuSelection.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)State;
                hashCode = (hashCode * 397) ^ OpenCharacterSelect.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}