using System.Collections.Generic;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Controllers.MenuEditor.Struct;
using Sewer56.SonicRiders.Parser.Menu.Metadata;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Structs;

namespace Riders.Tweakbox.Controllers.MenuEditor;

public unsafe class MenuEditorController : IController
{
    private MenuLayoutFileTracker _fileTracker = new MenuLayoutFileTracker();
    private Dictionary<Tm2dMetadataFileItem, InMemoryMenuMetadata> _metadataFiles = new Dictionary<Tm2dMetadataFileItem, InMemoryMenuMetadata>();

    public MenuEditorController()
    {
        _fileTracker.OnAddItem += AddMetadataFile;
        _fileTracker.OnRemoveItem += RemoveMetadataFile;
    }

    private void RemoveMetadataFile(Tm2dMetadataFileItem obj) => _metadataFiles.Remove(obj);

    private void AddMetadataFile(Tm2dMetadataFileItem obj) => _metadataFiles[obj] = new InMemoryMenuMetadata((MetadataHeader*)(*obj.MetadataFilePtrPtr), true);

    /// <summary>
    /// Returns pointers to all valid currently loaded in menu layout files.
    /// </summary>
    public Dictionary<Tm2dMetadataFileItem, InMemoryMenuMetadata> GetAllItems()
    {
        return _metadataFiles;
    }
}