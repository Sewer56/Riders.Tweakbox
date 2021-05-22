using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Riders.Tweakbox.API.Application.Commands.v1.Browser.Result;
using Riders.Tweakbox.Components.Netplay.Menus.Models;

namespace Riders.Tweakbox.Misc
{
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
            Mapper = new Mapper(typeAdapterConfig);
        }
    }
}
