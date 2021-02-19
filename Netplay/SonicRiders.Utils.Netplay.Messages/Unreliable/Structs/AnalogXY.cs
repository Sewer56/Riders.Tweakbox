using System;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders.Structures.Gameplay;

namespace Riders.Netplay.Messages.Unreliable.Structs
{
    /// <summary>
    /// Stores analog stick X and Y coordinates.
    /// </summary>
    public struct AnalogXY : Misc.Interfaces.IBitPackable<AnalogXY>
    {
        public sbyte X;
        public sbyte Y;

        /// <inheritdoc />
        public AnalogXY FromStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            return new AnalogXY()
            {
                X = stream.Read<sbyte>(),
                Y = stream.Read<sbyte>()
            };
        }

        /// <inheritdoc />
        public void ToStream<T>(ref BitStream<T> stream) where T : IByteStream
        {
            stream.Write(X);
            stream.Write(Y);
        }

        /// <summary>
        /// Writes the structure to the game.
        /// </summary>
        public unsafe void ToGame(Player* player)
        {
            player->PlayerInput->AnalogStickX = X;
            player->PlayerInput->AnalogStickY = Y;
        }

        /// <summary>
        /// Extracts the structure from the game.
        /// </summary>
        public static unsafe AnalogXY FromGame(Player* player)
        {
            return new AnalogXY()
            {
                X = player->AnalogX,
                Y = player->AnalogY,
            };
        }
    }
}