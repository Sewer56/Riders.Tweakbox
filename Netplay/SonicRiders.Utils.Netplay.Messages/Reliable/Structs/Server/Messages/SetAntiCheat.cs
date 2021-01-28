using System;
using Reloaded.Memory;
using Riders.Netplay.Messages.Reliable.Structs.Gameplay.Shared;
using Riders.Netplay.Messages.Reliable.Structs.Server.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Messages
{
    /// <summary>
    /// Sets the Anti-cheat types for this server.
    /// </summary>
    public struct SetAntiCheat : IServerMessage
    {
        public ServerMessageType GetMessageType() => ServerMessageType.HasSetAntiCheatTypes;
        public Span<byte> ToBytes(Span<byte> buffer) => Struct.GetBytes(this, buffer);

        public CheatKind Cheats { get; set; }
    }
}
