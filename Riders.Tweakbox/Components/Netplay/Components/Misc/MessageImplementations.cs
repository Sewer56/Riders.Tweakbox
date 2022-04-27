using System.Linq;
using LiteNetLib;
using Riders.Netplay.Messages;
using Riders.Netplay.Messages.External;
using Riders.Tweakbox.Components.Netplay.Components.Server;
using Riders.Tweakbox.Components.Netplay.Sockets;
using Riders.Tweakbox.Controllers.CustomCharacterController;
using Riders.Tweakbox.Controllers.CustomGearController;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;

namespace Riders.Tweakbox.Components.Netplay.Components.Misc;

public unsafe class MessageImplementations : INetplayComponent
{
    public Socket Socket { get; set; }

    private CustomGearController _customGearController = IoC.GetSingleton<CustomGearController>();
    private CustomCharacterController _customCharacterController = IoC.GetSingleton<CustomCharacterController>();

    public MessageImplementations(Socket socket)
    {
        Socket = socket;
        CustomCharacterData.FromGame += CustomCharFromGame;
        CustomCharacterData.ToGame += CustomCharToGame;
        
        CustomGearData.FromGame += CustomGearFromGame;
        CustomGearData.ToGame += CustomGearToGame;
    }

    public void Dispose()
    {
        CustomCharacterData.FromGame -= CustomCharFromGame;
        CustomCharacterData.ToGame -= CustomCharToGame;

        CustomGearData.FromGame -= CustomGearFromGame;
        CustomGearData.ToGame -= CustomGearToGame;
    }

    private void CustomGearToGame(CustomGearData obj)
    {
        Socket.TryGetComponent(out ConnectionManager manager);
        if (_customGearController.HasAllGears(obj.CustomGears, out var missingGears))
            _customGearController.Reload(obj.CustomGears);
        else
            manager.ForceDisconnect($"Client is missing custom gears being used by the host.\nGear List:\n\n{string.Join("\n", missingGears)}");
    }

    private CustomGearData CustomGearFromGame()
    {
        _customGearController.GetCustomGearNames(out var loadedGears, out _);
        return new CustomGearData()
        {
            CustomGears = loadedGears,
            Gears = Player.Gears.ToArray()
        };
    }

    private void CustomCharToGame(CustomCharacterData obj)
    {
        Socket.TryGetComponent(out ConnectionManager manager);
        if (_customCharacterController.HasAllCharacters(obj.ModifiedCharacters, out var missingChars))
            _customCharacterController.Reload(obj.ModifiedCharacters);
        else
            manager.ForceDisconnect($"Client is missing modified characters being used by the host.\nCharacter List:\n\n{string.Join("\n", missingChars)}");
    }

    private CustomCharacterData CustomCharFromGame()
    {
        var behaviours = _customCharacterController.GetAllCharacterBehaviours_Internal();
        var names = behaviours.SelectMany(x => x.Select(y => y.CharacterName));

        return new CustomCharacterData()
        {
            ModifiedCharacters = names.ToArray()
        };
    }

    // Interface disposal.
    public void HandleReliablePacket(ref ReliablePacket packet, NetPeer source) { }

    public void HandleUnreliablePacket(ref UnreliablePacket packet, NetPeer source) { }
}