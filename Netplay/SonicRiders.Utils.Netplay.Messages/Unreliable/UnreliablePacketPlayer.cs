using System;
using System.IO;
using System.Numerics;
using BitStreams;
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
        private const int AnimationBits = 7;
        private const int LapsBits = 7;

        /// <summary>
        /// Position of the player in X,Y,Z coordinates.
        /// </summary>
        public Vector3? Position;

        /// <summary>
        /// Rotation in the Yaw Direction
        /// </summary>
        public float? RotationX
        {
            get => _rotationX?.GetValue((Float) MaxRotation);
            set => SetRotation(ref _rotationX, value);
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
        public byte? LastState;

        /// <summary>
        /// The individual player state.
        /// </summary>
        public byte? State;

        /// <summary>
        /// Last animation index.
        /// </summary>
        public byte? LastAnimation;

        /// <summary>
        /// Current animation index.
        /// </summary>
        public byte? Animation;

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
        /// We send this to try to maintain "consistency" when packets drop.
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
        /// Amount the player is currently leaning. Range -1 to 1.
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
        /// Current lap counter.
        /// </summary>
        public byte? LapCounter;

        private CompressedNumber<float, Float, ushort, UShort>? _rotationX;
        private CompressedNumber<float, Float, ushort, UShort>? _turnAmount;
        private CompressedNumber<float, Float, ushort, UShort>? _leanAmount;

        public UnreliablePacketPlayer(Vector3? position, uint? air, byte? rings, PlayerState? lastState, PlayerState? state, float? rotationXRadians, Vector2? velocity) : this()
        {
            Position = position;
            Air = air;
            Rings = rings;
            LastState = (byte?) lastState;
            State = (byte?) state;
            Velocity = velocity;
            RotationX = rotationXRadians;
        }

        /// <summary>
        /// Gets a random instance of a player.
        /// </summary>
        public static UnreliablePacketPlayer GetRandom(int framecounter)
        {
            var random = new Random();
            var result = new UnreliablePacketPlayer();

            if (ShouldISend(framecounter, HasData.HasPosition)) result.Position = new Vector3((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            if (ShouldISend(framecounter, HasData.HasRotation)) result.RotationX = (float)random.NextDouble();
            if (ShouldISend(framecounter, HasData.HasAir)) result.Air = (uint?) random.Next(0, 200000);
            if (ShouldISend(framecounter, HasData.HasRings)) result.Rings = (byte?) random.Next(0, 99);
            if (ShouldISend(framecounter, HasData.HasState)) result.LastState = (byte?) random.Next(0, 32);
            if (ShouldISend(framecounter, HasData.HasState)) result.State = (byte?) random.Next(0, 32);
            if (ShouldISend(framecounter, HasData.HasVelocity)) result.Velocity = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            if (ShouldISend(framecounter, HasData.HasTurnAndLean)) result.TurningAmount = (float?) random.NextDouble();
            if (ShouldISend(framecounter, HasData.HasTurnAndLean)) result.LeanAmount = (float?) random.NextDouble();
            if (ShouldISend(framecounter, HasData.HasControlFlags)) result.ControlFlags = (PlayerControlFlags?) random.Next();
            if (ShouldISend(framecounter, HasData.HasAnimation)) result.Animation = (byte?)random.Next(0, 99);
            if (ShouldISend(framecounter, HasData.HasAnimation)) result.LastAnimation = (byte?)random.Next(0, 99);
            if (ShouldISend(framecounter, HasData.HasLapCounter)) result.LapCounter = (byte?)random.Next(0, 99);
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
            bitStream.WriteNullable(_turnAmount);
            bitStream.WriteNullable(_leanAmount);
            bitStream.WriteNullable(ControlFlags);

            bitStream.WriteNullable((byte?) Rings, RingsBits);
            bitStream.WriteNullable((byte?) State, StateBits);
            bitStream.WriteNullable((byte?) LastState, StateBits);
            bitStream.WriteNullable(Air, AirBits);
            bitStream.WriteNullable(Animation, AnimationBits);
            bitStream.WriteNullable(LastAnimation, AnimationBits);
            bitStream.WriteNullable((byte?) LapCounter, LapsBits);

            // Write final byte and copy back to original stream.
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
            var originalPos = reader.Position();
            stream.Position = originalPos;
            var bitStream   = new BitStream(stream);
            
            bitStream.ReadIfHasFlags(ref player.Position, fields, HasData.HasPosition);
            bitStream.ReadIfHasFlags(ref player._rotationX, fields, HasData.HasRotation);
            bitStream.ReadIfHasFlags(ref player.Velocity, fields, HasData.HasVelocity);
            bitStream.ReadIfHasFlags(ref player._turnAmount, fields, HasData.HasTurnAndLean);
            bitStream.ReadIfHasFlags(ref player._leanAmount, fields, HasData.HasTurnAndLean);
            bitStream.ReadIfHasFlags(ref player.ControlFlags, fields, HasData.HasControlFlags);

            bitStream.ReadIfHasFlags(ref player.Rings, fields, HasData.HasRings, RingsBits);
            bitStream.ReadIfHasFlags(ref player.State, fields, HasData.HasState, StateBits);
            bitStream.ReadIfHasFlags(ref player.LastState, fields, HasData.HasState, StateBits);
            bitStream.ReadIfHasFlags(ref player.Air, fields, HasData.HasAir, AirBits);
            bitStream.ReadIfHasFlags(ref player.Animation, fields, HasData.HasAnimation, AnimationBits);
            bitStream.ReadIfHasFlags(ref player.LastAnimation, fields, HasData.HasAnimation, AnimationBits);
            bitStream.ReadIfHasFlags(ref player.LapCounter, fields, HasData.HasLapCounter, LapsBits);

            // Seek the BSR
            var bitStreamPos = bitStream.GetStream().Position;
            var extraByte    = bitStream.BitPosition != 0 ? 1 : 0;
            reader.Seek(originalPos + bitStreamPos + extraByte, SeekOrigin.Begin);

            // TODO: Hack that removes a turbulence related flag in order to prevent crashing in the meantime.
            // Real way to get around this crash is not yet known; albeit the crash happens at 00457EC9
            player.ControlFlags &= ~PlayerControlFlags.TurbulenceHairpinTurnSymbol;

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
            if (ShouldISend(framecounter, HasData.HasRotation)) packet.RotationX = player.Rotation.Y;
            if (ShouldISend(framecounter, HasData.HasVelocity)) packet.Velocity = new Vector2(player.Speed, player.VSpeed);
            if (ShouldISend(framecounter, HasData.HasRings)) packet.Rings = (byte?)player.Rings;
            if (ShouldISend(framecounter, HasData.HasState))
            {
                packet.State = (byte?) player.PlayerState;
                packet.LastState = (byte?) player.LastPlayerState;
            }
            if (ShouldISend(framecounter, HasData.HasAir)) packet.Air = (uint?)player.Air;
            if (ShouldISend(framecounter, HasData.HasTurnAndLean))
            {
                packet.TurningAmount = player.TurningAmount;
                packet.LeanAmount = player.DriftAngle;
            }
            if (ShouldISend(framecounter, HasData.HasControlFlags)) packet.ControlFlags = player.PlayerControlFlags;
            if (ShouldISend(framecounter, HasData.HasAnimation))
            {
                packet.Animation = (byte?) player.Animation;
                packet.LastAnimation = (byte?) player.LastAnimation;
            }
            if (ShouldISend(framecounter, HasData.HasLapCounter)) packet.LapCounter = player.LapCounter;

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
            if (RotationX.HasValue) player.Rotation.Y = RotationX.Value;

            if (Velocity.HasValue)
            {
                player.Speed = Velocity.Value.X;
                player.VSpeed = Velocity.Value.Y;
            }

            if (Rings.HasValue) player.Rings = (int) Rings;
            if (Air.HasValue) player.Air = (int) Air.Value;
            if (TurningAmount.HasValue) player.TurningAmount = TurningAmount.Value;
            if (LeanAmount.HasValue) player.DriftAngleCopy = LeanAmount.Value;
            if (LeanAmount.HasValue) player.DriftAngle = LeanAmount.Value;
            if (ControlFlags.HasValue) player.PlayerControlFlags = ControlFlags.Value;
            if (State.HasValue)
            {
                if (!IsCurrentStateBlacklisted(player.LastPlayerState) && IsWhitelisted((PlayerState) LastState.Value))
                    player.LastPlayerState = (PlayerState) LastState.Value;

                if (!IsCurrentStateBlacklisted(player.PlayerState) && IsWhitelisted((PlayerState) State.Value))
                    player.PlayerState = (PlayerState) State.Value;
            }

            if (Animation.HasValue)
            {
                if (Animation.Value != 0)
                    player.Animation = (CharacterAnimation) Animation.Value;
                
                if (LastAnimation.Value != 0)
                    player.LastAnimation = (CharacterAnimation) LastAnimation.Value;
            }

            if (LapCounter.HasValue) player.LapCounter = (byte)LapCounter;

            // Hack for Misc Bugs
            // - Sometimes rotation value not updated after certain state transitions.
            // - Sometimes repeated attacks do not work.
            player.MaybeAttackLastState = PlayerState.None;
        }

        /// <summary>
        /// Determines if on a given frame, a piece of data should be sent.
        /// </summary>
        /// <param name="frameCounter">The current frame counter.</param>
        /// <param name="type">The type of data.</param>
        /// <returns>True if should be sent, else false.</returns>
        public static bool ShouldISend(int frameCounter, HasData type)
        {
            // State is currently disabled, seems to be redundant since we synced animations.
            if (type == HasData.HasState)
                return false;

            switch (type)
            {
                case HasData.HasLapCounter:
                    return ShouldISendFrequency(frameCounter, 10);

                case HasData.HasRings:
                case HasData.HasAir:
                    return ShouldISendFrequency(frameCounter, 30);
            }

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
                case PlayerState.FreeFalling:
                    return true;

                default:
                case PlayerState.TrickJumpFlatVertical:
                case PlayerState.TrickJumpUnknown1:
                case PlayerState.TrickJumpVertical:
                case PlayerState.TrickJumpUnknown2:
                case PlayerState.TrickJumpHorizontal:
                case PlayerState.Turbulence:
                case PlayerState.TrickJumpTurbulence:
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
                default:
                case PlayerState.Attacking:
                case PlayerState.GettingAttacked:
                case PlayerState.Turbulence:
                    return true;

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
        }

        private void SetRotation(ref CompressedNumber<float, Float, ushort, UShort>? rotation, float? value)
        {
            if (value == null)
            {
                rotation = null;
                return;
            }

            if (value < -0.0f)
                value = (float)(value + MaxRotation);

            value = (float)(value % MaxRotation);
            rotation = new CompressedNumber<float, Float, ushort, UShort>(value.Value, (Float)MaxRotation);
        }

        #region Autogenerated by R#
        /// <inheritdoc />
        public bool Equals(UnreliablePacketPlayer other)
        {
            return Nullable.Equals(Position, other.Position) && Air == other.Air && Rings == other.Rings && LastState == other.LastState && State == other.State && LastAnimation == other.LastAnimation && Animation == other.Animation && Nullable.Equals(Velocity, other.Velocity) && LapCounter == other.LapCounter && Nullable.Equals(_rotationX, other._rotationX) && Nullable.Equals(_turnAmount, other._turnAmount) && Nullable.Equals(_leanAmount, other._leanAmount);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is UnreliablePacketPlayer other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Position);
            hashCode.Add(Air);
            hashCode.Add(Rings);
            hashCode.Add(LastState);
            hashCode.Add(State);
            hashCode.Add(LastAnimation);
            hashCode.Add(Animation);
            hashCode.Add(Velocity);
            hashCode.Add(LapCounter);
            hashCode.Add(_rotationX);
            hashCode.Add(_turnAmount);
            hashCode.Add(_leanAmount);
            return hashCode.ToHashCode();
        }
        #endregion
    }
}