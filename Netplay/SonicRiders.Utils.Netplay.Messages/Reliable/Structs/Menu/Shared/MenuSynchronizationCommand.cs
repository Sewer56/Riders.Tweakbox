namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Shared
{
    public enum MenuSynchronizationCommand : byte
    {
        // Every frame of Track Select
        CourseSelectLoop, // Client -> Host
        CourseSelectSync, // Host   -> Client

        // Rule Settings
        RuleSettingsLoop, // Client -> Host
        RuleSettingsSync, // Host   -> Client
        
        // Player Entry
        CharaSelectLoop, // Client -> Host
        CharaSelectSync, // Host   -> Client
        CharaselectStart, // Client -> Host & Host -> Client.
    }
}