using System;
using System.Diagnostics;
namespace Riders.Tweakbox.Misc;

public static class BenchmarkUtilities
{
    public const string BenchmarkStringFormat = "[Benchmark] {0}: {1}ms";

    public static void Benchmark(Action action, string name)
    {
        var watch = Stopwatch.StartNew();
        action();
        Log.WriteLine(string.Format(BenchmarkStringFormat, name, watch.ElapsedMilliseconds), LogCategory.Benchmark);
    }

    public static T Benchmark<T>(Func<T> action, string name)
    {
        var watch = Stopwatch.StartNew();
        var result = action();
        Log.WriteLine(string.Format(BenchmarkStringFormat, name, watch.ElapsedMilliseconds), LogCategory.Benchmark);
        return result;
    }
}
