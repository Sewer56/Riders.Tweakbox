using System.Collections.Concurrent;
using System.Linq;
using Ninject;

namespace Riders.Netplay.Messages.Queue
{
    /// <summary>
    /// Represents a simple message queue.
    /// </summary>
    public class MessageQueue
    {
        private IKernel _kernel = new StandardKernel();

        /// <summary>
        /// Gets a queue for a specified packet.
        /// If one doesn't exist, creates a new queue.
        /// </summary>
        /// <typeparam name="T">The type of packet to get the queue for.</typeparam>
        /// <returns>The queue in question.</returns>
        public ConcurrentQueue<T> Get<T>()
        {
            if (IsBound<T>())
                return _kernel.Get<ConcurrentQueue<T>>();

            var queue = new ConcurrentQueue<T>();
            _kernel.Bind<ConcurrentQueue<T>>().ToConstant(queue);
            return queue;
        }

        /// <summary>
        /// Removes any bindings to a specific queue type.
        /// Note: Consider just clearing the queue instead.
        /// </summary>
        public void Clear<T>()
        {
            _kernel.Unbind<ConcurrentQueue<T>>();
        }

        /// <summary>
        /// Returns true if a queue for a specific type exists, else false.
        /// </summary>
        public bool IsBound<T>()
        {
            return !_kernel.GetBindings(typeof(ConcurrentQueue<T>)).All(x => x.IsImplicit);
        }
    }
}
