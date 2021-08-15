using System;
using Riders.Netplay.Messages.Misc.Interfaces;
namespace Riders.Netplay.Messages.Helpers;

/// <summary>
/// Copies a sequence number from an external source.
/// </summary>
public struct SequenceNumberCopy<T> : ISequenced, IDisposable where T : ISequenced, new()
{
    private static readonly int _maxValue = new T().MaxValue;

    /// <inheritdoc />
    public int MaxValue => _maxValue;

    /// <inheritdoc />
    public int SequenceNo { get; private set; }

    public static SequenceNumberCopy<T> Create(in T value)
    {
        return new SequenceNumberCopy<T>()
        {
            SequenceNo = value.SequenceNo
        };
    }

    /// <inheritdoc />
    public void Dispose() { }
}
