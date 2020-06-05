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
        public byte DeltaSelectionV;

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
    }
}