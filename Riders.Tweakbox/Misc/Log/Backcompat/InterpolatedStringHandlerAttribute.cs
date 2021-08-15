using System;
namespace System.Runtime.CompilerServices
{
    /* 
     * Implemented to add C#10 logging feature to .NET 5. 
     */
#if NET5_0
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerAttribute : Attribute
    {
        public InterpolatedStringHandlerAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public InterpolatedStringHandlerArgumentAttribute(string argument) { Arguments = new string[] { argument }; }
        public InterpolatedStringHandlerArgumentAttribute(params string[] arguments) { Arguments = arguments; }

        public string[] Arguments { get; }
    }
#endif
}