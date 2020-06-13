using System.Linq;
using Ninject;
using Riders.Tweakbox.Components.Netplay;
using Riders.Tweakbox.Controllers;

namespace Riders.Tweakbox.Misc
{
    public static class IoC
    {
        /// <summary>
        /// The standard NInject Kernel.
        /// </summary>
        public static IKernel Kernel { get; } = new StandardKernel();

        /// <summary>
        /// Initializes global bindings.
        /// </summary>
        public static void Initialize(string modFolder)
        {
            var io = new IO(modFolder);
            Kernel.Bind<IO>().ToConstant(io);
            Kernel.Bind<NetplayConfigFile>().ToConstant(io.GetNetplayConfig());
            GetConstant<NetplayImguiConfig>();
            GetConstant<EventController>();
        }

        /// <summary>
        /// Retrieves a service (class) from the IoC of a specified type.
        /// </summary>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }

        /// <summary>
        /// Retrieves a constant service/class.
        /// If none is registered, binds it as the new constant to then be re-acquired.
        /// </summary>
        public static T GetConstant<T>()
        {
            var value = Kernel.Get<T>();

            if (!IsExplicitlyBound<T>())
            {
                Kernel.Bind<T>().ToConstant(value);
            }

            return value;
        }

        /// <summary>
        /// Returns true if a type has been bound by the user, else false.
        /// </summary>
        public static bool IsExplicitlyBound<T>()
        {
            return !Kernel.GetBindings(typeof(T)).All(x => x.IsImplicit);
        }
    }
}
