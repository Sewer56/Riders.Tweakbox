using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CourseSelectLoop : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CourseSelectLoop;
        public byte[] ToBytes() => Struct.GetBytes(this);

        /// <summary>
        /// Change in X selection.
        /// </summary>
        public byte DeltaSelectionX;

        /// <summary>
        /// Change in Y selection.
        /// </summary>
        public byte DeltaSelectionY;

        /// <summary>
        /// Change in list scroll offset.
        /// </summary>
        public byte DeltaListScroll;

        /// <summary>
        /// Change in submenu selection.
        /// </summary>
        public byte SubmenuDeltaSelection;

        /// <summary>
        /// Opens the Submenu.
        /// </summary>
        public byte OpenSubmenu;

        /// <summary>
        /// Closes the Submenu.
        /// </summary>
        public byte CloseSubmenu;

        /// <summary>
        /// Opens the Race Rules menu.
        /// </summary>
        public byte OpenRaceRules;

        /// <summary>
        /// Adds the contents of two loops together.
        /// </summary>
        public CourseSelectLoop Add(CourseSelectLoop loop)
        {
            return new CourseSelectLoop()
            {
                CloseSubmenu = (byte) (CloseSubmenu + loop.CloseSubmenu),
                DeltaListScroll = (byte) (DeltaListScroll + loop.DeltaListScroll),
                DeltaSelectionY = (byte) (DeltaSelectionY + loop.DeltaSelectionY),
                DeltaSelectionX = (byte) (DeltaSelectionX + loop.DeltaSelectionX),
                OpenSubmenu = (byte) (OpenSubmenu + loop.OpenSubmenu),
                SubmenuDeltaSelection = (byte) (SubmenuDeltaSelection + loop.SubmenuDeltaSelection),
                OpenRaceRules = (byte)(OpenRaceRules + loop.OpenRaceRules)
            };
        }
    }
}