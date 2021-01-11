using System;
using System.Collections.Generic;
using System.Threading;
using DearImguiSharp;
using Sewer56.Imgui.Misc;
using Sewer56.Imgui.Shell.Interfaces;

namespace Riders.Tweakbox.Components.Misc
{
    public class MemoryDebugWindow : IComponent
    {
        /// <inheritdoc />
        public string Name { get; set; } = "Memory Debug";

        private int _objects    = 1000;
        private int _objectSize = 100;
        private int _timeBetweenAllocations = 100;

        private bool _isEnabled;
        private bool _simulatingLoad;
        private Thread _simulateLoadThread;

        public void StartThread(bool start)
        {
            if (!start) 
                return;

            if (_simulateLoadThread != null && _simulateLoadThread.IsAlive)
                _simulateLoadThread.Join();

            _simulateLoadThread = new Thread(SimulateMemoryLoad);
            _simulateLoadThread.Start();
        }

        private void SimulateMemoryLoad()
        {
            var data = new List<byte[]>();
            while (true)
            {
                if (!_simulatingLoad)
                    goto cleanup;

                data = new List<byte[]>(_objects);
                for (int x = 0; x < _objects; x++)
                    data.Add(new byte[_objectSize]);

                Thread.Sleep(_timeBetweenAllocations);
            }

            cleanup:
            data = null;
            GC.Collect();
            return;
        }

        /// <inheritdoc />
        public ref bool IsEnabled() => ref _isEnabled;

        /// <inheritdoc />
        public void Disable() { }

        /// <inheritdoc />
        public void Enable() { }

        /// <inheritdoc />
        public void Render() 
        {
            if (ImGui.Begin(Name, ref _isEnabled, 0))
            {
                if (ImGui.Button("Force Full Garbage Collection", Constants.DefaultVector2))
                    GC.Collect();

                if (ImGui.Checkbox("Simulate GC Load", ref _simulatingLoad))
                    StartThread(_simulatingLoad);

                if (_simulatingLoad)
                {
                    ImGui.DragInt("Number of Objects", ref _objects, 1.0f, 0, 99999, null);
                    ImGui.DragInt("Object Size", ref _objectSize, 1.0f, 0, 99999, null);
                    ImGui.DragInt("Time Between Allocations (ms)", ref _timeBetweenAllocations, 1.0f, 0, 99999, null);

                    var objectsPerSec = _objects * (1000.0f / _timeBetweenAllocations);
                    ImGui.Text($"Bytes/allocation: {(_objects * _objectSize) / 1000000.0f}MB");
                    ImGui.Text($"Objects/sec: {objectsPerSec}");
                    ImGui.Text($"Bytes/sec: {(objectsPerSec * _objectSize) / 1000000}MB");
                }
            }

            ImGui.End();
        }
    }
}
