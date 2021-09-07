using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Buffers;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Log;
using Sewer56.Hooks.Utilities;
using Sewer56.SonicRiders;

namespace Riders.Tweakbox.Controllers.CustomGearController.Behaviour;

public unsafe class CustomGearExtendedLevelsPatches
{
    private static CustomGearGameplayHooks _owner;
    private List<IAsmHook> _hooks = new List<IAsmHook>();
    private MemoryBufferHelper _memoryBufferHelper;

    internal CustomGearExtendedLevelsPatches(CustomGearGameplayHooks owner)
    {
        _owner = owner;
        _memoryBufferHelper = new MemoryBufferHelper(Process.GetCurrentProcess());
        var hooks = SDK.ReloadedHooks;

        const int fpuAlignment = 16;
        const int fpuBytes = 512;
        var fpuBackupAllocation = _memoryBufferHelper.AllocateAligned(fpuBytes, fpuAlignment);

        foreach (var patch in Patches)
        {
            var code = AssembleAsmPatch(patch, hooks, fpuBackupAllocation);
            _hooks.Add(hooks.CreateAsmHook(code, patch.Address, new AsmHookOptions()
            {
                Behaviour = AsmHookBehaviour.DoNotExecuteOriginal,
                MaxOpcodeSize = 5,
                PreferRelativeJump = true
            }).Activate());
        }
    }

    /// <summary>
    /// Translates an offset in the player struct pointing to level stats to a new location.
    /// </summary>
    /// <param name="playerPtr">Pointer to the player struct.</param>
    /// <param name="offset">Offset into the player struct.</param>
    [UnmanagedCallersOnly]
    public static void* GetNewAddress(byte* dataPtr)
    {
        // Get target & offset from level data.
        int playerIndex = Sewer56.SonicRiders.API.Player.GetPlayerIndexFromMidStructPtr(dataPtr, out var playerPtr);
        if (playerIndex == -1)
        {
            Log.WriteLine($"Failed to Relocate Original: {(long)dataPtr:X}");
            return dataPtr;
        }

        // Offset from start of struct.
        var offsetInGearData = dataPtr - (byte*)&playerPtr->LevelOneStats;
        var data = _owner.GetDataForIndex(playerIndex);
        var result =  (void*)(data + (int)offsetInGearData);

        Log.WriteLine($"Original: {(long)dataPtr:X}, New: {(long)result:X}, Offset: {offsetInGearData:X}");
        return result;
    }

    private string[] AssembleAsmPatch(CodePatch patch, IReloadedHooks hooks, uint fpuBackupAllocation)
    {
        var utilities = hooks.Utilities;
        patch.SplitMnemonic(out string destination, out string source, out string destinationRegister);
        
        // Add brackets to source if necessary.
        bool loadValue = source.EndsWith(']');
        if (!loadValue)
            source = $"[{source}]";

        // Prefix can be "byte", "word", etc.
        string sourcePrefix = "";
        if (loadValue)
        {
            if (source.IndexOf('[') > 0) // Has Prefix
                sourcePrefix = source.Substring(0, source.IndexOf('[') - 1);
        }

        // TODO: Don't back up modified registers.
        string[] code = new string[]
        {
            "use32",
            $"{utilities.PushCdeclCallerSavedRegistersExcept(destinationRegister)}",
            $"fxsave [{fpuBackupAllocation}]",  // Store FPU State
            $"pushfd",                          // Push Flags/Control Register
            $"lea eax, {source}",               // Push Address
            $"push eax",
            $"{utilities.AssembleAbsoluteCall(typeof(CustomGearExtendedLevelsPatches), nameof(GetNewAddress), false)}",
            $"popfd",                           // Restore Flags/Control Register
            $"fxrstor [{fpuBackupAllocation}]", // Restore FPU State
            $"{destination}, " + (loadValue ? $"{sourcePrefix} [eax]" : "eax"),

            $"{utilities.PopCdeclCallerSavedRegistersExcept(destinationRegister)}",
            (patch.ExtraOriginalCode != null ? patch.ExtraOriginalCode : "")
        };

        return code;
    }



    #region Code Patches
    private readonly CodePatch[] Patches = new CodePatch[]
    {
        new CodePatch(0x4CFBBC, "movss xmm0, dword [eax+esi+0A18h]"),
        new CodePatch(0x4B8E1D, "movss xmm0, dword [eax+esi+0A18h]"),
        new CodePatch(0x4E7925, "movss xmm0, dword [ecx+edi+0A18h]"),
        new CodePatch(0x4E6F86, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4BD95F, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x40F0A0, "movss xmm2, dword [eax+ecx+0A18h]"),
        new CodePatch(0x44BBD5, "movss xmm0, dword [ecx+edi+0A18h]"),
        new CodePatch(0x44DF19, "movss xmm0, dword [edx+ebp+0A18h]"),

        new CodePatch(0x486091, "mov ecx, [eax+ebx+0A18h]"),
        new CodePatch(0x48612A, "movss xmm0, dword [eax+ebx+0A18h]"),
        new CodePatch(0x4898B9, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x4911E8, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4A62C0, "movss xmm0, dword [edx+eax+0A18h]"),
        new CodePatch(0x4A646A, "movss xmm0, dword [edx+edi+0A18h]"),
        new CodePatch(0x4AD242, "movss xmm0, dword [eax+esi+0A18h]"),
        new CodePatch(0x4AF6FA, "movss xmm1, dword [eax+ebx+0A18h]"),
        new CodePatch(0x4B5F5C, "movss xmm0, dword [esi+eax+0A18h]"),
        new CodePatch(0x4BF09A, "movss xmm1, dword [ecx+eax+0A18h]"),
        new CodePatch(0x4C1FCC, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4CE361, "movss xmm0, dword [eax+esi+0A18h]"),
        new CodePatch(0x4CEA94, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4E2142, "movss xmm0, dword [eax+esi+0A18h]"),
        new CodePatch(0x4E2296, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x4E2644, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x4E27E9, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4E2880, "movss xmm0, dword [eax+ebx+0A18h]"),
        new CodePatch(0x4E2D2E, "movss xmm0, dword [edx+ebx+0A18h]"),
        new CodePatch(0x4E2E67, "movss xmm0, dword [edx+ebx+0A18h]"),

        new CodePatch(0x4E4112, "comiss xmm0, dword [edx+eax+0A18h]"),
        new CodePatch(0x4E411A, "lea edx, [edx+eax+0A18h]"),
        new CodePatch(0x4E615C, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x4E6662, "movss xmm0, dword [edx+esi+0A18h]"),
        new CodePatch(0x4EC5E4, "movss xmm0, dword [ecx+ebx+0A18h]"),
        new CodePatch(0x4ECAC5, "movss xmm1, dword [eax+ebx+0A18h]"),
        new CodePatch(0x501257, "movss xmm0, dword [ecx+edx+0A18h]"),
        new CodePatch(0x501C67, "movss xmm0, dword [ecx+esi+0A18h]"),
        new CodePatch(0x501D3F, "movss xmm0, dword [ecx+eax+0A18h]"),
        new CodePatch(0x501A92, "lea eax, [esi+0A10h]"),
        new CodePatch(0x501CD6, "lea eax, [ecx+0A10h]"),
        new CodePatch(0x502C5F, "lea eax, [esi+0A10h]"),
        new CodePatch(0x44D7AD, "mov eax, [edx+esi+0A1Ch]"),
        new CodePatch(0x44D7C6, "mov edx, [ecx+esi+0A1Ch]"),
        new CodePatch(0x4C1A13, "movss xmm0, dword [ecx+esi+0A28h]"),
        new CodePatch(0x4C81AC, "movss xmm0, dword [eax+ebx+0A28h]"),
        new CodePatch(0x4C85E4, "movss xmm0, dword [ecx+ebx+0A28h]"),
        new CodePatch(0x4C8ADC, "movss xmm0, dword [edx+ebx+0A28h]"),
        new CodePatch(0x4CAF45, "comiss xmm0, dword [eax+esi+0A28h]"),
        new CodePatch(0x42D73B, "lea edx, [ecx+eax+0A2Ch]"),
        new CodePatch(0x42DC24, "mov ecx, [eax+edi+0A2Ch]"),
        new CodePatch(0x44A0B5, "cvtsi2ss xmm1, dword [ecx+eax+0A2Ch]"),

        new CodePatch(0x47C4B7, "mov ecx, [ecx+edi+0A2Ch]"),
        new CodePatch(0x4A1894, "mov esi, [ecx+edi+0A2Ch]"),

        new CodePatch(0x4B52FB, "cvtsi2ss xmm1, dword [eax+ebx+0A2Ch]"),
        new CodePatch(0x4B5694, "cvtsi2ss xmm1, dword [ecx+ebx+0A2Ch]"),
        new CodePatch(0x4B5954, "cvtsi2ss xmm1, dword [eax+esi+0A2Ch]"),
        new CodePatch(0x4B6335, "cmp eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B6896, "cmp eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B6BB8, "cmp eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B6FF7, "cmp eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B73BE, "cmp eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B787B, "mov edx, [ecx+esi+0A2Ch]"),
        new CodePatch(0x4B78A1, "imul ecx, [eax+esi+0A2Ch]"),
        new CodePatch(0x4C6922, "mov eax, [edx+ebx+0A2Ch]"),
        new CodePatch(0x4C7248, "mov eax, [eax+ebx+0A2Ch]"),
        new CodePatch(0x4C72A8, "mov eax, [eax+ebx+0A2Ch]"),
        new CodePatch(0x4C72D0, "mov ecx, [eax+ebx+0A2Ch]"),
        new CodePatch(0x4C74C3, "mov ecx, [edx+ebx+0A2Ch]"),
        new CodePatch(0x4C750F, "mov eax, [edx+ebx+0A2Ch]"),
        new CodePatch(0x4C7C7A, "mov ecx, [ecx+ebx+0A2Ch]"),
        new CodePatch(0x4C8919, "mov ecx, [edx+ebx+0A2Ch]"),
        new CodePatch(0x4CF37B, "mov ecx, [edx+edi+0A2Ch]"),
        new CodePatch(0x4CF391, "mov ecx, [ecx+edi+0A2Ch]"),
        new CodePatch(0x4E185B, "mov ecx, [eax+esi+0A2Ch]"),
        new CodePatch(0x4E188F, "mov ecx, [edx+esi+0A2Ch]"),
        new CodePatch(0x4E18BC, "cvtsi2ss xmm0, dword [ecx+esi+0A2Ch]"),
        new CodePatch(0x4E9623, "mov edi, [ecx+esi+0A2Ch]"),
        new CodePatch(0x5012A0, "mov ecx, [eax+edx+0A2Ch]"),
        new CodePatch(0x5013E3, "mov ecx, [eax+esi+0A2Ch]"),
        new CodePatch(0x501533, "mov eax, [edx+esi+0A2Ch]"),
        new CodePatch(0x5016E4, "mov edx, [ecx+esi+0A2Ch]"),
        new CodePatch(0x501703, "mov ecx, [eax+edi+0A2Ch]"),
        new CodePatch(0x506E74, "mov eax, [edx+ebx+0A2Ch]"),
        new CodePatch(0x4E99DF, "mov ecx, [ecx+esi+0A30h]"),
        new CodePatch(0x500E9B, "lea ecx, [edx+0A30h]"),
        new CodePatch(0x429402, "cmp ecx, [edx+eax+0A38h]"),
        new CodePatch(0x430AD8, "cmp ecx, [edx+eax+0A38h]"),
        new CodePatch(0x4AE0BF, "mov ebx, [edx+esi+0A38h]"),
        new CodePatch(0x4AE3C2, "cmp eax, [ecx+esi+0A38h]"),
        new CodePatch(0x4AE75A, "mov ebx, [ecx+esi+0A38h]"),
        new CodePatch(0x4AF9C1, "cmp eax, [ecx+ebx+0A38h]"),
        new CodePatch(0x4B3B13, "cmp edx, [ecx+eax+0A38h]"),
        new CodePatch(0x4BD1F2, "mov edx, [esi+ebp+0A38h]"),
        new CodePatch(0x4BD1FB, "lea ecx, [esi+ebp+0A38h]"),
        new CodePatch(0x4CB19C, "mov edx, [ecx+esi+0A38h]"),
        new CodePatch(0x4CB1F4, "mov edi, [eax+esi+0A38h]"),
        new CodePatch(0x4B385F, "mov ebp, [edx+eax+0A3Ch]"),
        new CodePatch(0x4CB09B, "mov edi, [ecx+esi+0A3Ch]"),
        new CodePatch(0x4E5028, "movss xmm0, dword [edx+ebx+0A40h]"),
        new CodePatch(0x4CB106, "mov ecx, [eax+esi+0A44h]"),
        new CodePatch(0x4CB1FB, "mov ecx, [eax+esi+0A44h]"),
        new CodePatch(0x4CF9E9, "mov edx, [ecx+esi+0A44h]"),
        new CodePatch(0x4CFC94, "movss xmm0, dword [eax+esi+0A44h]"),
        new CodePatch(0x4CFD5D, "mov edx, [ecx+eax+0A44h]"),
        
        new CodePatch(0x4B2556, "mov eax, [esi+0A2Ch]"),
        new CodePatch(0x4B25E7, "mov ecx, [eax+esi+0A2Ch]"),
        new CodePatch(0x4B261B, "mov ecx, [edx+esi+0A2Ch]"),
        new CodePatch(0x4B20DD, "lea eax, [esi+0A44h]"),

        // New
        new CodePatch(0x4CAC37, "add ebx, esi", "cmp ebp, edi\ncvtsi2ss xmm0, eax"),
        new CodePatch(0x42E837, "mov ecx, [ebx]", "mov eax, 0EF9DB22Dh"),
        
        new CodePatch(0x430C4D, "mov edx, [eax+esi]", "mov [esp+ecx*4+100], edx"),
        new CodePatch(0x4BD1C0, "mov ecx, [edi+1Ch]", "xor edx, edx"),
        new CodePatch(0x4BD2FB, "mov ecx, [edi+1Ch]", "xor edx, edx"),
        new CodePatch(0x4BD19D, "mov ebx, [edi+24h]", "imul ebx, edx"),
        
        new CodePatch(0x4BD196, "cvtsi2ss xmm1, [edi+20h]"),
        new CodePatch(0x4BD1F2, "mov edx, [esi+ebp+0A38h]"),
        new CodePatch(0x4BD1FB, "lea ecx, [esi+ebp+0A38h]"),
        
        new CodePatch(0x42E873, "movzx edx, byte [0x6A5D36]"),
        new CodePatch(0x430e8c, "cmp edi, [ecx+0x6A55B8]"),

        new CodePatch(0x430E8C, "cmp edi, [ecx+0x6A55B8]"),

        new CodePatch(0x42E622, "movzx ecx, byte [0x006A5D36]"),
        new CodePatch(0x42E632, "cmp edx, [ecx+0x6A55B8]"),

        //new CodePatch(0x430C40, "movzx eax, byte [esi+71Ah]"),
        //new CodePatch(0x430C54, "mov edx, [esi]", "mov eax, 10624DD3h"),
        new CodePatch(0x430C67, "mov edx, [esi+1Ch]", "add esi, 1200h"),
    };

    #endregion

    private struct CodePatch
    {
        /// <summary>
        /// The address at which the opcode is found.
        /// </summary>
        public int Address;

        /// <summary>
        /// The original instruction that moves or manipulates the value.
        /// e.g. movss xmm0, dword [eax+esi+0A18h];
        /// </summary>
        public string OriginalMnemonic;

        /// <summary>
        /// Extra code to append to the end of the generated asm.
        /// </summary>
        public string ExtraOriginalCode;

        public CodePatch(int address, string originalMnemonic)
        {
            Address = address;
            OriginalMnemonic = originalMnemonic;
            ExtraOriginalCode = default;
        }

        public CodePatch(int address, string originalMnemonic, string extraOriginalCode)
        {
            Address = address;
            OriginalMnemonic = originalMnemonic;
            ExtraOriginalCode = extraOriginalCode;
        }

        /// <summary>
        /// Splits the mnemonic into the destination and source.
        /// </summary>
        public void SplitMnemonic(out string destination, out string source, out string destinationRegister)
        {
            var arr = OriginalMnemonic.Split(',');
            destination = arr[0].Trim();
            source = arr[1].Trim();
            destinationRegister = destination.Substring(destination.IndexOf(' ') + 1);
        }
    }
}
