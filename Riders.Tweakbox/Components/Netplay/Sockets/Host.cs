using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    public unsafe class Host : Socket
    {
        public override SocketType GetSocketType() => SocketType.Host;

        /// <summary>
        /// Contains that is used by the server.
        /// </summary>
        public new HostState State => (HostState) base.State;
        
        public Host(NetplayConfig config, NetplayController controller) : base(controller, config)
        {
            Log.WriteLine($"[Host] Hosting Server on {config.Data.HostPort.Value} with password {config.Data.Password.Text}", LogCategory.Socket);
            base.State = new HostState(config.ToPlayerData(), this);
            Manager.StartInManualMode(config.Data.HostPort);
            Initialize();
        }
    }
}
