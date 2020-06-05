using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CourseSelectExit : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CourseSelectExit;
        public byte[] ToBytes() => Struct.GetBytes(this);

        public CourseSelectExitType ExitType;
    }

    public enum CourseSelectExitType : byte
    {
        Exit,
        EnterChara,
        EnterRule
    }
}