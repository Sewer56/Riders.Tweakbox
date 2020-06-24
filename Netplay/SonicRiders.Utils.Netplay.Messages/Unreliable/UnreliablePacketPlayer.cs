using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Resources;
using BitStreams;
using EnumsNET;
using Reloaded.Memory.Streams;
using Riders.Netplay.Messages.Misc;
using Sewer56.NumberUtilities;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;

namespace Riders.Netplay.Messages.Unreliable
{
    /// <summary>
    /// Represents the header for an unreliable packet.
    /// </summary>
    public struct UnreliablePacketPlayer : IEquatable<UnreliablePacketPlayer>
    {
        private const double MaxRotation = Math.PI * 2;
        private const float MinLean = -1.0f;
        private const float MaxLean = 1.0f;
        private const float MinTurn = -0.03f;
        private const float MaxTurn = 0.03f;
        private const int RingsBits = 7;
        private const int StateBits = 5;
        private const int AirBits   = 18;

        /// <summary>
        /// Position of the player in X,Y,Z coordinates.
        /// </summary>
        public Vector3? Position;

        /// <summary>
        /// Rotation in the Yaw Direction
        /// </summary>
        public float? Rotation
        {
            get => _rotationX?.GetValue((Float) MaxRotation);
            set
            {
                if (value == null)
                {
                    _rotationX = null;
                }
                else
                {
                    if (value < -0.0f)
                        value = (float)(value + MaxRotation);

                    value = (float)(value % MaxRotation);
                    _rotationX = new CompressedNumber<float, Float, ushort, UShort>(value.Value, (Float)MaxRotation);
                }
            }
        }

        /// <summary>
        /// Total amount of air the player contains.
        /// </summary>
        public uint? Air;

        /// <summary>
        /// The amount of rings the player contains.
        /// </summary>
        public byte? Rings;

        /// <summary>
        /// The last player state.
        /// </summary>
        public PlayerState? LastState;

        /// <summary>
        /// The individual player state.
        /// </summary>
        public PlayerState? State;

        /// <summary>
        /// Flags that control various behaviours.
        /// </summary>
        public PlayerControlFlags? ControlFlags;

        /// <summary>
        /// Velocity of the player in the X and Y direction.
        /// </summary>
        public Vector2? Velocity;

        /// <summary>
        /// Amount the player is currently turning.
        /// </summary>
        public float? TurningAmount
        {
            get => _turnAmount?.GetValue(MinTurn, MaxTurn);
            set
            {
                if (value == null)
                    _turnAmount = null;
                else
                    _turnAmount = new CompressedNumber<float, Float, ushort, UShort>((Float)value.Value, MinTurn, MaxTurn);
            }
        }

        /// <summary>
        /// Amount the player is currently leaning.
        /// </summary>
        public float? LeanAmount
        {
            get => _leanAmount?.GetValue(MinLean, MaxLean);
            set
            {
                if (value == null)
                    _leanAmount = null;
                else
                    _leanAmount = new CompressedNumber<float, Float, ushort, UShort>((Float)value.Value, MinLean, MaxLean);
            }
        }

        /// <summary>
        /// Yaw rotation of the player X.
        /// Measured in radians, i.e. 2PI = 360 degrees.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _rotationX;

        /// <summary>
        /// Amount the player is currently turning.
        /// We send this to try to maintain "consistency" when packets drop.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _turnAmount;

        /// <summary>
        /// Amount the player is currently leaning while performing a drift or turning.
        /// Range -1 to 1.
        /// </summary>
        private CompressedNumber<float, Float, ushort, UShort>? _leanAmount;

        public UnreliablePacketPlayer(Vector3? position, uint? air, byte? rings, PlayerState? state, float? rotationXRadians, Vector2? velocity) : this()
        {
            Position = position;
            Air = air;
            Rings = rings;
            State = state;
            Velocity = velocity;
            Rotation = rotationXRadians;
        }

        /// <summary>
        /// Gets a random instance of a player.
        /// </summary>
        public static UnreliablePacketPlayer GetRandom()
        {
            var random = new Random();
            var result = new UnreliablePacketPlayer();

            result.Position = new Vector3((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            result.Rotation = (float)random.NextDouble();
            result.Air = (uint?) random.Next();
            result.Rings = (byte?) random.Next();
            result.LastState = (PlayerState?)random.Next();
            result.State = (PlayerState?) random.Next();
            result.Velocity = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            result.TurningAmount = (float?) random.NextDouble();
            result.LeanAmount = (float?) random.NextDouble();
            result.ControlFlags = (PlayerControlFlags?) random.Next();

            return result;
        }

        /// <summary>
        /// Serializes the current packet.
        /// </summary>
        public unsafe byte[] Serialize()
        {
            using var memStream = new MemoryStream(sizeof(UnreliablePacketPlayer));
            var bitStream       = new BitStream(memStream);
            bitStream.AutoIncreaseStream = true;

            bitStream.WriteNullable(Position);
            bitStream.WriteNullable(_rotationX);
            bitStream.WriteNullable(Velocity);

            if (Rings.HasValue) bitStream.WriteByte((byte) Rings, RingsBits);
            if (State.HasValue || LastState.HasValue)
            {
                bitStream.WriteByte((byte)State, StateBits);
                bitStream.WriteByte((byte)LastState, StateBits);
            }

            bitStream.WriteNullable(Air, AirBits);
            bitStream.WriteNullable(_turnAmount);
            bitStream.WriteNullable(_leanAmount);
            bitStream.WriteNullable(ControlFlags);

            // Copy back to original stream.
            memStream.Seek(0, SeekOrigin.Begin);
            bitStream.CopyStreamTo(memStream);
            return memStream.ToArray();
        }

        /// <summary>
        /// Serializes an instance of the packet.
        /// </summary>
        public static unsafe UnreliablePacketPlayer Deserialize(BufferedStreamReader reader, HasData fields)
        {
            var player      = new UnreliablePacketPlayer();
            var stream      = reader.BaseStream();
            stream.Position = reader.Position();
            var bitStream   = new BitStream(stream);
            
            bitStream.ReadIfHasFlags(ref player.Position, fields, HasData.HasPosition);
            bitStream.ReadIfHasFlags(ref player._rotationX, fields, HasData.HasRotation);
            bitStream.ReadIfHasFlags(ref player.Velocity, fields, HasData.HasVelocity);
            
            if (fields.HasAllFlags(HasData.HasRings)) player.Rings = bitStream.ReadByte(RingsBits);
            if (fields.HasAllFlags(HasData.HasState))
            {
                player.State = (PlayerState?) bitStream.ReadByte(StateBits);
                player.LastState = (PlayerState?)bitStream.ReadByte(StateBits);
            }

            bitStream.ReadIfHasFlags(ref player.Air, fields, HasData.HasAir, AirBits);
            bitStream.ReadIfHasFlags(ref player._turnAmount, fields, HasData.HasTurnAndLean);
            bitStream.ReadIfHasFlags(ref player._leanAmount, fields, HasData.HasTurnAndLean);
            bitStream.ReadIfHasFlags(ref player.ControlFlags, fields, HasData.HasControlFlags);

            // Seek the BSR
            reader.Seek(stream.Position, SeekOrigin.Begin);
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

            if (ShouldISend(framecounter, HasData.HasPosition)) packet.Position = player.Position;
            if (ShouldISend(framecounter, HasData.HasRotation)) packet.Rotation = player.Rotation.Y;
            if (ShouldISend(framecounter, HasData.HasVelocity)) packet.Velocity = new Vector2(player.Speed, player.VSpeed);
            if (ShouldISend(framecounter, HasData.HasRings)) packet.Rings = (byte?)player.Rings;
            if (ShouldISend(framecounter, HasData.HasState))
            {
                packet.State = player.PlayerState;
                packet.LastState = player.LastPlayerState;
            }
            if (ShouldISend(framecounter, HasData.HasAir)) packet.Air = (uint?)player.Air;
            if (ShouldISend(framecounter, HasData.HasTurnAndLean))
            {
                packet.TurningAmount = player.TurningAmount;
                packet.LeanAmount = player.DriftAngle;
            }
            if (ShouldISend(framecounter, HasData.HasControlFlags)) packet.ControlFlags = player.PlayerControlFlags;

            return packet;
        }

        /// <summary>
        /// Applies the current packet data to a specified player index.
        /// </summary>
        /// <param name="index">Individual player index.</param>
        public unsafe void ToGame(in int index)
        {
            ref var player = ref Player.Players[index];
            if (Position.HasValue) player.Position = Position.Value;
            if (Rotation.HasValue) player.Rotation.Y = Rotation.Value;
            if (Velocity.HasValue)
            {
                player.Speed = Velocity.Value.X;
                player.VSpeed = Velocity.Value.Y;
            }

            if (Rings.HasValue) player.Rings = (int) Rings;
            if (Air.HasValue) player.Air = (int) Air.Value;
            if (TurningAmount.HasValue) player.TurningAmount = TurningAmount.Value;
            if (LeanAmount.HasValue) player.DriftAngle = LeanAmount.Value;
            if (LeanAmount.HasValue) player.DriftAngle = LeanAmount.Value;
            if (ControlFlags.HasValue) player.PlayerControlFlags = ControlFlags.Value;
            if (State.HasValue)
            {
                player.LastPlayerState = LastState.Value;
                
                if (!IsCurrentStateBlacklisted(player.PlayerState) && IsWhitelisted(State.Value))
                    player.PlayerState = State.Value;

                player.MaybeAttackLastState = PlayerState.None;
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
            // Used to have settings here, removed for now.
            // Will be implemented if we ever need to reduce bandwidth usage.
            return true;
        }

        public bool IsDefault() => this.Equals(new UnreliablePacketPlayer());

        private static bool ShouldISendFrequency(int framecounter, int frequency) =>
            framecounter % ((int) (60 / frequency)) == 0;


        public bool IsWhitelisted(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Running:
                case PlayerState.NormalOnBoard:
                case PlayerState.Jump:
                case PlayerState.ElectricShockLong:
                case PlayerState.InstantStop:
                case PlayerState.Flying:
                case PlayerState.RunningAfterStart:
                case PlayerState.ElectricShock:
                case PlayerState.Turbulence:
                case PlayerState.FreeFalling:
                case PlayerState.TrickJumpTurbulence:
                case PlayerState.TrickJumpFlatVertical:
                case PlayerState.TrickJumpUnknown1:
                case PlayerState.TrickJumpVertical:
                case PlayerState.TrickJumpUnknown2:
                case PlayerState.TrickJumpHorizontal:
                    return true;

                default:
                case PlayerState.ElectricShockCrash:
                case PlayerState.Reset:
                case PlayerState.Retire:
                case PlayerState.Grinding:
                case PlayerState.RotateSection:
                case PlayerState.Attacking:
                case PlayerState.GettingAttacked:
                    return false;
            }
        }

        public bool IsCurrentStateBlacklisted(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Attacking:
                case PlayerState.GettingAttacked:
                case PlayerState.Turbulence:
                    return true;

                default:
                case PlayerState.FreeFalling:
                case PlayerState.Running:
                case PlayerState.NormalOnBoard:
                case PlayerState.Jump:
                case PlayerState.ElectricShockLong:
                case PlayerState.InstantStop:
                case PlayerState.Flying:
                case PlayerState.RunningAfterStart:
                case PlayerState.ElectricShock:
                case PlayerState.TrickJumpTurbulence:
                case PlayerState.TrickJumpFlatVertical:
                case PlayerState.TrickJumpUnknown1:
                case PlayerState.TrickJumpVertical:
                case PlayerState.TrickJumpUnknown2:
                case PlayerState.TrickJumpHorizontal:
                case PlayerState.ElectricShockCrash:
                case PlayerState.Reset:
                case PlayerState.Retire:
                case PlayerState.Grinding:
                case PlayerState.RotateSection:
                    return false;
            }

            return (state == PlayerState.Attacking || state == PlayerState.GettingAttacked);
        }

        #region Autogenerated by R#
        /// <inheritdoc />
        public bool Equals(UnreliablePacketPlayer other)
        {
            return Nullable.Equals(Position, other.Position) && Air == other.Air && Rings == other.Rings && LastState == other.LastState && State == other.State && Nullable.Equals(Velocity, other.Velocity) && Nullable.Equals(_rotationX, other._rotationX);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is UnreliablePacketPlayer other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Air, Rings, LastState, State, Velocity, _rotationX);
        }
        #endregion
    }
}