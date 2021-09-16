using Riders.Tweakbox.Controllers.CustomCharacterController;
using Riders.Tweakbox.Interfaces;
using Riders.Tweakbox.Interfaces.Structs.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Riders.Tweakbox.Api.Children;

public class CustomCharacterApi : ICustomCharacterApi
{
    private CustomCharacterController _controller;

    public CustomCharacterApi(CustomCharacterController controller)
    {
        _controller = controller;
    }

    public ModifyCharacterRequest AddCharacterBehaviour(ModifyCharacterRequest request) => _controller.AddCharacterBehaviour(request);

    public bool RemoveCharacterBehaviour(string name, bool clearCharacter = true) => _controller.RemoveCharacterBehaviour(name, clearCharacter);

    public bool TryGetAllCharacterBehaviours(int index, out List<ModifyCharacterRequest> behaviours) => _controller.TryGetAllCharacterBehaviours(index, out behaviours);

    public bool TryGetCharacterBehaviours(int index, out List<ModifyCharacterRequest> behaviours) => _controller.TryGetCharacterBehaviours(index, out behaviours);
    public void Reset(bool clearCharacters = true) => _controller.Reset(clearCharacters);
    public void Reload(IEnumerable<string> names) => _controller.Reload(names);
    public void ReloadAll() => _controller.ReloadAll();
    public bool UnloadCharacter(string name) => _controller.UnloadCharacter(name);
}
