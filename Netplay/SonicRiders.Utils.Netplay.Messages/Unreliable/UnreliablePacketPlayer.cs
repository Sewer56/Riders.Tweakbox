using System;
using System.Numerics;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Sewer56.NumberUtilities;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using Sewer56.SonicRiders.Utility;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    public struct UnreliablePacketPlayer : IEquatable<UnreliablePacketPlayer>
    {
        private const double MaxRotation = Math.PI * 2;
        private static readonly float CompressedMinVelocity = Formula.SpeedometerToFloat(-1080);
        private static readonly float CompressedMaxVelocity = Formula.SpeedometerToFloat(1080);

        /*
            // Player Data  (All Optional)
            Position          = 12 bytes
            Rotation          = 2 bytes (Compressed as BAMS)
            Velocity          = 8 bytes (Floats) (30Hz)
            Rings             = 1 byte  (5Hz)
            State             = 1 byte  (10Hz/OnDemand)

            // TODO: Animation
            Animation (10Hz)
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
        /// Velocity of the player in the X and Y direction.
        /// </summary>
        public Vector2? Velocity;

        /// <summary>
        /// Yaw rotation of the player X.
        /// Measured in radians, i.e. 2PI = 360 degrees.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _rotationX;

        public UnreliablePacketPlayer(Vector3? position, byte? rings,  PlayerState? state, float? rotationXRadians, Vector2? velocity) : this()
        {
            Position = position;
            Rings = rings;
            State = state;
            Velocity = velocity;
            _rotationX = null;

            if (rotationXRadians.HasValue)
                SetRotationX(rotationXRadians.Value);
        }

        /// <summary>
        /// Retrieves the Rotation in the X (Yaw) direction as a floating point number in radians.
        /// </summary>
        /// <returns></returns>
        public float? GetRotationX() => _rotationX?.GetValue((Float)MaxRotation);

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
        /// Serializes the current packet.
        /// </summary>
        public unsafe byte[] Serialize()
        {
            using var writer = new ExtendedMemoryStream(sizeof(UnreliablePacketPlayer));
            writer.WriteNullable(Position);
            writer.WriteNullable(_rotationX);
            writer.WriteNullable(Velocity);
            writer.WriteNullable(Rings);
            writer.WriteNullable(State);
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
            reader.SetValueIfHasFlags(ref player.Velocity, fields, HasData.HasVelocity);
            reader.SetValueIfHasFlags(ref player.Rings, fields, HasData.HasRings);
            reader.SetValueIfHasFlags(ref player.State, fields, HasData.HasState);
            return player;
        }

        /// <summary>
        /// Gets a packet for an individual player given the index of the player.
        /// </summary>
        /// <param name="index">The current player index (0-7).</param>
        /// <param name="framecounter">The current frame counter used to include/exclude values.</param>
        public static UnreliablePacketPlayer FromGame(int index, int framecounter = 0)
        {
            ref var player = ref Player.Players[index];
            var packet = new UnreliablePacketPlayer();

            packet.Position = player.Position;
            packet.SetRotationX(player.Rotation.Y);
            packet.Velocity = new Vector2(player.Speed, player.VSpeed);
            packet.Rings = (byte?)player.Rings;
            packet.State = player.PlayerState;

            /*
            if (ShouldISend(framecounter, HasData.HasPosition)) packet.Position = player.Position;
            if (ShouldISend(framecounter, HasData.HasRotation)) packet.SetRotationX(player.Rotation.Y);
            if (ShouldISend(framecounter, HasData.HasVelocity)) packet.Velocity = new Vector2(player.Speed, player.VSpeed);
            if (ShouldISend(framecounter, HasData.HasRings)) packet.Rings = (byte?)player.Rings;
            if (ShouldISend(framecounter, HasData.HasState)) packet.State = player.PlayerState;
            */

            return packet;
        }

        /// <summary>
        /// Applies the current packet data to a specified player index.
        /// </summary>
        /// <param name="index">Individual player index.</param>
        public void ToGame(in int index)
        {
            ref var player = ref Player.Players[index];
            if (Position.HasValue) player.Position = Position.Value;
            if (GetRotationX().HasValue) player.Rotation.Y = GetRotationX().Value;
            if (Velocity.HasValue)
            {
                player.Speed = Velocity.Value.X;
                player.VSpeed = Velocity.Value.Y;
            }

            if (Rings.HasValue) player.Rings = (int) Rings;
            if (State.HasValue)
            {
                //player.LastPlayerState = player.PlayerState;
                //player.PlayerState = (PlayerState) State;
            }

            // TODO: Check if setting last state is the correct thing to do
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public bool IsDefault() => this.Equals(new UnreliablePacketPlayer());

        private static bool ShouldISendFrequency(int framecounter, int frequency) =>
            framecounter % ((int) (60 / frequency)) == 0;

        #region Autogenerated by R#
        public bool Equals(UnreliablePacketPlayer other)
        {
            return Nullable.Equals(Position, other.Position) && Rings == other.Rings && State == other.State && Nullable.Equals(_rotationX, other._rotationX) && Nullable.Equals(Velocity, other.Velocity);
        }

        public override bool Equals(object obj)
        {
            return obj is UnreliablePacketPlayer other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rings, State, _rotationX, Velocity);
        }
        #endregion
    }
}