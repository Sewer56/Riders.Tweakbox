﻿using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.API;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Netplay.Messages.Reliable.Structs.Gameplay.Struct
{
    public struct LapCounter : Misc.Interfaces.IBitPackable<LapCounter>, IEquatable<LapCounter>
    {
        public const int CounterBits = 7;
        public const int TimerBits   = 24; // Last byte is unused.

        /// <summary>
        /// The current value of the lap.
        /// </summary>
        public byte Counter;

        /// <summary>
        /// Current value of the race timer.
        /// </summary>
        public int Timer;

        public unsafe LapCounter(Player* player) : this(player->LapCounter, *State.StageTimer) { }
        public LapCounter(byte counter, int timer)
        {
            Counter = counter;
            Timer = timer;
        }

        /// <inheritdoc />
        public LapCounter FromStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            return new LapCounter()
            {
                Counter = stream.Read<byte>(CounterBits),
                Timer = stream.Read<int>(TimerBits)
            };
        }

        /// <inheritdoc />
        public void ToStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            stream.Write(Counter, CounterBits);
            stream.Write(Timer, TimerBits);
        }

        #region Autogenerated by R#
        /// <inheritdoc />
        public override bool Equals(object obj) => obj is LapCounter other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Counter.GetHashCode();
        public bool Equals(LapCounter other) => Counter == other.Counter;
        #endregion
    }
}
