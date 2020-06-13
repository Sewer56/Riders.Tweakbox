using System;
using System.Numerics;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Sewer56.NumberUtilities;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Utility;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public struct UnreliablePacketPlayer
    {
        private const double MaxRotation = Math.PI * 2;
        private static readonly float CompressedMaxVelocity = Formula.SpeedometerToFloat(1080);

        /*
            // Player Data  (All Optional)
            Position          = 12 bytes
            Rotation          = 2 bytes (Compressed as BAMS)
            Velocity          = 4 bytes (Compressed Floats)   (30Hz)
            Rings             = 1 byte  (5Hz)
            State             = 1 byte  (10Hz/OnDemand)
            Latency           = 2 bytes (4Hz/OnDemand)

            // TODO: Animation
            Animation                                         (10Hz)
            {
                Animation Id    = 1 byte 
                Animation Frame = 2 bytes
            }
        */

        /// <summary>
        /// Position of the player in X,Y,Z coordinates.
        /// </summary>
        public Vector3? Position;

        /// <summary>
        /// The amount of rings the player contains.
        /// </summary>
        public byte? Rings;

        /// <summary>
        /// The individual player state.
        /// </summary>
        public PlayerState? State;

        /// <summary>
        /// Individual player delay/latency.
        /// </summary>
        public short? Latency;

        /// <summary>
        /// Yaw rotation of the player X.
        /// Measured in radians, i.e. 2PI = 360 degrees.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _rotationX;

        /// <summary>
        /// Velocity of the player in X direction.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _velocityX;

        /// <summary>
        /// Velocity of the player in Y direction.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _velocityY;

        public UnreliablePacketPlayer(Vector3? position, byte? rings,  PlayerState? state, float? rotationXRadians, float? velocityX, float? velocityY, short? latency) : this()
        {
            Position = position;
            Rings = rings;
            State = state;
            Latency = latency;
            _rotationX = null;
            _velocityX = null;
            _velocityY = null;

            if (rotationXRadians.HasValue)
                SetRotationX(rotationXRadians.Value);

            if (velocityX.HasValue && velocityY.HasValue)
                SetVelocityXY(velocityX.Value, velocityY.Value);
        }

        /// <summary>
        /// Retrieves the Rotation in the X (Yaw) direction as a floating point number in radians.
        /// </summary>
        /// <returns></returns>
        public float? GetRotationX() => _rotationX?.GetValue((Float)MaxRotation);

        /// <summary>
        /// Gets the Velocity of the player in the X direction.
        /// </summary>
        public float? GetVelocityX() => _velocityX?.GetValue(CompressedMaxVelocity);

        /// <summary>
        /// Gets the Velocity of the player in the Y direction.
        /// </summary>
        public float? GetVelocityY() => _velocityY?.GetValue(CompressedMaxVelocity);

        /// <summary>
        /// Sets the Rotation in the X (Yaw) direction as a floating point number in radians.
        /// </summary>
        public void SetRotationX(float rotationXRadians)
        {
            if (rotationXRadians < -0.0f)
                rotationXRadians = (float)(rotationXRadians + MaxRotation);

            rotationXRadians = (float)(rotationXRadians % MaxRotation);
            _rotationX = new CompressedNumber<float, Float, ushort, UShort>(rotationXRadians, (Float)MaxRotation);
        }

        /// <summary>
        /// Sets the Velocity of the player in the X and Y directions.
        /// </summary>
        public void SetVelocityXY(float velocityX, float velocityY)
        {
            _velocityX = new CompressedNumber<float, Float, ushort, UShort>(velocityX, CompressedMaxVelocity);
            _velocityY = new CompressedNumber<float, Float, ushort, UShort>(velocityY, CompressedMaxVelocity);
        }

        /// <summary>
        /// Serializes the current packet.
        /// </summary>
        public unsafe byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(sizeof(UnreliablePacketHeader));
            writer.WriteNullable(Position);
            writer.WriteNullable(_rotationX);
            writer.WriteNullable(_velocityX);
            writer.WriteNullable(_velocityY);
            writer.WriteNullable(Rings);
            writer.WriteNullable(State);
            writer.WriteNullable(Latency);
            return writer.ToArray();
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static unsafe UnreliablePacketPlayer Deserialize(BufferedStreamReader reader, HasData fields)
        {
            var player = new UnreliablePacketPlayer();
            reader.SetValueIfHasFlags(ref player.Position, fields, HasData.HasPosition);
            reader.SetValueIfHasFlags(ref player._rotationX, fields, HasData.HasRotation);
            reader.SetValueIfHasFlags(ref player._velocityX, fields, HasData.HasVelocity);
            reader.SetValueIfHasFlags(ref player._velocityY, fields, HasData.HasVelocity);
            reader.SetValueIfHasFlags(ref player.Rings, fields, HasData.HasRings);
            reader.SetValueIfHasFlags(ref player.State, fields, HasData.HasState);
            reader.SetValueIfHasFlags(ref player.Latency, fields, HasData.HasLatency);
            return player;
        }

        /// <summary>
        /// Gets the player status from game memory.
        /// </summary>
        public static UnreliablePacketPlayer FromGame()
        {
            return new UnreliablePacketPlayer();
        }

        /// <summary>
        /// Determines if on a given frame, a piece of data should be sent.
        /// </summary>
        /// <param name="frameCounter">The current frame counter.</param>
        /// <param name="type">The type of data.</param>
        /// <returns>True if should be sent, else false.</returns>
        public static bool ShouldISend(int frameCounter, HasData type)
        {
            switch (type)
            {
                case HasData.HasPosition:
                case HasData.HasRotation:
                    return true;
                case HasData.HasVelocity:
                    return ShouldISendFrequency(frameCounter, 30);
                case HasData.HasRings:
                    return ShouldISendFrequency(frameCounter, 5);
                case HasData.HasState:
                    return ShouldISendFrequency(frameCounter, 10);
                case HasData.HasLatency:
                    return ShouldISendFrequency(frameCounter, 4);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static bool ShouldISendFrequency(int framecounter, int frequency) =>
            framecounter % ((int) (60 / frequency)) == 0;
    }
}