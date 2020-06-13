using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using Sewer56.SonicRiders.Structures.Tasks.Enums.Shared;
using Sewer56.SonicRiders.Structures.Tasks.Enums.States;

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

        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaselectStart;
        public byte[] ToBytes() => Struct.GetBytes(this);
    }

    public enum ExitKind
    {
        Null,
        Exit,
        Start
    }
}
