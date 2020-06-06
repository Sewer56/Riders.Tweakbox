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
            switch (command.CommandKind)
            {
                case Shared.MenuSynchronizationCommand.CourseSelectLoop:
                    command.Command = reader.Read<CourseSelectLoop>();
                    break;
                case Shared.MenuSynchronizationCommand.CourseSelectSync:
                    command.Command = reader.Read<CourseSelectSync>();
                    break;
                case Shared.MenuSynchronizationCommand.CourseSelectExit:
                    command.Command = reader.Read<CourseSelectExit>();
                    break;
                case Shared.MenuSynchronizationCommand.RuleSettingsLoop:
                    command.Command = reader.Read<RuleSettingsLoop>();
                    break;
                case Shared.MenuSynchronizationCommand.RuleSettingsSync:
                    command.Command = reader.Read<RuleSettingsSync>();
                    break;
                case Shared.MenuSynchronizationCommand.RuleSettingsExit:
                    command.Command = reader.Read<RuleSettingsExit>();
                    break;
                case Shared.MenuSynchronizationCommand.CharaSelectLoop:
                    command.Command = reader.Read<CharaSelectLoop>();
                    break;
                case Shared.MenuSynchronizationCommand.CharaSelectSync:
                    command.Command = CharaSelectSync.FromBytes(reader);
                    break;
                case Shared.MenuSynchronizationCommand.CharaSelectExit:
                    command.Command = reader.Read<CharaSelectExit>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command.CommandKind), command.CommandKind, null);
            }

            return command;
        }
    }
}