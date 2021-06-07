using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Reloaded.Assembler;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Sewer56.Hooks.Utilities.Enums;
using Sewer56.NumberUtilities.Helpers;
using static Sewer56.Hooks.Utilities.Macros;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;

namespace Sewer56.Hooks.Utilities
{
    /// <summary>
    /// Provides X86 and X64 ASM extensions for <see cref="IReloadedHooksUtilities"/> that are not part of the standard library.
    /// Intended for advanced hooking scenarios.
    /// </summary>
    public static class AsmHelpers
    {
        private const int SizeOfXmmRegister = 16;
        private static ConcurrentBag<object> _wrappers = new ConcurrentBag<object>();
        private static Assembler _assembler = new Assembler();

        /// <summary>
        /// Assembles a relative call from the current address to a given target address.
        /// </summary>
        /// <param name="currentAddress">Current address in memory.</param>
        /// <param name="targetAddress">Target address to call.</param>
        /// <returns>x86 asm bytes</returns>
        public static byte[] AssembleRelativeCall(long currentAddress, long targetAddress) => _assembler.Assemble(new[]
        {
            Architecture(false),
            SetAddress(currentAddress),
            $"call dword {targetAddress}"
        });

        /// <summary>
        /// Macro for push eax, ecx, edx.
        /// </summary>
        public static string PushCdeclCallerSavedRegisters(this IReloadedHooksUtilities utilities) => $"push {_eax}\npush {_ecx}\npush {_edx}";

        /// <summary>
        /// Macro for pop edx, ecx, eax.
        /// </summary>
        public static string PopCdeclCallerSavedRegisters(this IReloadedHooksUtilities utilities) => $"pop {_edx}\npop {_ecx}\npop {_eax}";

        /// <summary>
        /// Assembles code to push the value of a single XMM register.
        /// </summary>
        public static string PushXmmRegister(this IReloadedHooksUtilities utilities, string register = "xmm0")
        {
            return $"sub {_esp}, {SizeOfXmmRegister}\nvmovdqu [{_esp}+{SizeOfXmmRegister}],{register}";
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
            return $"vmovdqu {register}, [{_esp}+{SizeOfXmmRegister}]\nadd {_esp}, {SizeOfXmmRegister}";
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
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, AsmAction function, out IReverseWrapper<AsmAction> reverseWrapper) => AssembleAbsoluteCall<AsmAction>(utilities, function, out reverseWrapper);

        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function without parameters.
        /// </summary>
        /// <param name="utilities"/>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        /// <param name="callerSaveRegisters">Whether to caller save registers.</param>
        public static string AssembleAbsoluteCall<TAsmAction>(this IReloadedHooksUtilities utilities, TAsmAction function, out IReverseWrapper<TAsmAction> reverseWrapper, bool callerSaveRegisters = true) where TAsmAction : Delegate
        {
            var asm = callerSaveRegisters
                ? new string[]
                {
                    $"{utilities.PushCdeclCallerSavedRegisters()}",
                    $"{utilities.GetAbsoluteCallMnemonics<TAsmAction>(function, out reverseWrapper)}",
                    $"{utilities.PopCdeclCallerSavedRegisters()}",
                }
                : new string[]
                {
                    $"{utilities.GetAbsoluteCallMnemonics<TAsmAction>(function, out reverseWrapper)}",
                };

            _wrappers.Add(reverseWrapper);
            return String.Join(Environment.NewLine, asm);
        }

        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function that returns true/false.
        /// The return value is compared against a value of 1, i.e. cmp eax, 1.
        /// </summary>
        /// <param name="utilities"/>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
        /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
        /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
        /// <param name="condition">The condition, e.g. jump equal, jump less, jump greater.</param>
        public static string AssembleAbsoluteCall(this IReloadedHooksUtilities utilities, AsmFunc function, out IReverseWrapper<AsmFunc> reverseWrapper, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions, string condition = "je")
        {
            var asm = new string[]
            {
                $"{utilities.PushCdeclCallerSavedRegisters()}",
                $"{utilities.GetAbsoluteCallMnemonics<AsmFunc>(function, out reverseWrapper)}",
                $"cmp {_eax}, 1",
                $"{utilities.PopCdeclCallerSavedRegisters()}",
                $"{utilities.AssembleTrueFalseFinally(trueInstructions, falseInstructions, completeInstructions, condition)}"
            };

            _wrappers.Add(reverseWrapper);
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
        /// <param name="condition">The condition, e.g. jump equal, jump less, jump greater.</param>
        public static string AssembleTrueFalseFinally(this IReloadedHooksUtilities utilities, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions, string condition = "je")
        {
            trueInstructions ??= new string[0];
            falseInstructions ??= new string[0];
            completeInstructions ??= new string[0];

            return String.Join(Environment.NewLine, new[]
            {
                $"jg complete",
                $"{condition} true",
                $"false:",
                $"{String.Join(Environment.NewLine, falseInstructions)}",
                $"jmp complete",

                $"true:",
                $"{String.Join(Environment.NewLine, trueInstructions)}",

                $"complete:",
                $"{String.Join(Environment.NewLine, completeInstructions)}",
            });
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
    }

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AsmAction();

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Enum<AsmFunctionResult> AsmFunc();
}
