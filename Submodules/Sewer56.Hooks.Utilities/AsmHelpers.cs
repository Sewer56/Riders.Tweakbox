using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Memory.Buffers;
using Riders.Tweakbox.Misc;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using static Sewer56.Hooks.Utilities.Macros;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
namespace Sewer56.Hooks.Utilities;

/// <summary>
/// Provides X86 and X64 ASM extensions for <see cref="IReloadedHooksUtilities"/> that are not part of the standard library.
/// Intended for advanced hooking scenarios.
/// </summary>
public unsafe static class AsmHelpers
{
    private static IReloadedHooks _hooks;
    private const int SizeOfXmmRegister = 16;
    private static ConcurrentBag<object> _wrappers = new ConcurrentBag<object>();
    private static Assembler _assembler = new Assembler();
    private static readonly bool _is64Bit = IntPtr.Size == 8;

    private static string[] CdeclCallerSavedRegisters = new[]
    {
        "eax",
        "ecx",
        "edx",
    };

    /// <summary>
    /// Initialises the <see cref="AsmHelpers"/> class.
    /// </summary>
    public static void Init(IReloadedHooks hooks) => _hooks = hooks;

    /// <summary>
    /// Assembles a relative call from the current address to a given target address.
    /// </summary>
    /// <param name="currentAddress">Current address in memory.</param>
    /// <param name="targetAddress">Target address to call.</param>
    /// <returns>x86 asm bytes</returns>
    public static byte[] AssembleRelativeCall(long currentAddress, long targetAddress) => _assembler.Assemble(new[]
    {
        Architecture(_is64Bit),
        SetAddress(currentAddress),
        $"call dword {targetAddress}"
    });

    /// <summary>
    /// Macro for push eax, ecx, edx.
    /// </summary>
    public static string PushCdeclCallerSavedRegisters(this IReloadedHooksUtilities utilities) => $"push {_eax}\npush {_ecx}\npush {_edx}";

    /// <summary>
    /// Macro for push eax, ecx, edx.
    /// </summary>
    /// <param name="exception">The register to exclude.</param>
    public static string PushCdeclCallerSavedRegistersExcept(this IReloadedHooksUtilities utilities, string exception)
    {
        var builder = new StringBuilder();
        foreach (var register in CdeclCallerSavedRegisters)
        {
            if (register != exception)
                builder.AppendLine($"push {register}");
        }
        
        return builder.ToString();
    }

    /// <summary>
    /// Macro for push eax, ecx, edx.
    /// </summary>
    /// <param name="exception">The register to exclude.</param>
    public static string PopCdeclCallerSavedRegistersExcept(this IReloadedHooksUtilities utilities, string exception)
    {
        var builder = new StringBuilder();
        for (var x = CdeclCallerSavedRegisters.Length - 1; x >= 0; x--)
        {
            var register = CdeclCallerSavedRegisters[x];
            if (register != exception)
                builder.AppendLine($"pop {register}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Macro for pop edx, ecx, eax.
    /// </summary>
    public static string PopCdeclCallerSavedRegisters(this IReloadedHooksUtilities utilities) => $"pop {_edx}\npop {_ecx}\npop {_eax}";

    /// <summary>
    /// Assembles code to push the value of a single XMM register.
    /// </summary>
    public static string PushXmmRegister(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"sub {_esp}, {SizeOfXmmRegister}\nmovdqu [{_esp}],{register}";
    }

    /// <summary>
    /// Assembles code to push the value of a single XMM register.
    /// </summary>
    public static string PushXmmRegisterFloat(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"sub {_esp}, {sizeof(float)}\nmovss [{_esp}],{register}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string CopyFromX87ToXmm(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"fst dword [{_esp}]\n" +
               $"movss {register}, [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string PopFromX87ToXmm(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"fstp dword [{_esp}]\n" +
               $"movss {register}, [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string LoadFromRegisterToX87(this IReloadedHooksUtilities utilities, string register = "eax")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"mov [{_esp}], {register}\n" +
               $"fld dword [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string MultiplyFromRegisterToX87(this IReloadedHooksUtilities utilities, string register = "eax")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"mov [{_esp}], {register}\n" +
               $"fmul dword [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string CopyFromX87ToRegister(this IReloadedHooksUtilities utilities, string register = "eax")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"fst dword [{_esp}]\n" +
               $"mov {register}, [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code copy a float from the top of the x87 stack into an xmm register.
    /// </summary>
    public static string PopFromX87ToRegister(this IReloadedHooksUtilities utilities, string register = "eax")
    {
        return $"sub {_esp}, {sizeof(float)}\n" +
               $"fstp dword [{_esp}]\n" +
               $"mov {register}, [{_esp}]\n" +
               $"add {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code to push the values of all XMM registers except the one specified.
    /// </summary>
    public static string PushAllXmmRegistersExcept(this IReloadedHooksUtilities utilities, string ignoreRegister)
    {
        var registers = Constants.XmmRegisters;
        string code = $"sub {_esp}, {SizeOfXmmRegister * registers.Length}";
        for (var x = 0; x < registers.Length; x++)
        {
            if (registers[x] != ignoreRegister)
                code += $"\nmovdqu [{_esp}+{SizeOfXmmRegister * x}],{registers[x]}";
        }

        return code;
    }

    /// <summary>
    /// Assembles code to pop the values of all XMM registers except the one specified.
    /// </summary>
    public static string PopAllXmmRegistersExcept(this IReloadedHooksUtilities utilities, string ignoreRegister)
    {
        string code = "";
        var registers = Constants.XmmRegisters;

        for (var x = registers.Length - 1; x >= 0; x--)
        {
            if (registers[x] != ignoreRegister)
                code += $"movdqu {registers[x]}, [{_esp}+{SizeOfXmmRegister * x}]\n";
        }

        code += $"add {_esp}, {SizeOfXmmRegister * registers.Length}";
        return code;
    }

    /// <summary>
    /// Assembles code to push the values of a set of XMM registers.
    /// </summary>
    public static string PushXmmRegisters(this IReloadedHooksUtilities utilities, string[] registers)
    {
        string code = $"sub {_esp}, {SizeOfXmmRegister * registers.Length}";
        for (var x = 0; x < registers.Length; x++)
            code += $"\nmovdqu [{_esp}+{SizeOfXmmRegister * x}],{registers[x]}";

        return code;
    }

    /// <summary>
    /// Assembles code to pop the value of a single XMM register.
    /// </summary>
    public static string PopXmmRegister(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"movdqu {register}, [{_esp}]\nadd {_esp}, {SizeOfXmmRegister}";
    }

    /// <summary>
    /// Assembles code to push the value of a single XMM register.
    /// </summary>
    public static string PopXmmRegisterFloat(this IReloadedHooksUtilities utilities, string register = "xmm0")
    {
        return $"movss {register}, [{_esp}]\nadd {_esp}, {sizeof(float)}";
    }

    /// <summary>
    /// Assembles code to pop the values of a set of XMM registers.
    /// </summary>
    public static string PopXmmRegisters(this IReloadedHooksUtilities utilities, string[] registers)
    {
        string code = "";
        for (var x = registers.Length - 1; x >= 0; x--)
            code += $"movdqu {registers[x]}, [{_esp}+{SizeOfXmmRegister * x}]\n";

        code += $"add {_esp}, {SizeOfXmmRegister * registers.Length}";
        return code;
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function without parameters.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="function">The function to execute.</param>
    public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, AsmAction function) => AssembleAbsoluteCall<AsmAction>(utilities, function);

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function without parameters.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="function">The function to execute.</param>
    /// <param name="callerSaveRegisters">Whether to caller save registers.</param>
    public static string AssembleAbsoluteCall<TAsmAction>(this IReloadedHooksUtilities utilities, TAsmAction function, bool callerSaveRegisters = true) where TAsmAction : Delegate
    {
        var reverseWrapper = _hooks.CreateReverseWrapper<TAsmAction>(function);
        _wrappers.Add(reverseWrapper);
        return AssembleAbsoluteCall(utilities, (void*) reverseWrapper.WrapperPointer, callerSaveRegisters);
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function without parameters.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="name">Name of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="type">Type of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="callerSaveRegisters">Whether to caller save registers.</param>
    public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, Type type, string name, bool callerSaveRegisters = true) => AssembleAbsoluteCall(utilities, utilities.GetFunctionPointer(type, name), callerSaveRegisters);

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="name">Name of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="type">Type of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="callerSaveRegisters">Whether to caller save registers.</param>
    public static string AssembleAbsoluteCall<TAsmFunction>(this IReloadedHooksUtilities utilities, Type type, string name, bool callerSaveRegisters = true)
    {
        var ptr     = utilities.GetFunctionPointer(type, name);
        var wrapper = _hooks.CreateReverseWrapper<TAsmFunction>((IntPtr) ptr);
        
        return AssembleAbsoluteCall(utilities, (void*) wrapper.WrapperPointer, callerSaveRegisters);
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function without parameters.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="function">The function to execute.</param>
    /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
    /// <param name="callerSaveRegisters">Whether to caller save registers.</param>
    public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, void* function, bool callerSaveRegisters = true)
    {
        var asm = callerSaveRegisters ? new string[] 
        {
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.GetAbsoluteCallMnemonics((IntPtr) function, _is64Bit)}",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
        }
        : new string[]
        {
            $"{utilities.GetAbsoluteCallMnemonics((IntPtr) function, _is64Bit)}",
        };
        
        return String.Join(Environment.NewLine, asm);
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function that returns true/false.
    /// The return value is compared against a value of 1, i.e. cmp eax, 1.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="function">The function to execute.</param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, AsmFunc function, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions)
    {
        var reverseWrapper = _hooks.CreateReverseWrapper<AsmFunc>(function);
        _wrappers.Add(reverseWrapper);
        return AssembleAbsoluteCall(utilities, (void*)reverseWrapper.WrapperPointer, trueInstructions, falseInstructions, completeInstructions);
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function that returns true/false.
    /// The return value is compared against a value of 1, i.e. cmp eax, 1.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="name">Name of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="type">Type of the function containing the function pointer [UnmanagedCallersOnly]</param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    public static string AssembleAbsoluteCall<TAsmFunction>(this IReloadedHooksUtilities utilities, Type type, string name, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions)
    {
        var ptr     = utilities.GetFunctionPointer(type, name);
        var wrapper = _hooks.CreateReverseWrapper<TAsmFunction>((IntPtr)ptr);

        return AssembleAbsoluteCall(utilities, (void*)wrapper.WrapperPointer, trueInstructions, falseInstructions, completeInstructions);
    }

    /// <summary>
    /// Assembles the opcodes for an absolute call to a C# function that returns true/false.
    /// The return value is compared against a value of 1, i.e. cmp eax, 1.
    /// </summary>
    /// <param name="utilities"/>
    /// <param name="function">The function to execute.</param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, void* function, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions)
    {
        var asm = new string[]
        {
            $"{utilities.PushCdeclCallerSavedRegisters()}",
            $"{utilities.GetAbsoluteCallMnemonics((IntPtr)function, _is64Bit)}",
            $"cmp {_eax}, 1",
            $"{utilities.PopCdeclCallerSavedRegisters()}",
            $"{utilities.AssembleTrueFalseForAsmFunctionResult(trueInstructions, falseInstructions, completeInstructions)}"
        };
        
        return String.Join(Environment.NewLine, asm);
    }

    /// <summary>
    /// Returns the assembly code to run following a comparison.
    /// This assembly code assumes that the `cmp` opcode has been already executed and appropriate flags set.
    /// </summary>
    /// <param name="utilities"></param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    /// <param name="condition">The condition to evaluate to true; e.g. "je"</param>
    public static string AssembleTrueFalseComplete(this IReloadedHooksUtilities utilities, string trueInstructions, string falseInstructions, string completeInstructions, string condition = "je")
    {
        trueInstructions ??= "";
        falseInstructions ??= "";
        completeInstructions ??= "";

        return String.Join(Environment.NewLine, new[]
        {
            $"{condition} true",
            $"false:",
            $"{falseInstructions}",
            $"jmp complete",

            $"true:",
            $"{trueInstructions}",

            $"complete:",
            $"{completeInstructions}",
        });
    }

    /// <summary>
    /// Returns the assembly code to run following a comparison.
    /// This assembly code assumes that the `cmp` opcode has been already executed and appropriate flags set.
    /// </summary>
    /// <param name="utilities"></param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    /// <param name="condition">The condition to evaluate to true; e.g. "je"</param>
    public static string AssembleTrueFalseComplete(this IReloadedHooksUtilities utilities, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions, string condition = "je")
    {
        trueInstructions ??= new string[0];
        falseInstructions ??= new string[0];
        completeInstructions ??= new string[0];

        return AssembleTrueFalseComplete(utilities,
            String.Join(Environment.NewLine, trueInstructions),
            String.Join(Environment.NewLine, falseInstructions),
            String.Join(Environment.NewLine, completeInstructions), 
            condition);
    }

    /// <summary>
    /// Returns the assembly code to run following a comparison.
    /// This assembly code assumes that the `cmp` opcode has been already executed and appropriate flags set.
    /// To be used with return values of <see cref="AsmFunctionResult"/> after a cmp has been made with them.
    /// </summary>
    /// <param name="utilities"></param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    public static string AssembleTrueFalseForAsmFunctionResult(this IReloadedHooksUtilities utilities, string trueInstructions, string falseInstructions, string completeInstructions)
    {
        trueInstructions ??= "";
        falseInstructions ??= "";
        completeInstructions ??= "";

        return String.Join(Environment.NewLine, new[]
        {
            $"jg complete",
            $"{AssembleTrueFalseComplete(utilities, trueInstructions, falseInstructions, completeInstructions, "je")}"
        });
    }

    /// <summary>
    /// Returns the assembly code to run following a comparison.
    /// This assembly code assumes that the `cmp` opcode has been already executed and appropriate flags set.
    /// To be used with return values of <see cref="AsmFunctionResult"/> after a cmp has been made with them.
    /// </summary>
    /// <param name="utilities"></param>
    /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
    /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
    /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
    public static string AssembleTrueFalseForAsmFunctionResult(this IReloadedHooksUtilities utilities, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions)
    {
        trueInstructions ??= new string[0];
        falseInstructions ??= new string[0];
        completeInstructions ??= new string[0];

        return AssembleTrueFalseForAsmFunctionResult(utilities, 
            String.Join(Environment.NewLine, trueInstructions), 
            String.Join(Environment.NewLine, falseInstructions), 
            String.Join(Environment.NewLine, completeInstructions));
    }

    private static string Architecture(bool is64bit) => is64bit ? "use64" : "use32";
    private static string SetAddress(long address) => $"org {address}";

    /// <summary>
    /// Invokes the function if it's not null, else does nothing.
    /// </summary>
    public static Enum<AsmFunctionResult> InvokeIfNotNull(this AsmFunc func)
    {
        if (func != null)
            return func();

        return AsmFunctionResult.Indeterminate;
    }

    /// <summary>
    /// Finds an existing <see cref="MemoryBuffer"/> or creates one satisfying the given size.
    /// </summary>
    /// <param name="size">The required size of buffer.</param>
    /// <param name="minimumAddress">Maximum address of the buffer.</param>
    /// <param name="maximumAddress">Minimum address of the buffer.</param>
    /// <param name="alignment">Required alignment of the item to add to the buffer.</param>
    public static MemoryBuffer FindOrCreateBufferInRange(this MemoryBufferHelper helper, int size, long minimumAddress = 1, long maximumAddress = int.MaxValue, int alignment = 4)
    {
        var buffers = helper.FindBuffers(size + alignment, (IntPtr)minimumAddress, (IntPtr)maximumAddress);
        return buffers.Length > 0 ? buffers[0] : helper.CreateMemoryBuffer(size, minimumAddress, maximumAddress);
    }

    /// <summary>
    /// Allocates an aligned piece of memory.
    /// </summary>
    /// <param name="helper">The buffer helper.</param>
    /// <param name="size">Size of memory to allocate.</param>
    /// <param name="alignment">Alignment of memory to allocate.</param>
    /// <returns></returns>
    public static uint AllocateAligned(this MemoryBufferHelper helper, int size, int alignment)
    {
        var buffer = helper.FindOrCreateBufferInRange(size, 1, int.MaxValue, alignment);
        return (uint) buffer.ExecuteWithLock(() =>
        {
            return buffer.Add(size, alignment);
        });
    }
}

[Function(CallingConventions.Cdecl)]
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void AsmAction();

[Function(CallingConventions.Cdecl)]
public struct AsmActionPtr { public FuncPtr<Reloaded.Hooks.Definitions.Structs.Void> Value; }

[Function(CallingConventions.Cdecl)]
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate Enum<AsmFunctionResult> AsmFunc();

[Function(CallingConventions.Cdecl)]
public struct AsmFuncPtr { public FuncPtr<Enum<AsmFunctionResult>> Value; }
