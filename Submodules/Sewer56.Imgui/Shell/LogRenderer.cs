using Sewer56.Imgui.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DearImguiSharp;
using Sewer56.Imgui.Shell.Structures;

namespace Sewer56.Imgui.Shell;

/// <summary>
/// Renders the log.
/// </summary>
public class LogRenderer
{
    /// <summary>
    /// The log window.
    /// </summary>
    private InformationWindow _logWindow = new InformationWindow("Sewer56.Imgui Log", Pivots.Pivot.BottomRight, Pivots.Pivot.BottomRight);

    /// <summary>
    /// The position where the log is rendered.
    /// </summary>
    public Pivots.Pivot LogPosition { get; set; } = Pivots.Pivot.BottomLeft;

    /// <summary>
    /// Keeps all log text available.
    /// </summary>
    private Queue<LogItem> _logs = new Queue<LogItem>();

    public LogRenderer(string logWindowName)
    {
        _logWindow = new InformationWindow(logWindowName, Pivots.Pivot.BottomRight, Pivots.Pivot.BottomRight);
    }

    /// <summary>
    /// Renders the log for a frame.
    /// </summary>
    public void Render()
    {
        lock (_logs)
        {
            var totalItems = _logs.Count;
            if (totalItems <= 0)
                return;

            _logWindow.SetPivot(LogPosition, LogPosition);
            _logWindow.Begin();
            int itemsRendered = 0;
            while (itemsRendered < totalItems && _logs.TryDequeue(out var logItem))
            {
                // Render Item
                ImGui.Text(logItem.Text);

                // Re-queue item if necessary.
                if (!logItem.HasExpired())
                    _logs.Enqueue(logItem);

                itemsRendered += 1;
            }

            _logWindow.End();
        }
    }

    /// <summary>
    /// Logs a given item to the renderer.
    /// </summary>
    /// <param name="item"></param>
    public void Log(in LogItem item)
    {
        lock (_logs)
            _logs.Enqueue(item);
    }
}
