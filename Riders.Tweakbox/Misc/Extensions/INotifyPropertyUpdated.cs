using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Riders.Tweakbox.Misc.Extensions
{
    /// <summary>
    /// A variant of <see cref="INotifyPropertyChanged"/> that allows for external callers to fire events.
    /// </summary>
    public interface INotifyPropertyUpdated
    {
        protected PropertyChangedEventHandler m_PropertyUpdated { get; set; }

        /// <summary>
        /// Invoked when a property has been changed on an element.
        /// </summary>
        public event PropertyChangedEventHandler PropertyUpdated
        {
            add
            {
                lock (this)
                    m_PropertyUpdated += value;
            }
            remove
            {
                lock (this)
                    m_PropertyUpdated -= value;
            }
        }

        /// <summary>
        /// Raises the <seealso cref="PropertyUpdated"/> event for an individual item.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        public void RaisePropertyUpdated(string propName) => m_PropertyUpdated?.Invoke(propName);
    }

    /// <summary>
    /// Extensions to the custom version of <see cref="INotifyPropertyChanged"/>, <see cref="INotifyPropertyUpdated"/>.
    /// </summary>
    public static class INotifyPropertyUpdatedExtensions 
    {
        /// <summary>
        /// Raises an event that a property has changed.
        /// </summary>
        /// <param name="item">The item on which the property was raised.</param>
        /// <param name="property">The event to add..</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPropertyUpdatedHandler<T>(this T item, PropertyChangedEventHandler property) where T : INotifyPropertyUpdated 
            => item.PropertyUpdated += property;

        /// <summary>
        /// Raises an event that a property has changed.
        /// </summary>
        /// <param name="item">The item on which the property was raised.</param>
        /// <param name="property">The event to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemovePropertyUpdatedHandler<T>(this T item, PropertyChangedEventHandler property) where T : INotifyPropertyUpdated 
            => item.PropertyUpdated -= property;

        /// <summary>
        /// Raises an event that a property has changed.
        /// </summary>
        /// <param name="item">The item on which the property was raised.</param>
        /// <param name="prop">Name of the property.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RaisePropertyUpdated<T>(this T item, string prop) where T : INotifyPropertyUpdated 
            => item.RaisePropertyUpdated(prop);

        /// <summary>
        /// Notifies if a property has changed.
        /// </summary>
        /// <param name="hasChanged">True if it has changed and PropertyChanged should be invoked.</param>
        /// <param name="item">The item whose property has changed.</param>
        /// <param name="propName">Name of the property.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify<T>(this bool hasChanged, T item, string propName) where T : INotifyPropertyUpdated
        {
            if (hasChanged)
                item.RaisePropertyUpdated(propName);
        }
    }

    /// <summary>
    /// Base class that adds the required field.
    /// </summary>
    public class NotifyPropertyChangedBase : INotifyPropertyUpdated
    {
        /// <inheritdoc />
        PropertyChangedEventHandler INotifyPropertyUpdated.m_PropertyUpdated { get; set; }
    }

    /// <summary>
    /// Lite version of INotifyPropertyChanged.
    /// </summary>
    public delegate void PropertyChangedEventHandler(string propertyName);
}