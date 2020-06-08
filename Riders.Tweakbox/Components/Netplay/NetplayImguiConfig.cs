using Riders.Tweakbox.Misc;
using Sewer56.Imgui.Utilities;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayImguiConfig
    {
        private const int TextLength = 32;
        private const int IPLength   = 15;

        public TextInputData PlayerName;
        public TextInputData Password;
        public TextInputData ClientIP;
        public int HostPort;
        public int ClientPort;
        public bool ShowPlayers;

        public NetplayImguiConfig(NetplayConfigFile netplayConfigFile)
        {
            PlayerName = new TextInputData(netplayConfigFile.PlayerName, TextLength);
            Password = new TextInputData(netplayConfigFile.Password, TextLength);
            ClientIP = new TextInputData(netplayConfigFile.ClientIP, IPLength, sizeof(byte));
            ShowPlayers = netplayConfigFile.ShowPlayers;
            HostPort = netplayConfigFile.HostPort;
            ClientPort = netplayConfigFile.ClientPort;
        }

        public NetplayConfigFile GetConfig()
        {
            return new NetplayConfigFile
            {
                Password = Password.GetText(),
                PlayerName = PlayerName.GetText(),
                ClientIP = ClientIP.GetText(),
                ClientPort = ClientPort,
                HostPort = HostPort,
                ShowPlayers = ShowPlayers
            };
        }

        public void Save() => IoC.Get<IO>().SaveNetplayConfig(GetConfig());
    }
}
