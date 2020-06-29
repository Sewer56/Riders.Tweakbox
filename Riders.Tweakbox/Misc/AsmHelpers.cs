using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Misc
{
    public static class AsmHelpers
    {
        public static readonly string[] XmmRegisters = new[] { "xmm0", "xmm1", "xmm2", "xmm3", "xmm4", "xmm5", "xmm6", "xmm7" };

        private const int SizeOfXmmRegister = 16;
        private static ConcurrentBag<object> _wrapper = new ConcurrentBag<object>();

        /// <summary>
        /// Assembles code to push the value of a single XMM register.
        /// </summary>
        public static string PushXmmRegister(string register = "xmm0")
        {
            return $"sub esp, {SizeOfXmmRegister}\nvmovdqu [esp+{SizeOfXmmRegister}],{register}";
        }

        /// <summary>
        /// Assembles code to push the values of a set of XMM registers.
        /// </summary>
        public static string PushXmmRegisters(string[] registers)
        {
            string code = $"sub esp, {SizeOfXmmRegister * registers.Length}";
            for (var x = 0; x < registers.Length; x++)
                code += $"\nmovdqu [esp+{SizeOfXmmRegister * x}],{registers[x]}";

            return code;
        }

        /// <summary>
        /// Assembles code to pop the value of a single XMM register.
        /// </summary>
        public static string PopXmmRegister(string register = "xmm0")
        {
            return $"vmovdqu {register}, [esp+{SizeOfXmmRegister}]\nadd esp, {SizeOfXmmRegister}";
        }

        /// <summary>
        /// Assembles code to pop the values of a set of XMM registers.
        /// </summary>
        public static string PopXmmRegisters(string[] registers)
        {
            string code = "";
            for (var x = registers.Length - 1; x >= 0; x--)
                code += $"movdqu {registers[x]}, [esp+{SizeOfXmmRegister * x}]\n";

            code += $"add esp, {SizeOfXmmRegister * registers.Length}";
            return code;
        }

        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function without parameters.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        public static string AssembleAbsoluteCall(AsmAction function, out IReverseWrapper<AsmAction> reverseWrapper) => AssembleAbsoluteCall<AsmAction>(function, out reverseWrapper);

        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function without parameters.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        public static string AssembleAbsoluteCall<TAsmAction>(TAsmAction function, out IReverseWrapper<TAsmAction> reverseWrapper) where TAsmAction : Delegate
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var asm = new string[]
            {
                $"{utilities.PushCdeclCallerSavedRegisters()}",
                $"{utilities.GetAbsoluteCallMnemonics<TAsmAction>(function, out reverseWrapper)}",
                $"{utilities.PopCdeclCallerSavedRegisters()}",
            };

            _wrapper.Add(reverseWrapper);
            return String.Join(Environment.NewLine, asm);
        }

        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function that returns true/false.
        /// The return value is compared against a value of 1, i.e. cmp eax, 1.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
        /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
        /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
        /// <param name="condition">The condition, e.g. jump equal, jump less, jump greater.</param>
        public static string AssembleAbsoluteCall(AsmFunc function, out IReverseWrapper<AsmFunc> reverseWrapper, string[] trueInstructions, string[] falseInstructions, string[] completeInstructions, string condition = "je")
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var asm = new string[]
            {
                $"{utilities.PushCdeclCallerSavedRegisters()}",
                $"{utilities.GetAbsoluteCallMnemonics<AsmFunc>(function, out reverseWrapper)}",
                $"cmp eax, 1",
                $"{utilities.PopCdeclCallerSavedRegisters()}",
                $"{AsmHelpers.AssembleTrueFalseFinally(trueInstructions, falseInstructions, completeInstructions, condition)}"
            };

            _wrapper.Add(reverseWrapper);
            return String.Join(Environment.NewLine, asm);
        }

        /// <summary>
        /// Returns the assembly code to run following a comparison.
        /// This assembly code assumes that the `cmp` opcode has been already executed and appropriate flags set.
        /// </summary>
        /// <param name="trueInstructions">The assembly instructions to execute if the condition evaluates true.</param>
        /// <param name="falseInstructions">The assembly instructions to execute if the condition evaluates false.</param>
        /// <param name="completeInstructions">The assembly instructions to execute after true or false branch executed.</param>
        /// <param name="condition">The condition, e.g. jump equal, jump less, jump greater.</param>
        public static string AssembleTrueFalseFinally(string[] trueInstructions, string[] falseInstructions, string[] completeInstructions, string condition = "je")
        {
            if (trueInstructions == null)
                trueInstructions = new string[0];

            if (falseInstructions == null)
                falseInstructions = new string[0];

            if (completeInstructions == null)
                completeInstructions = new string[0];

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
    }

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AsmAction();

    [Function(CallingConventions.Cdecl)]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Enum<AsmFunctionResult> AsmFunc();

    public enum AsmFunctionResult : int
    {
        False = 0,
        True = 1,
        Indeterminate = 2,
    } 
}
