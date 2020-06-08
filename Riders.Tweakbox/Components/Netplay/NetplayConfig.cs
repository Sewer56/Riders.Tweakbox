using System;

namespace Riders.Tweakbox.Components.Netplay
{
    public class NetplayConfig
    {
        public string PlayerName    { get; set; } = "Pixel";
        public string Password      { get; set; } = String.Empty;
        public string ClientIP      { get; set; } = "127.0.0.1";
        public int ClientPort       { get; set; } = 42069;
        public int HostPort         { get; set; } = 42069;
        public bool ShowPlayers     { get; set; } = false;
    }
}
