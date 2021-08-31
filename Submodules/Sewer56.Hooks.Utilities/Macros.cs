using System;
using System.Collections.Generic;

namespace Sewer56.Hooks.Utilities;

public static class Macros
{
    public static bool Is64Bit = Environment.Is64BitProcess;

    public static string _use32 = Is64Bit ? "use64" : "use32";
    public static string _eax = Is64Bit ? "rax" : "eax";
    public static string _ebx = Is64Bit ? "rbx" : "ebx";
    public static string _ecx = Is64Bit ? "rcx" : "ecx";
    public static string _edx = Is64Bit ? "rdx" : "edx";
    public static string _esi = Is64Bit ? "rsi" : "esi";
    public static string _edi = Is64Bit ? "rdi" : "edi";
    public static string _ebp = Is64Bit ? "rbp" : "ebp";
    public static string _esp = Is64Bit ? "rsp" : "esp";

    /// <summary>
    /// Dictionary that converts full sized register (e.g. rax, eax) to its low 16 bits variant (e.g. ax).
    /// </summary>
    public static Dictionary<string, string> ToShort = new Dictionary<string, string>()
    {
        { _eax, "ax" },
        { _ebx, "bx" },
        { _ecx, "cx" },
        { _edx, "dx" },
        { _esi, "si" },
        { _edi, "di" },
        { _ebp, "bp" },
        { _esp, "sp" },
    };

    /// <summary>
    /// Dictionary that converts full sized register (e.g. rax, eax) to its low 8 bits variant (e.g. al).
    /// </summary>
    public static Dictionary<string, string> ToByte = new Dictionary<string, string>()
    {
        { _eax, "al" },
        { _ebx, "bl" },
        { _ecx, "cl" },
        { _edx, "dl" },
        { _esi, "sil" },
        { _edi, "dil" },
        { _ebp, "bpl" },
        { _esp, "spl" },
    };

    /// <summary>
    /// Represents the full word operand size for current architecture.
    /// </summary>
    public static string _word = Is64Bit ? "qword" : "dword";
}
