using System;
using Riders.Tweakbox.Components.Netplay.Sockets.Helpers;
using Riders.Tweakbox.Controllers;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.Utility;

namespace Riders.Tweakbox.Components.Netplay.Sockets
{
    /// <inheritdoc />
    public unsafe class Client : Socket
    {
        public override SocketType GetSocketType() => SocketType.Client;

        public Client(NetplayConfig config, NetplayController controller) : base(controller, config)
        {
            Log.WriteLine($"[Client] Joining Server on {config.Data.ClientIP.Text}:{config.Data.ClientPort} with password {config.Data.Password.Text}", LogCategory.Socket);
            if (Event.LastTask != Tasks.CourseSelect)
                throw new Exception("You are only allowed to join the host in the Course Select Menu");

            Manager.StartInManualMode(0);
            State = new CommonState(config.ToPlayerData(), this);
            Manager.Connect(config.Data.ClientIP.Text, config.Data.ClientPort, config.Data.Password.Text);
            Initialize();
        }
    }
}
