using System;
using System.Numerics;
using Reloaded.Memory.Streams;
using Sewer56.NumberUtilities;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Utility;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    public struct UnreliablePacketPlayer : IEquatable<UnreliablePacketPlayer>
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

        public UnreliablePacketPlayer(Vector3? position, byte? rings,  PlayerState? state, float? rotationXRadians, float? velocityX, float? velocityY) : this()
        {
            Position = position;
            Rings = rings;
            State = state;
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
            return writer.ToArray();
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static unsafe UnreliablePacketPlayer Deserialize(BufferedStreamReader reader, UnreliablePacketHeader.HasData fields)
        {
            var player = new UnreliablePacketPlayer();
            reader.SetValueIfHasFlags(ref player.Position, fields, UnreliablePacketHeader.HasData.HasPosition);
            reader.SetValueIfHasFlags(ref player._rotationX, fields, UnreliablePacketHeader.HasData.HasRotation);
            reader.SetValueIfHasFlags(ref player._velocityX, fields, UnreliablePacketHeader.HasData.HasVelocity);
            reader.SetValueIfHasFlags(ref player._velocityY, fields, UnreliablePacketHeader.HasData.HasVelocity);
            reader.SetValueIfHasFlags(ref player.Rings, fields, UnreliablePacketHeader.HasData.HasRings);
            reader.SetValueIfHasFlags(ref player.State, fields, UnreliablePacketHeader.HasData.HasState);
            return player;
        }

        // Autogenerated by R#
        public bool Equals(UnreliablePacketPlayer other) => Position.Equals(other.Position) && Rings == other.Rings && State == other.State && _rotationX.Equals(other._rotationX) && _velocityX.Equals(other._velocityX) && _velocityY.Equals(other._velocityY);
        public override bool Equals(object obj) => obj is UnreliablePacketPlayer other && Equals(other);
        public override int GetHashCode() => Position.GetHashCode();
    }
}