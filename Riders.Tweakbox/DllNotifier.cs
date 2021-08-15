using System;
using System.IO;
using Reloaded.Hooks.Definitions;
using Microsoft.Windows.Sdk;
using Reloaded.Hooks.Definitions.X86;
using Riders.Tweakbox.Misc;
namespace Riders.Tweakbox;

/// <summary>
/// Notifies when certain DLLs are being loaded.
/// </summary>
public class DllNotifier
{
    const string ErrNoTrampoline = "Hook by JMP instruction insertion. The original called by restoring instructions temporarily, which causes problems with other programs incl. Tweakbox";

    /// <summary>
    /// List of modules to warn against.
    /// </summary>
    public readonly NotifyEntry[] WarningList = new NotifyEntry[]
    {
        new NotifyEntry("RTSSHooks.dll", "RivaTuner Statistics Server", $"Most commonly ships with MSI Afterburner. Does not use trampoline for function hooks. ({ErrNoTrampoline})"),
    };

    /// <summary>
    /// List of modules to warn against.
    /// </summary>
    public readonly NotifyEntry[] BlockList = new NotifyEntry[]
    {
        new NotifyEntry("speedhack-i386.dll", "Cheat Engine Speed Hack", $"Please don't use it, it makes things really weird in Netplay. (Besides you'd be lame anyway)"),
    };

    private IHook<LdrLoadDll> _ldrLoadDllHook;

    public unsafe DllNotifier(IReloadedHooks hooks)
    {
        // This log call is important, in order to prevent an endless loop in LdrLoadDllImpl
        // We are ensuring that all DLLs used by logging are activated before the hood.
        Log.WriteLine($"[{nameof(DllNotifier)}] Initialising.");

        var ntdll = PInvoke.LoadLibrary("ntdll.dll");
        var ldrLoadDll = hooks.CreateFunction<LdrLoadDll>((long)Native.GetProcAddress(ntdll, nameof(LdrLoadDll)));

        _ldrLoadDllHook = ldrLoadDll.Hook(LdrLoadDllImpl).Activate();
    }

    private unsafe uint LdrLoadDllImpl(int searchPath, uint flags, Native.UNICODE_STRING* modulefilename, out IntPtr handle)
    {
        var moduleName = modulefilename->ToString();
        var moduleFileName = Path.GetFileName(moduleName);
        if (TestDll(moduleFileName))
        {
            handle = (IntPtr)(-1);
            return 0x0; // Lie about successfully loading DLL.
        }

        return _ldrLoadDllHook.OriginalFunction(searchPath, flags, modulefilename, out handle);
    }

    /// <summary>
    /// Tests a given DLL and presents any warnings as needed.
    /// </summary>
    /// <param name="dllName">The name of the DLL to be tested.</param>
    /// <returns>True if the dll is to be blocked, else false.</returns>
    private bool TestDll(string dllName)
    {
        // Check Warninng List
        var index = IndexOfDll(WarningList, dllName);
        if (index != -1)
        {
            var warning = WarningList[index];
            Log.WriteLine($"[WARNING!!] Unsafe library is being loaded.");
            Log.WriteLine($"DLL Name: {warning.DllName}");
            Log.WriteLine($"Name: {warning.Name}");
            Log.WriteLine($"Reason: {warning.Reason}");
            Log.WriteLine($"If Tweakbox crashes consider disabling this program or finding a way to delay its injection etc.");
        }

        // Check Block List
        index = IndexOfDll(BlockList, dllName);
        if (index != -1)
        {
            var block = BlockList[index];
            Log.WriteLine($"[ERROR!!] Blocked library is being loaded.");
            Log.WriteLine($"DLL Name: {block.DllName}");
            Log.WriteLine($"Name: {block.Name}");
            Log.WriteLine($"Reason: {block.Reason}");
            return true;
        }

        return false;
    }

    private int IndexOfDll(NotifyEntry[] source, string dllName)
    {
        for (int x = 0; x < source.Length; x++)
        {
            if (source[x].DllName.Equals(dllName, StringComparison.OrdinalIgnoreCase))
                return x;
        }

        return -1;
    }

    // Definitions
    public struct NotifyEntry
    {
        public string DllName;
        public string Name;
        public string Reason;

        public NotifyEntry(string dllName, string name, string reason)
        {
            DllName = dllName;
            Name = name;
            Reason = reason;
        }
    }

    [Function(CallingConventions.Stdcall)]
    public unsafe delegate uint LdrLoadDll(int searchPath, uint flags, Native.UNICODE_STRING* moduleFileName, out IntPtr handle);
}
