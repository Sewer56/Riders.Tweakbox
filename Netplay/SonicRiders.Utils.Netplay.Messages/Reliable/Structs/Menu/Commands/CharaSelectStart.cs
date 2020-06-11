using System;
using System.Collections.Generic;
using System.Text;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Commands
{
    public struct CharaSelectStart : IMenuSynchronizationCommand
    {
        public bool Dummy;

        public Shared.MenuSynchronizationCommand GetCommandKind() => Shared.MenuSynchronizationCommand.CharaselectStart;
        public byte[] ToBytes() => Struct.GetBytes(this);
    }
}
