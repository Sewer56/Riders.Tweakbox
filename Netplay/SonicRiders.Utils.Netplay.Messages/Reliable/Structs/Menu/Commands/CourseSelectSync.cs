using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Menus.Enums;

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
        public byte SelectionV;

        /// <summary>
        /// List scroll offset.
        /// </summary>
        public byte ListScroll;

        /// <summary>
        /// Submenu selection.
        /// </summary>
        public byte SubmenuSelection;

        /// <summary>
        /// Current Task State.
        /// </summary>
        public CourseSelectTaskState TaskState;
    }
}