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
        private static ConcurrentBag<IReverseWrapper<AsmAction>> _actionWrappers = new ConcurrentBag<IReverseWrapper<AsmAction>>();
        private static ConcurrentBag<IReverseWrapper<AsmFunc>> _funcWrappers = new ConcurrentBag<IReverseWrapper<AsmFunc>>();


        /// <summary>
        /// Assembles the opcodes for an absolute call to a C# function without parameters.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <param name="reverseWrapper">The reverse wrapper to your function. You can discard it freely, the class will keep an instance.</param>
        public static string AssembleAbsoluteCall(AsmAction function, out IReverseWrapper<AsmAction> reverseWrapper)
        {
            var utilities = SDK.ReloadedHooks.Utilities;
            var asm = new string[]
            {
                $"{utilities.PushCdeclCallerSavedRegisters()}",
                $"{utilities.GetAbsoluteCallMnemonics<AsmAction>(function, out reverseWrapper)}",
                $"{utilities.PopCdeclCallerSavedRegisters()}",
            };

            _actionWrappers.Add(reverseWrapper);
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

            _funcWrappers.Add(reverseWrapper);
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
    public delegate bool AsmFunc();
}
