using System;
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
        public byte[] ToBytes()
        {
            using var extendedMemoryStream = new ExtendedMemoryStream();
            extendedMemoryStream.Write(CommandKind);
            extendedMemoryStream.Write(Command.ToBytes());
            return extendedMemoryStream.ToArray();
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