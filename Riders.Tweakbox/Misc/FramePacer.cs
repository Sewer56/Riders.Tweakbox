using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Reloaded.WPF.Animations.FrameLimiter;
namespace Riders.Tweakbox.Misc;

/// <summary>
/// The <see cref="FramePacer"/> class is a simple class that allows for control of frame pacing
/// of an arbitrary function loop, such as a graphics or game logic loop.
/// 
/// Intended for graphics, it allows for function loops to run at regular time intervals by 
/// allowing you to specify a specific frequency to run the loops at in terms of frames per second
/// (hertz/how many times to execute a second).
///
/// To use this method, simply place <see cref="EndFrame"/> at the end/exit points of a recurring logic
/// loop. 
/// </summary>
public class FramePacer
{
    /*
        Note: This is a customized version of the frame pacer from Reloaded.WPF.Animations
              It's been optimized for CPU usage.
    */

    private const double MillisecondsInSecond = 1000.0D;
    private const int StopwatchSamples = 100;

    /// <summary>
    /// Contains the stopwatch used for timing the frame time.
    /// </summary>
    private Stopwatch _frameTimeWatch;

    /// <summary>
    /// Contains the stopwatch used for timing sleep periods.
    /// </summary>
    private Stopwatch _sleepWatch;

    /// <summary>
    /// Contains the stopwatch used for checking how long thread yields while spinning.
    /// </summary>
    private Stopwatch _threadYieldWatch;

    /// <summary>
    /// Contains a history of frame times of the recent <see cref="FPSLimit"/> frames.
    /// </summary>
    private CircularBuffer<double> _frameTimeBuffer;

    // ----------------------------------------------------
    // User configurable

    /// <summary>
    /// Sets or gets the current framerate cap.
    /// </summary>
    public float FPSLimit
    {
        get { return _FPSLimit; }
        set
        {
            FrameTimeTarget = MillisecondsInSecond / value;
            _FPSLimit = value;
        }
    }

    private float _FPSLimit;

    /// <summary>
    /// [Milliseconds] Contains the current set maximum allowed time that a frame should be rendered in.
    /// This value is automatically generated when you set the <see cref="FPSLimit"/>.
    /// </summary>
    public double FrameTimeTarget { get; private set; }

    // ----------------------------------------------------

    /// <summary>
    /// Contains the current amount of frames per second.
    /// </summary>
    public double StatFPS => (MillisecondsInSecond / _frameTimeBuffer.Average());

    /// <summary>
    /// Contains the number of frames per second that would be rendered if all of the
    /// remaining frames were to take as long as the last.
    /// </summary>
    public double StatFrameFPS { get; private set; }

    /// <summary>
    /// Contains the current amount of frames per second in the case the FPS limit were to be removed
    /// according to the time spent rendering the previous frame.
    /// </summary>
    public double StatPotentialFPS { get; private set; }

    /// <summary>
    /// [Milliseconds]
    /// Stores how much more time the CPU has spent sleeping than requested (by <see cref="StatSleepTime"/>) on the last frame.
    /// </summary>
    public double StatOverslept { get; private set; }

    /// <summary>
    /// [Milliseconds] The amount of time spent between the start of the last and the next frame.
    /// </summary>
    public double StatFrameTime { get; private set; }

    /// <summary>
    /// [Milliseconds] The amount spent rendering the last frame.
    /// </summary>
    public double StatRenderTime { get; private set; }

    /// <summary>
    /// [Milliseconds] The time that will be spent sleeping during the next frame.
    /// Note: Actual time slept is <see cref="StatSleepTime"/> + <see cref="StatOverslept"/>.
    /// </summary>
    public double StatSleepTime { get; private set; }

    /// <summary>
    /// Granularity of the Windows timer in milliseconds.
    /// </summary>
    public float TimerGranularity { get; set; }

    /// <summary>
    /// See summary of <see cref="FramePacer"/>.
    /// </summary>
    public FramePacer()
    {
        _frameTimeWatch = new Stopwatch();
        _sleepWatch = new Stopwatch();
        _frameTimeBuffer = new CircularBuffer<double>(StopwatchSamples);
        _threadYieldWatch = new Stopwatch();
        FPSLimit = 144;

        Native.NtQueryTimerResolution(out int maximumResolution, out int _, out int currentResolution);
        TimerGranularity = currentResolution / 10000f;
    }

    /// <summary>
    /// Marks the end of an individual frame/recurring piece of logic to be performed/executed.
    /// You should put this at the end of a reoccuring loop.
    /// </summary>
    /// <param name="spin">
    ///     If true, uses an alternative timing method where CPU briefly spins (performs junk calculations) after sleeping slightly less time until it is precisely the time to start the next frame.
    ///     Increases accuracy at the expense of CPU load.
    /// 
    ///     See: <see cref="SpinTimeRemaining"/> to control the time in milliseconds left to sleep at which to start spinning at.
    /// </param>
    /// <param name="maxSpeedupTimeMillis">The maximum duration in milliseconds of how long speedup can occur.</param>
    /// <param name="allowSpeedup">
    ///     Allows for the speeding up of the frame counter to maintain target FPS by sleeping less on the next frame.
    ///     If set to false, frame pacer will not try to catch up on next sleep in cases of lost frames.
    /// </param>
    /// <param name="spinAllowThreadYield">
    ///     If true allows the thread to yield when spinning.
    ///     Only set this to false if the CPU is maxed out (>95% usage).
    /// </param>
    /// <param name="dontSleep">Does not sleep if true, only collects framerate information.</param>
    public void EndFrame(float maxSpeedupTimeMillis = 2000, bool spin = false, bool allowSpeedup = true, bool spinAllowThreadYield = true, bool dontSleep = false)
    {
        // Summarize stats for the current frame.
        StatRenderTime = _frameTimeWatch.Elapsed.TotalMilliseconds;
        StatPotentialFPS = MillisecondsInSecond / StatRenderTime;
        StatSleepTime = FrameTimeTarget - StatOverslept - StatRenderTime;

        if (!allowSpeedup && StatSleepTime < 0)
            StatSleepTime = 0;

        if (allowSpeedup && StatSleepTime < -maxSpeedupTimeMillis)
            StatSleepTime = -maxSpeedupTimeMillis;

        // Sleep
        if (!dontSleep)
            Sleep(spin, spinAllowThreadYield);

        // Restart calculation for new frame.
        StartFrame();
    }

    /// <summary>
    /// Calculates statistics for the previous frame and resets the timers to begin a new frame.
    /// </summary>
    private void StartFrame()
    {
        // Calculate FPS at start of frame.
        StatFrameTime = _frameTimeWatch.Elapsed.TotalMilliseconds;
        StatFrameFPS = MillisecondsInSecond / StatFrameTime;
        _frameTimeBuffer.PushBack(StatFrameTime);

        // Restart the stopwatch.
        _frameTimeWatch.Restart();
    }


    /// <summary>
    /// Pauses execution for the remaining of the time until the next frame begins.
    /// </summary>
    /// <param name="spin">
    ///     If true, uses an alternative timing method where CPU briefly spins (performs junk calculations) after sleeping slightly less time until it is precisely the time to start the next frame.
    ///     Increases accuracy at the expense of CPU load.
    /// </param>
    /// <param name="allowThreadYield">
    ///    If true allows the thread to yield when spinning.
    ///    Only set this to false if the CPU is maxed out (>95% usage).
    /// </param>
    private void Sleep(bool spin = false, bool allowThreadYield = false)
    {
        double sleepStart = _frameTimeWatch.Elapsed.TotalMilliseconds;

        _sleepWatch.Restart();
        double timeLeft = double.MaxValue;
        while ((timeLeft = StatSleepTime - _sleepWatch.Elapsed.TotalMilliseconds) > 0)
        {
            if (spin)
                if (timeLeft < TimerGranularity)
                    Spin(allowThreadYield);

            var sleepTimeLeft = (int)(timeLeft - TimerGranularity);
            Thread.Sleep(sleepTimeLeft >= 1 ? sleepTimeLeft : 1);
        }

        double timeSlept = (_frameTimeWatch.Elapsed.TotalMilliseconds - sleepStart);
        StatOverslept = timeSlept - StatSleepTime;
    }

    /// <summary>
    /// Spins until it is time to begin the next frame.
    /// </summary>
    private void Spin(bool allowThreadYield)
    {
        var lastYieldTime = new TimeSpan();
        double timeLeft;
        while ((timeLeft = _sleepWatch.Elapsed.TotalMilliseconds - StatSleepTime) < 0)
        {
            if (!allowThreadYield)
                continue;

            if (lastYieldTime.TotalMilliseconds > 0)
            {
                /*
                    The 0.15 value is derived from hand testing on Windows 10 20H2 with
                    an i7 4790k and 98-100% CPU load.

                    The 99.9th percentile for calling Sleep(0) has been ~0.13ms, hence I am
                    adding a tiny bit extra to compensate.
                */

                if (timeLeft > lastYieldTime.TotalMilliseconds || timeLeft > 0.15)
                    Thread.Sleep(0);
            }
            else
            {
                _threadYieldWatch.Restart();
                Thread.Sleep(0);
                lastYieldTime = _threadYieldWatch.Elapsed;
            }
        }
    }
}
