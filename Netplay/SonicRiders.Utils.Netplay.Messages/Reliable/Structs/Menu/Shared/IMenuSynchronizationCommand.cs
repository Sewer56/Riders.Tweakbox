namespace Riders.Netplay.Messages.Reliable.Structs.Menu.Shared
{
    // Dummy interface
    public interface IMenuSynchronizationCommand
    {
        Shared.MenuSynchronizationCommand GetCommandKind();
        byte[] ToBytes();
    }
}