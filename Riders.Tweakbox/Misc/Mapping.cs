using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.Components.Netplay.Menus.Models;
using Riders.Tweakbox.Controllers.CustomGearController.Structs;
using Riders.Tweakbox.Controllers.CustomGearController.Structs.Internal;
using Sewer56.Imgui.Controls;
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
        typeAdapterConfig.NewConfig<AddGearRequest, CustomGearData>().AfterMapping((request, data) => data.GearData = request.GearData.Value);
        Mapper = new Mapper(typeAdapterConfig);
    }
}
