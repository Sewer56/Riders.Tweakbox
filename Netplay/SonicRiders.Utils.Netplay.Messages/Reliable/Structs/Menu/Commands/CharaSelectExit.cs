using System;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CharaSelectExit : IMenuSynchronizationCommand
    {
        /// <summary>
        /// True if starting a race, else exiting the menu.
        /// </summary>
        public ExitKind Type;

        public CharaSelectExit(ExitKind type) : this()
        {
            Type = type;
        }

        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaselectExit;
        public Span<byte> ToBytes(Span<byte> buffer) => Struct.GetBytes(this, buffer);
    }

    public enum ExitKind
    {
        Null,
        Exit,
        Start
    }
}
