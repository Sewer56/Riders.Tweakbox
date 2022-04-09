using System;
using System.Runtime.InteropServices;
using Reloaded.Memory.Pointers;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Parser.Layout.Structs;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
namespace Riders.Tweakbox.Controllers.ObjectLayoutController.Struct;

public unsafe class LoadedLayoutFile : IDisposable
{
    /// <summary>
    /// Contains a list of all pointers leading to individual object tasks.
    /// </summary>
    public BlittablePointer<SetObjectTask<SetObjectTaskData>>?[] ObjectTasks;

    /// <summary>
    /// Pointer to the actual file itself.
    /// </summary>
    public InMemoryLayoutFile LayoutFile;
    private bool _ownsMemory;

    public LoadedLayoutFile(InMemoryLayoutFile layoutFile, bool ownsMemory = false)
    {
        LayoutFile = layoutFile;
        ObjectTasks = new BlittablePointer<SetObjectTask<SetObjectTaskData>>?[LayoutFile.Header->ObjectCount];
        _ownsMemory = ownsMemory;
    }

    public LoadedLayoutFile() { }

    ~LoadedLayoutFile() => Dispose();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsMemory)
            Marshal.FreeHGlobal((IntPtr)LayoutFile.Header);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finds the task for a specified object.
    /// </summary>
    public bool IsMyObject(SetObject* theObject) => LayoutFile.IsMyObject(theObject);

    /// <summary>
    /// Gets the number of alive tasks for this layout file.
    /// </summary>
    public int CountTasks()
    {
        int number = 0;
        for (int x = 0; x < ObjectTasks.Length; x++)
        {
            if (ObjectTasks[x].HasValue)
                number++;
        }

        return number;
    }

    /// <summary>
    /// Finds the task for a specified object.
    /// </summary>
    public int GetObjectIndex(SetObject* theObject)
    {
        for (int x = 0; x < LayoutFile.Objects.Count; x++)
        {
            var obj = &LayoutFile.Objects.Pointer[x];
            if (theObject == obj)
                return x;
        }

        return -1;
    }

    /// <summary>
    /// Finds the task for a specified object.
    /// </summary>
    public BlittablePointer<SetObjectTask<SetObjectTaskData>>? GetObjectTask(SetObject* theObject)
    {
        var index = GetObjectIndex(theObject);
        return ObjectTasks[index];
    }

    /// <summary>
    /// Kills all tasks related to this layout file.
    /// </summary>
    public void KillAllTasks()
    {
        foreach (var task in ObjectTasks)
        {
            if (task.HasValue)
                task.Value.Pointer->TaskStatus = 1;
        }
    }

    /// <summary>
    /// Allocates an individual file in the game's native heap and returns a struct wrapping it.
    /// </summary>
    /// <param name="numItems">The number of items.</param>
    public static LoadedLayoutFile Create(int numItems)
    {
        // Allocate and Initialise Header
        var layoutPtr = (void*)Marshal.AllocHGlobal(InMemoryLayoutFile.CalcFileSize(numItems));
        var header = (LayoutHeader*)layoutPtr;
        header->Initialise(numItems);

        // Make file and pointers
        var file = new LoadedLayoutFile(new InMemoryLayoutFile(layoutPtr), true);
        file.ObjectTasks = new BlittablePointer<SetObjectTask<SetObjectTaskData>>?[numItems];

        // Setup remaining data.
        for (int x = 0; x < numItems; x++)
            file.LayoutFile.UnknownArray[x] = 4;

        return file;
    }
}
