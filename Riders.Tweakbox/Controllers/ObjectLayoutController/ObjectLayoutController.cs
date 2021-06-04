using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Interop;
using Reloaded.Memory.Pointers;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Writers;
using Riders.Tweakbox.Controllers.ObjectLayoutController.Struct;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Functions;
using Sewer56.SonicRiders.Parser.Layout;
using Sewer56.SonicRiders.Parser.Layout.Enums;
using Sewer56.SonicRiders.Parser.Layout.Structs;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Structures.Tasks;
using Sewer56.SonicRiders.Structures.Tasks.Base;
using StructLinq;
using Functions = Sewer56.SonicRiders.Functions.Functions;
using Task = Sewer56.SonicRiders.Structures.Tasks.Base.Task;

namespace Riders.Tweakbox.Controllers.ObjectLayoutController
{
    public unsafe class ObjectLayoutController
    {
        /// <summary>
        /// Contains the current loaded layout files.
        /// </summary>
        public List<LoadedLayoutFile> LoadedLayouts = new List<LoadedLayoutFile>();

        private LoadedLayoutFile _currentLayoutFile;

        private IHook<Functions.InitializeObjectLayoutFn> _initialiseLayoutHook;
        private IHook<Functions.CheckResetTaskFnPtr> _checkResetTaskHook;

        private EventController _event;
        private Functions.InitializeObjectLayoutFn _initializeObjectLayoutOriginal;

        private Pinnable<int> _currentLoadingObjectIndex = new Pinnable<int>(0);
        private IAsmHook _setCurrentLoadingObjectIndex;
        private static ObjectLayoutController _this;

        /// <summary>
        /// Skips map portal initialisation.
        /// </summary>
        private Patch _skipPortalInit = new Patch(0x41bcb5, new byte[] { 0xE9, 0x81, 0x01, 0x00, 0x00 });

        public ObjectLayoutController(EventController @event, IReloadedHooks hooks)
        {
            _this = this;
            _event = @event;
            _initialiseLayoutHook = Functions.InitializeObjectLayout.Hook(InitializeLayoutImpl).Activate();
            _initializeObjectLayoutOriginal = _initialiseLayoutHook.OriginalFunction;
            Event.AfterKillAllTasks += AfterKillAllTasks;

            string[] setLoadingObjectAsm = new[]
            {
                "use32",
                $"mov dword [{(int)_currentLoadingObjectIndex.Pointer}], ebx"
            };

            _checkResetTaskHook = Functions.CheckResetTask.HookAs<Functions.CheckResetTaskFnPtr>(typeof(ObjectLayoutController), nameof(CheckResetTaskImplStatic)).Activate();
            _setCurrentLoadingObjectIndex = hooks.CreateAsmHook(setLoadingObjectAsm, 0x00419880, AsmHookBehaviour.ExecuteFirst).Activate();
        }

        /// <summary>
        /// Calculates the Total Number of objects for this controller.
        /// </summary>
        public int CountTotalObjects() => LoadedLayouts.ToStructEnumerable().Sum(x => x.LayoutFile.Header->ObjectCount, x => x);

        /// <summary>
        /// Calculates the Total Number of tasks for this controller.
        /// </summary>
        public int CountTotalTasks() => LoadedLayouts.ToStructEnumerable().Sum(x => x.CountTasks(), x => x);

        /// <summary>
        /// Finds the task for a specified object.
        /// </summary>
        public bool TryFindObjectTask(SetObject* theObject, out BlittablePointer<SetObjectTask<SetObjectTaskData>> task)
        {
            task = default;

            // The occurrence 
            foreach (var layout in LoadedLayouts)
            {
                if (layout.IsMyObject(theObject))
                {
                    var maybeTask = layout.GetObjectTask(theObject);
                    if (!maybeTask.HasValue)
                        return false;

                    task = maybeTask.Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves a given object.
        /// </summary>
        public void MoveObject(SetObject* theObject, Vector3 position)
        {
            if (!TryFindObjectTask(theObject, out var objectPtr))
                return;

            objectPtr.Pointer->TaskData->Position = position;
        }

        /// <summary>
        /// Rotates a given objects.
        /// </summary>
        public void RotateObject(SetObject* theObject, Vector3 rotationDegrees)
        {
            if (!TryFindObjectTask(theObject, out var objectPtr))
                return;

            objectPtr.Pointer->TaskData->RotationBams = rotationDegrees.DegreesToBamsInt();
        }

        /// <summary>
        /// Sets a certain attribute to an object.
        /// </summary>
        public void SetAttribute(SetObject* theObject, ushort attribute)
        {
            if (!TryFindObjectTask(theObject, out var objectPtr))
                return;

            objectPtr.Pointer->Attribute = attribute;
        }

        /// <summary>
        /// Sets a certain portal character to an object.
        /// </summary>
        public void SetPortalChar(SetObject* theObject, byte portalChar)
        {
            if (!TryFindObjectTask(theObject, out var objectPtr))
                return;

            objectPtr.Pointer->Portal = portalChar;
        }

        /// <summary>
        /// Ends the item's task so it is visually deleted and sets its id to invalid
        /// so it wouldn't spawn next time.
        /// </summary>
        public void InvalidateItem(SetObject* item)
        {
            item->Type = ObjectId.oInvalid;

            if (!TryFindObjectTask(item, out var objectPtr))
                return;

            objectPtr.Pointer->TaskStatus = 1;
        }

        /// <summary>
        /// Quickly restarts the current stage.
        /// </summary>
        public unsafe void FastRestart()
        {
            // Fast Restart
            *(int*) 0x00692BC0 = 0x004171B0;
            *(int*) 0x00692BC4 = 0;
            *(int*) 0x00692BC8 = 0;
            *State.EndOfGameMode = EndOfGameMode.Restart;
        }

        /// <summary>
        /// Quickly restarts the stage.
        /// </summary>
        /// <param name="setObject">The object.</param>
        /// <returns>Pointer to the newly created object.</returns>
        public unsafe SetObject* LoadNewObject(SetObject* setObject)
        {
            _currentLayoutFile = LoadedLayoutFile.Create(1);
            _currentLayoutFile.LayoutFile.Objects[0] = *setObject;
            LoadedLayouts.Add(_currentLayoutFile);
            LoadExtraLayoutFile(_currentLayoutFile);
            return _currentLayoutFile.LayoutFile.Objects.Pointer;
        }

        /// <summary>
        /// Finds the item nearest to a given position.
        /// </summary>
        /// <param name="ignore">Ignore this item.</param>
        /// <param name="position">The position to search nearest to.</param>
        /// <param name="withTaskOnly">Only include items with a task; i.e. those that are active/rendered.</param>
        /// <param name="distance">Distance between the item and the returned item.</param>
        /// <param name="index">Index of the nearest item over all collections.</param>
        public SetObject* FindNearestItem(SetObject* ignore, Vector3 position, bool withTaskOnly, out float distance, out int index)
        {
            SetObject* result = null;
            distance = float.MaxValue;
            index = default;

            float nearestDistanceSquared = float.MaxValue;
            int totalIndex = 0;

            foreach (var layout in LoadedLayouts)
            {
                var objects = layout.LayoutFile.Objects;
                for (int x = 0; x < objects.Count; x++)
                {
                    ref var currentItem = ref objects[x];
                    var distSquared = Vector3.DistanceSquared(position, currentItem.Position);

                    // Check if visible only.
                    if (withTaskOnly && !layout.ObjectTasks[x].HasValue)
                    {
                        totalIndex++;
                        continue;
                    }

                    // Check distance.
                    if (distSquared < nearestDistanceSquared)
                    {
                        result = &objects.Pointer[x];
                        nearestDistanceSquared = distSquared;
                        index = totalIndex;
                    }

                    totalIndex++;
                }
            }

            if (result != (void*) 0)
                distance = MathF.Sqrt(nearestDistanceSquared);

            return result;
        }

        /// <summary>
        /// Exports the current layout data to bytes.
        /// </summary>
        /// <param name="bigEndian">The endian to export in.</param>
        public byte[] Export(bool bigEndian = false)
        {
            using var stream = new ExtendedMemoryStream();

            var objects = new List<SetObject>();
            var objectUnknown = new List<ushort>();
            
            // Collect Data
            foreach (var layout in LoadedLayouts)
            {
                var layoutObjects = layout.LayoutFile.Objects;
                var unknownArray = layout.LayoutFile.UnknownArray;
                for (int x = 0; x < layoutObjects.Count; x++)
                {
                    ref var layoutObject = ref layoutObjects[x];
                    if (layoutObject.Type == ObjectId.oInvalid) 
                        continue;

                    objects.Add(layoutObject);
                    objectUnknown.Add(unknownArray[x]);
                }
            }

            // Write Data
            var header = new LayoutHeader(objects.Count, true);
            stream.Write(bigEndian ? Reflection.SwapStructEndianness(header) : header);

            foreach (var obj in objects)
                stream.Write(bigEndian ? Reflection.SwapStructEndianness(obj) : obj);

            foreach (var unk in objectUnknown)
                stream.Write(bigEndian ? Reflection.SwapStructEndianness(unk) : unk);

            return stream.ToArray();
        }

        private int CheckResetTaskImpl(int a1, int a2)
        {
            if (*State.ResetTask != (void*) 0 && *State.EndOfGameMode != EndOfGameMode.Restart)
            {
                foreach (var layout in LoadedLayouts)
                    layout.Dispose();

                LoadedLayouts.Clear();
            }

            return _checkResetTaskHook.OriginalFunction.Value.Invoke(a1, a2);
        }

        private void AfterKillAllTasks()
        {
            // Dispose all of our own layout files.
            for (int x = 0; x < LoadedLayouts.Count; x++)
                Array.Fill(LoadedLayouts[x].ObjectTasks, default);
        }

        private unsafe int InitializeLayoutImpl()
        {
            // Add initial Layout File
            _currentLayoutFile = new LoadedLayoutFile(InMemoryLayoutFile.Current);
            if (LoadedLayouts.Count < 1)
                LoadedLayouts.Add(_currentLayoutFile);
            else
                LoadedLayouts[0] = _currentLayoutFile;

            // Initialise original.
            Event.AfterSetTask += CollectObjectTask;
            var result = _initializeObjectLayoutOriginal();
            Event.AfterSetTask -= CollectObjectTask;

            // Initialise all added layouts.
            // And thus collect all tasks.
            for (var x = 1; x < LoadedLayouts.Count; x++)
                LoadExtraLayoutFile(LoadedLayouts[x]);

            return result;
        }

        private void LoadExtraLayoutFile(LoadedLayoutFile targetFile)
        {
            // Backup Layout Ptr
            var originalPtr = *State.CurrentStageObjectLayout;

            Event.AfterSetTask += CollectObjectTask;
            _skipPortalInit.Enable();

            _currentLayoutFile = targetFile;
            *State.CurrentStageObjectLayout = targetFile.LayoutFile.Header;
            _initializeObjectLayoutOriginal();

            _skipPortalInit.Disable();
            Event.AfterSetTask -= CollectObjectTask;

            // Restore Layout Ptr
            *State.CurrentStageObjectLayout = originalPtr;
        }

        private unsafe void CollectObjectTask(void* methodptr, uint maybemaxtaskheapsize, int taskdatasize, Task* result)
        {
            _currentLayoutFile.ObjectTasks[_currentLoadingObjectIndex.Value] = new BlittablePointer<SetObjectTask<SetObjectTaskData>>((SetObjectTask<SetObjectTaskData>*) result);
        }

        [UnmanagedCallersOnly]
        private static int CheckResetTaskImplStatic(int a1, int a2) => _this.CheckResetTaskImpl(a1, a2);
    }
}
