using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CharaSelectExit : IMenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaSelectExit;
        public byte[] ToBytes() => Struct.GetBytes(this);

        /// <summary>
        /// If true, load stage.
        /// If false, exit.
        /// </summary>
        public bool IsLoadStage;
    }
}