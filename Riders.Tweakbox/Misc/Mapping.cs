using System;
using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Reloaded.Memory;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Riders.Tweakbox.Interfaces.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using Sewer56.Imgui.Controls;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Tweakbox.Misc;

/// <summary>
/// Provides mapping support using Mapster.
/// </summary>
public static class Mapping
{
    public static readonly Mapper Mapper;

    static Mapping()
    {
        TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
        var typeAdapterConfig = new TypeAdapterConfig();
        typeAdapterConfig.Compiler = exp => exp.CompileFast();
        typeAdapterConfig.NewConfig<GetServerResult, GetServerResultEx>().AfterMapping(result => result.Extend());
        typeAdapterConfig.NewConfig<TextInputData, TextInputData>().MapWith(result => new TextInputData(result.Text, (int)result.SizeOfData, 1));

        // Preserve reference for self copy
        typeAdapterConfig.NewConfig<CustomGearDataInternal, CustomGearDataInternal>();

        // Internal Copying
        typeAdapterConfig.NewConfig<AddGearRequest, CustomGearDataInternal>()
            .Ignore(item => item.GearData)
            .AfterMapping((request, data) => Struct.FromArray(request.GearData.AsSpan(), out data.GearData));

        typeAdapterConfig.NewConfig<CustomGearDataInternal, AddGearRequest>()
            .Ignore(item => item.GearData)
            .AfterMapping((data, request) => request.GearData = Struct.GetBytes(data.GearData));
        
        // Convert To/From API
        typeAdapterConfig.NewConfig<CustomGearDataInternal, CustomGearData>()
            .Ignore(item => item.GearData)
            .AfterMapping((data, gearData) => gearData.GearData = Struct.GetBytes(data.GearData));

        typeAdapterConfig.NewConfig<IExtremeGear, IExtremeGear>().MapWith(data => data).PreserveReference(true);
        typeAdapterConfig.NewConfig<ICustomStats, ICustomStats>().MapWith(data => data).PreserveReference(true);

        Mapper = new Mapper(typeAdapterConfig);
    }
}
