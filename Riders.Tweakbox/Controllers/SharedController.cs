using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Controllers
{
    public static class SharedController
    {
        /// <summary>
        /// True if is in Netplay mode.
        /// </summary>
        public static bool NetplayEnabled = IoC.GetConstant<NetplayController>().IsConnected();
    }
}