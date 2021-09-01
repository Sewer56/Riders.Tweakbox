using Riders.Tweakbox.Components.Netplay.Menus;

namespace Riders.Tweakbox.Components.Debug;

public class ChatMenuDebug : ComponentBase
{
    public override string Name { get; set; } = "Chat Menu Debug";
    public ChatMenu Chat;

    public ChatMenuDebug()
    {
        Chat = new ChatMenu(() => "Lianne Sandlot", s => { }, () => IsEnabled());
    }

    public override void Render() { }
}
