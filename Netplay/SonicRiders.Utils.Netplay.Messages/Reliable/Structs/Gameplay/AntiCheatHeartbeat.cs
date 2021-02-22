using System;
using System.Diagnostics;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay
{
    public struct AntiCheatHeartbeat : IReliableMessage
    {
        public const int HeartbeatIntervalFrames = 300;
        public const float MarginOfError         = 1.02f;

        /// <summary>
        /// The Date/Time at which the Heartbeat was generated.
        /// (Unaffected by speed hacks)
        /// </summary>
        public DateTime DateTime;

        /// <summary>
        /// Frames elapsed since last Heartbeat.
        /// (Affected by speed hacks)
        /// </summary>
        public short FramesElapsed;

        public AntiCheatHeartbeat(short framesElapsed) : this()
        {
            DateTime = DateTime.UtcNow;
            FramesElapsed = framesElapsed;
        }

        public AntiCheatHeartbeat(DateTime dateTime, short framesElapsed)
        {
            DateTime = dateTime;
            FramesElapsed = framesElapsed;
        }

        /// <summary>
        /// Returns true if a player is cheating, else false.
        /// </summary>
        /// <param name="last">The last heartbeat result.</param>
        /// <param name="reason">Cheat description.</param>
        public bool IsCheating(AntiCheatHeartbeat last, out string reason)
        {
            var realTimeSinceLast = DateTime - last.DateTime.ToUniversalTime();
            var realFramesElapsed = ToFrames(realTimeSinceLast);

            // Real-time check. (Type A) 
            if (FramesElapsed > (realFramesElapsed * MarginOfError))
            {
                reason = "Unknown Speed Hack";
                return true;
            }

            // DLL Check
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.Equals("speedhack-i386.dll", StringComparison.OrdinalIgnoreCase))
                {
                    reason = "Cheat Engine";
                    return true;
                }
            }

            reason = "";
            return false;
        }

        private float ToFrames(TimeSpan span) => (int) (span.TotalMilliseconds / 16.66667F);

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public readonly MessageType GetMessageType() => MessageType.AntiCheatHeartbeat;

        /// <inheritdoc />
        public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            DateTime      = bitStream.ReadGeneric<DateTime>();
            FramesElapsed = bitStream.ReadGeneric<short>();
        }

        /// <inheritdoc />
        public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
        {
            bitStream.WriteGeneric(DateTime);
            bitStream.WriteGeneric(FramesElapsed);
        }
    }
}