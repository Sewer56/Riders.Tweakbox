namespace Riders.Netplay.Messages.Misc
{
    public static class Delegates
    {
        /// <summary>
        /// Performs an action on an item passed by reference.
        /// </summary>
        /// <param name="item">The item modified.</param>
        public delegate void ActionRef<T>(ref T item);

        /// <summary>
        /// Performs an action on an item passed by reference.
        /// </summary>
        /// <param name="item">The item modified.</param>
        /// <param name="value">Value used to modify the item.</param>
        public delegate void ActionRef<T, T2>(ref T item, in T2 value);
    }
}
