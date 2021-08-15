using Riders.Tweakbox.Misc.Log;
using System;
using System.Diagnostics;
namespace Riders.Tweakbox.Misc;

public static class BenchmarkUtilities
{
    public const string BenchmarkStringFormat = "[Benchmark] {0}: {1}ms";
    private static Logger _log = new Logger(LogCategory.Benchmark);

    public static void Benchmark(Action action, string name)
    {
        var watch = Stopwatch.StartNew();
        action();
        _log.WriteLine(string.Format(BenchmarkStringFormat, name, watch.ElapsedMilliseconds));
    }

    public static T Benchmark<T>(Func<T> action, string name)
    {
        var watch = Stopwatch.StartNew();
        var result = action();
        _log.WriteLine(string.Format(BenchmarkStringFormat, name, watch.ElapsedMilliseconds));
        return result;
    }
}
