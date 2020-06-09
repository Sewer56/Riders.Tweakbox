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
    public struct CourseSelectSync : IMenuSynchronizationCommand
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
        /// Merges the current sync packer with a loop packet.
        /// </summary>
        public unsafe void Merge(CourseSelectLoop loop)
        {
            SubmenuSelection += loop.SubmenuDeltaSelection;
            SelectionX += loop.DeltaSelectionX;
            SelectionY += loop.DeltaSelectionY;
            ListScroll += loop.DeltaListScroll;
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
            var data = task->TaskData;
            data->SubmenuSelection = SubmenuSelection;
            data->SelectionHorizontal = SelectionX;
            data->SelectionVertical = SelectionY;
            data->ListScrollOffset = ListScroll;

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
        }

        /// <summary>
        /// Gets the sync instance from the current game given pointer to the task.
        /// </summary>
        public static unsafe CourseSelectSync FromGame(Task<CourseSelect, CourseSelectTaskState>* task)
        {
            return new CourseSelectSync()
            {
                ListScroll = task->TaskData->ListScrollOffset,
                SelectionY = task->TaskData->SelectionVertical,
                SelectionX = task->TaskData->SelectionHorizontal,
                State = task->TaskStatus,
                SubmenuSelection = task->TaskData->SubmenuSelection
            };
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

            return new CourseSelectLoop()
            {
                DeltaSelectionX = (byte) (after.SelectionX - SelectionX),
                DeltaSelectionY = (byte) (after.SelectionY - SelectionY),
                DeltaListScroll = (byte) (after.ListScroll - ListScroll),
                SubmenuDeltaSelection = (byte) (after.SubmenuSelection - SubmenuSelection),
                CloseSubmenu = (byte) (closeSubmenu ? 1 : 0),
                OpenSubmenu = (byte) (openSubmenu ? 1 : 0),
                OpenRaceRules = (byte) (openSettings ? 1 : 0)
            };
        }
    }
}