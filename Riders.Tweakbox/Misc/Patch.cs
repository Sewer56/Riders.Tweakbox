using System;
using Reloaded.Memory.Kernel32;
using Reloaded.Memory.Sources;
namespace Riders.Tweakbox.Misc;

/// <summary>
/// A simple structure that defines an address and the bytes that should be written to the address.
/// </summary>
public struct Patch
{
    public bool IsEnabled { get; private set; }

    private IntPtr _address;
    private byte[] _bytes;
    private byte[] _originalBytes;

    public Patch(long address, byte[] bytes, bool changePermission = false) : this((IntPtr)address, bytes, changePermission) { }
    public Patch(IntPtr address, byte[] bytes, bool changePermission = false)
    {
        _address = address;
        _bytes = bytes;
        _originalBytes = null;
        IsEnabled = false;

        if (changePermission)
            ChangePermission();
    }

    /// <summary>
    /// Changes permission for this memory region.
    /// </summary>
    public unsafe Patch ChangePermission()
    {
        Memory.CurrentProcess.ChangePermission(_address, _bytes.Length, Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE);
        return this;
    }

    /// <summary>
    /// Enables or disables the patch.
    /// </summary>
    /// <param name="enable">True to enable, false to disable.</param>
    public unsafe Patch Set(bool enable)
    {
        if (enable)
            Enable();
        else
            Disable();

        return this;
    }

    /// <summary>
    /// Applies the patch without changing permissions.
    /// </summary>
    public unsafe Patch Enable()
    {
        var addressSpan = new Span<byte>((void*)_address, _bytes.Length);
        _originalBytes ??= addressSpan.ToArray();

        var bytesSpan = _bytes.AsSpan();
        bytesSpan.CopyTo(addressSpan);
        IsEnabled = true;
        return this;
    }

    /// <summary>
    /// Applies the patch without changing permissions.
    /// </summary>
    public unsafe Patch Disable()
    {
        if (!IsEnabled)
            return this;

        var addressSpan = new Span<byte>((void*)_address, _originalBytes.Length);
        var bytesSpan = _originalBytes.AsSpan();
        bytesSpan.CopyTo(addressSpan);
        IsEnabled = false;
        return this;
    }
}
