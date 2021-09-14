using System;
using System.Collections.Generic;
using MapsterMapper;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Gears;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Services;

namespace Riders.Tweakbox.Api.Children;

public class CustomGearApi : ICustomGearApi
{
    private CustomGearController _controller;
    private CustomGearService _service;

    public CustomGearApi(CustomGearController controller, CustomGearService service)
    {
        _controller = controller;
        _service = service;
    }

    public AddGearRequest ImportFromFolder(string folderPath) => _service.ImportFromFolder(folderPath);

    CustomGearData ICustomGearApi.AddGear(AddGearRequest request) => AddGear(request);

    public CustomGearData AddGear(AddGearRequest request)
    {
        var result = _controller.AddGear(request);
        return Mapping.Mapper.Map<CustomGearData>(result);
    }

    public void GetCustomGearNames(Span<string> loadedNames, Span<string> unloadedNames) => _controller.GetCustomGearNames(loadedNames, unloadedNames);

    public void GetCustomGearNames(out string[] loadedGears, out string[] unloadedGears) => _controller.GetCustomGearNames(out loadedGears, out unloadedGears);

    public void GetCustomGearCount(out int loadedGears, out int unloadedGears) => _controller.GetCustomGearCount(out loadedGears, out unloadedGears);

    public string GetGearName(int index, out bool isCustomGear) => _controller.GetGearName(index, out isCustomGear);

    public bool UnloadGear(string name) => _controller.UnloadGear(name);

    public bool RemoveGear(string name, bool clearGear = true) => _controller.RemoveGear(name, clearGear);

    public bool IsGearLoaded(string name) => _controller.IsGearLoaded(name);

    public bool HasAllGears(IEnumerable<string> gearNames, out List<string> missingGears) => _controller.HasAllGears(gearNames, out missingGears);

    public void Reload(IEnumerable<string> gearNames) => _controller.Reload(gearNames);

    public void ReloadAll() => _controller.ReloadAll();

    public void Reset(bool clearGears = true) => _controller.Reset(clearGears);
    public void RemoveVanillaGears() => _controller.Reset(false, true);
    public bool TryGetGearData(string name, out CustomGearData data)
    {
        if (_controller.TryGetGearData(name, out var internalData))
        {
            data = Mapping.Mapper.Map<CustomGearData>(internalData);
            return true;
        }

        data = default;
        return false;
    }

    public bool TryGetGearData(int index, out CustomGearData data)
    {
        if (_controller.TryGetGearData(index, out var internalData))
        {
            data = Mapping.Mapper.Map<CustomGearData>(internalData);
            return true;
        }

        data = default;
        return false;
    }
}
