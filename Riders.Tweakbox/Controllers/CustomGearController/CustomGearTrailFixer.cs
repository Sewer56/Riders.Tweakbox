﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Misc;

namespace Riders.Tweakbox.Controllers.CustomGearController;

/// <summary>
/// A workaround for exhaust trail issues related to accessing data at offset based on gear index.
/// </summary>
public class CustomGearTrailFixer
{
    private IAsmHook _hookA;
    private IAsmHook _hookB;
    private IAsmHook _hookC;

    public CustomGearTrailFixer()
    {
        var hooks = IoC.GetSingleton<IReloadedHooks>();
        _hookA = hooks.CreateAsmHook(GetAsm("esi"), 0x4134FC).Activate();
        _hookB = hooks.CreateAsmHook(GetAsm("eax"), 0x413E89).Activate();
        _hookC = hooks.CreateAsmHook(GetAsm("eax"), 0x413F2D).Activate();
    }

    private string[] GetAsm(string register)
    {
        return new string[]
        {
            "use32",
            $"cmp {register}, 0x28",
            "jle exit",
            $"mov {register}, 0",
            "exit:"
        };
    }
}
