using System;
using System.Buffers;
using System.IO;
using DotNext.Buffers;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Commands;
using Riders.Netplay.Messages.Reliable.Structs.Menu.Shared;

namespace Riders.Netplay.Messages.Reliable.Structs.Menu
{
    public struct MenuSynchronizationCommand
    {
        public Shared.MenuSynchronizationCommand CommandKind;
        public IMenuSynchronizationCommand Command;

        public MenuSynchronizationCommand(IMenuSynchronizationCommand command, Shared.MenuSynchronizationCommand commandKind)
        {
            CommandKind = commandKind;
            Command = command;
        }

        public MenuSynchronizationCommand(IMenuSynchronizationCommand command) : this()
        {
            Command = command;
            CommandKind = command.GetCommandKind();
        }

        /// <summary>
        /// Converts the synchronization command to a set of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <returns>A sliced version of the buffer.</returns>
        public unsafe Span<byte> ToBytes(Span<byte> buffer)
        {
            using var rental = new ArrayRental<byte>(64);
            fixed (byte* bytePtr = buffer)
            {
                using var unmanagedStream = new UnmanagedMemoryStream(bytePtr, buffer.Length, buffer.Length, FileAccess.Write);
                unmanagedStream.Write(CommandKind);
                unmanagedStream.Write(Command.ToBytes(rental.Span));
                return buffer.Slice(0, (int) unmanagedStream.Position);
            }
        }

        /// <param name="reader">The stream reader for the current packet.</param>
        public static MenuSynchronizationCommand FromBytes(BufferedStreamReader reader)
        {
            var command = new MenuSynchronizationCommand();
            reader.Read<Shared.MenuSynchronizationCommand>(out command.CommandKind);
            command.Command = command.CommandKind switch
            {
                Shared.MenuSynchronizationCommand.CourseSelectLoop => reader.Read<CourseSelectLoop>(),
                Shared.MenuSynchronizationCommand.CourseSelectSync => reader.Read<CourseSelectSync>(),
                Shared.MenuSynchronizationCommand.RuleSettingsLoop => reader.Read<RuleSettingsLoop>(),
                Shared.MenuSynchronizationCommand.RuleSettingsSync => reader.Read<RuleSettingsSync>(),
                Shared.MenuSynchronizationCommand.CharaSelectLoop => reader.Read<CharaSelectLoop>(),
                Shared.MenuSynchronizationCommand.CharaSelectSync => CharaSelectSync.FromBytes(reader),
                Shared.MenuSynchronizationCommand.CharaselectExit => reader.Read<CharaSelectExit>(),
                Shared.MenuSynchronizationCommand.CourseSelectSetStage => reader.Read<CourseSelectSetStage>(),
                _ => throw new ArgumentOutOfRangeException(nameof(command.CommandKind), command.CommandKind, null)
            };

            return command;
        }
    }
}