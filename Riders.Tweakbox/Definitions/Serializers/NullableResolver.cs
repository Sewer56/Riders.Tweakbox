using System;
using MessagePack;
using MessagePack.Formatters;

namespace Riders.Tweakbox.Definitions.Serializers
{
    public class NullableResolver : IFormatterResolver
    {
        // Resolver should be singleton.
        public static readonly IFormatterResolver Instance = new NullableResolver();
        private NullableResolver() { }

        // GetFormatter<T>'s get cost should be minimized so use type cache.
        public IMessagePackFormatter<T> GetFormatter<T>() => FormatterCache<T>.Formatter;

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter;

            // generic's static constructor should be minimized for reduce type generation size!
            // use outer helper method.
            static FormatterCache() => Formatter = (IMessagePackFormatter<T>)SampleCustomResolverGetFormatterHelper.GetFormatter(typeof(T));
        }
    }

    internal static class SampleCustomResolverGetFormatterHelper
    {
        internal static object GetFormatter(Type t)
        {
            // If target type is generics, use MakeGenericType.
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Activator.CreateInstance(typeof(NullableFormatter<>).MakeGenericType(t.GenericTypeArguments));

            // If type can not get, must return null for fallback mechanism.
            return null;
        }
    }
}
