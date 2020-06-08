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

        public NetplayImguiConfig(NetplayConfig netplayConfig)
        {
            PlayerName = new TextInputData(netplayConfig.PlayerName, TextLength);
            Password = new TextInputData(netplayConfig.Password, TextLength);
            ClientIP = new TextInputData(netplayConfig.ClientIP, IPLength, sizeof(byte));
            ShowPlayers = netplayConfig.ShowPlayers;
            HostPort = netplayConfig.HostPort;
            ClientPort = netplayConfig.ClientPort;
        }

        public void Save()
        {
            var config = new NetplayConfig
            {
                Password = Password.GetText(),
                PlayerName = PlayerName.GetText(),
                ClientIP = ClientIP.GetText(),
                ClientPort = ClientPort,
                HostPort = HostPort,
                ShowPlayers = ShowPlayers
            };

            IoC.Get<IO>().SaveNetplayConfig(config);
        }
    }
}
