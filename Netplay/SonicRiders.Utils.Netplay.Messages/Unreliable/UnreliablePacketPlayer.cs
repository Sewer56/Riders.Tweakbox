using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using EnumsNET;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc;
using Riders.Netplay.Messages.Unreliable.Enums;
using Riders.Netplay.Messages.Unreliable.Structs;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.NumberUtilities;
using Sewer56.NumberUtilities.Primitives;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Enums;
using static Riders.Netplay.Messages.Unreliable.UnreliablePacketHeader;
using MovementFlags = Riders.Netplay.Messages.Unreliable.Structs.MovementFlags;
namespace Riders.Netplay.Messages.Unreliable;

/// <summary>
/// Represents the header for an unreliable packet.
/// </summary>
[Equals(DoNotAddEqualityOperators = true)]
public struct UnreliablePacketPlayer
{
    private const double MaxRotation = Math.PI * 2;
    private const float MinLean = -1.0f;
    private const float MaxLean = 1.0f;
    private const float MinTurn = -0.03f;
    private const float MaxTurn = 0.03f;
    private const int RingsBits = 7;
    private const int StateBits = 5;
    private const int AirBits = 18;
    private const int AnimationBits = 7;
    private static readonly int ControlFlagsBits = EnumNumBits<MinControlFlags>.Number;

    /// <summary>
    /// Position of the player in X,Y,Z coordinates.
    /// </summary>
    public Vector3? Position;

    /// <summary>
    /// Rotation in the Yaw Direction
    /// </summary>
    public float? RotationX
    {
        get => _rotationX?.GetValue((Float)MaxRotation);
        set => SetRotation(ref _rotationX, value);
    }

    /// <summary>
    /// Rotation in the Pitch Direction
    /// </summary>
    public float? RotationY
    {
        get => _rotationY?.GetValue((Float)MaxRotation);
        set => SetRotation(ref _rotationY, value);
    }

    /// <summary>
    /// Rotation in the Roll Direction
    /// </summary>
    public float? RotationZ
    {
        get => _rotationZ?.GetValue((Float)MaxRotation);
        set => SetRotation(ref _rotationZ, value);
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
    public MinControlFlags? ControlFlags;

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
    /// Stores the movement flags related to drift/break/charge jump for the player.
    /// </summary>
    public MovementFlags? MovementFlags;

    /// <summary>
    /// X and Y movement of the analog stick.
    /// </summary>
    public AnalogXY? AnalogXY;

    private CompressedNumber<float, Float, ushort, UShort>? _rotationX;
    private CompressedNumber<float, Float, ushort, UShort>? _rotationY;
    private CompressedNumber<float, Float, ushort, UShort>? _rotationZ;
    private CompressedNumber<float, Float, ushort, UShort>? _turnAmount;
    private CompressedNumber<float, Float, ushort, UShort>? _leanAmount;

    /// <summary>
    /// Serializes the current packet.
    /// </summary>
    public unsafe void Serialize<TByteSource>(ref BitStream<TByteSource> bitStream, HasData data = HasDataAll) where TByteSource : IByteStream
    {
        if (data.HasAllFlags(HasData.HasPosition)) bitStream.WriteGeneric(Position.GetValueOrDefault());
        if (data.HasAllFlags(HasData.HasRotation))
        {
            bitStream.WriteGeneric(_rotationX.GetValueOrDefault());
            bitStream.WriteGeneric(_rotationY.GetValueOrDefault());
            bitStream.WriteGeneric(_rotationZ.GetValueOrDefault());
        }

        if (data.HasAllFlags(HasData.HasVelocity)) bitStream.WriteGeneric(Velocity.GetValueOrDefault());
        if (data.HasAllFlags(HasData.HasTurnAndLean))
        {
            bitStream.WriteGeneric(_turnAmount.GetValueOrDefault());
            bitStream.WriteGeneric(_leanAmount.GetValueOrDefault());
        }

        if (data.HasAllFlags(HasData.HasControlFlags)) bitStream.WriteGeneric((int)ControlFlags.GetValueOrDefault(), ControlFlagsBits);
        if (data.HasAllFlags(HasData.HasRings)) bitStream.Write<byte>(Rings.GetValueOrDefault(), RingsBits);
        if (data.HasAllFlags(HasData.HasState))
        {
            bitStream.Write(State.GetValueOrDefault(), StateBits);
            bitStream.Write(LastState.GetValueOrDefault(), StateBits);
        }

        if (data.HasAllFlags(HasData.HasAir)) bitStream.Write<uint>(Air.GetValueOrDefault(), AirBits);
        if (data.HasAllFlags(HasData.HasAnimation))
        {
            bitStream.Write(Animation.GetValueOrDefault(), AnimationBits);
            bitStream.Write(LastAnimation.GetValueOrDefault(), AnimationBits);
        }

        if (data.HasAllFlags(HasData.HasMovementFlags)) MovementFlags.GetValueOrDefault().ToStream(ref bitStream);
        if (data.HasAllFlags(HasData.HasAnalogInput)) AnalogXY.GetValueOrDefault().ToStream(ref bitStream);
    }

    /// <summary>
    /// Serializes an instance of the packet.
    /// </summary>
    public static unsafe UnreliablePacketPlayer Deserialize<TByteSource>(ref BitStream<TByteSource> bitStream, HasData fields) where TByteSource : IByteStream
    {
        var player = new UnreliablePacketPlayer();

        bitStream.ReadStructIfHasFlags(ref player.Position, fields, HasData.HasPosition);
        bitStream.ReadStructIfHasFlags(ref player._rotationX, fields, HasData.HasRotation);
        bitStream.ReadStructIfHasFlags(ref player._rotationY, fields, HasData.HasRotation);
        bitStream.ReadStructIfHasFlags(ref player._rotationZ, fields, HasData.HasRotation);
        bitStream.ReadStructIfHasFlags(ref player.Velocity, fields, HasData.HasVelocity);
        bitStream.ReadStructIfHasFlags(ref player._turnAmount, fields, HasData.HasTurnAndLean);
        bitStream.ReadStructIfHasFlags(ref player._leanAmount, fields, HasData.HasTurnAndLean);
        bitStream.ReadStructIfHasFlags(ref player.ControlFlags, fields, HasData.HasControlFlags, ControlFlagsBits);

        bitStream.ReadIfHasFlags(ref player.Rings, fields, HasData.HasRings, RingsBits);
        bitStream.ReadIfHasFlags(ref player.State, fields, HasData.HasState, StateBits);
        bitStream.ReadIfHasFlags(ref player.LastState, fields, HasData.HasState, StateBits);
        bitStream.ReadIfHasFlags(ref player.Air, fields, HasData.HasAir, AirBits);
        bitStream.ReadIfHasFlags(ref player.Animation, fields, HasData.HasAnimation, AnimationBits);
        bitStream.ReadIfHasFlags(ref player.LastAnimation, fields, HasData.HasAnimation, AnimationBits);
        if (fields.HasAllFlags(HasData.HasMovementFlags)) player.MovementFlags = new MovementFlags().FromStream(ref bitStream);
        if (fields.HasAllFlags(HasData.HasAnalogInput)) player.AnalogXY = new AnalogXY().FromStream(ref bitStream);

        return player;
    }

    /// <summary>
    /// Gets a packet for an individual player given the index of the player.
    /// </summary>
    /// <param name="index">The current player index (0-7).</param>
    public static unsafe UnreliablePacketPlayer FromGame(int index)
    {
        ref var player = ref Player.Players[index];
        return new UnreliablePacketPlayer
        {
            Position = player.Position,
            RotationX = player.Rotation.Y,
            RotationY = player.Rotation.X,
            RotationZ = player.Rotation.Z,
            Velocity = new Vector2(player.Speed, player.VSpeed),
            Rings = (byte?)player.Rings,
            State = (byte?)player.PlayerState,
            LastState = (byte?)player.LastPlayerState,
            Air = (uint?)player.Air,
            TurningAmount = player.TurningAmount,
            LeanAmount = player.DriftAngle,
            ControlFlags = player.PlayerControlFlags.Extract(),
            Animation = (byte?)player.Animation,
            LastAnimation = (byte?)player.LastAnimation,
            MovementFlags = new MovementFlags(player.MovementFlags),
            AnalogXY = Structs.AnalogXY.FromGame((Sewer56.SonicRiders.Structures.Gameplay.Player*)Unsafe.AsPointer(ref player))
        };
    }

    /// <summary>
    /// Applies the current packet data to a specified player index.
    /// Does not set <see cref="MovementFlags"/> and <see cref="AnalogXY"/>, these have separate caching mechanisms.
    /// </summary>
    /// <param name="index">Individual player index.</param>
    public unsafe void ToGame(in int index)
    {
        ref var player = ref Player.Players[index];
        if (Position.HasValue)
        {
            player.Position = Position.Value;
            player.PositionAlt = Position.Value;
        }

        if (RotationX.HasValue)
        {
            player.Rotation.Y = RotationX.Value;
            player.Rotation.X = RotationY.Value;
            player.Rotation.Z = RotationZ.Value;
        }

        if (Velocity.HasValue)
        {
            player.Speed = Velocity.Value.X;
            player.VSpeed = Velocity.Value.Y;
        }

        if (Rings.HasValue) player.Rings = (int)Rings;
        if (Air.HasValue) player.Air = (int)Air.Value;
        if (TurningAmount.HasValue) player.TurningAmount = TurningAmount.Value;
        if (LeanAmount.HasValue) player.DriftAngleCopy = LeanAmount.Value;
        if (LeanAmount.HasValue) player.DriftAngle = LeanAmount.Value;
        if (ControlFlags.HasValue) player.PlayerControlFlags.SetMinFlags(ControlFlags.Value);
        if (State.HasValue)
        {
            if (!IsCurrentStateBlacklisted(player.LastPlayerState) && IsWhitelisted((PlayerState)LastState.Value))
                player.LastPlayerState = (PlayerState)LastState.Value;

            if (!IsCurrentStateBlacklisted(player.PlayerState) && IsWhitelisted((PlayerState)State.Value))
                player.PlayerState = (PlayerState)State.Value;
        }

        if (Animation.HasValue)
        {
            // Animation playback for running state is a bit weird; so we'll let the game handle it.
            if (player.PlayerState != PlayerState.RunningAfterStart)
            {
                if (Animation.Value != 0)
                    player.Animation = (CharacterAnimation)Animation.Value;

                if (LastAnimation.Value != 0)
                    player.LastAnimation = (CharacterAnimation)LastAnimation.Value;
            }
        }

        // TODO: Find real solutions for these hacks.
        // Hack for Misc Bugs
        // - Sometimes rotation value not updated after certain state transitions.
        // - Sometimes repeated attacks do not work.
        player.MaybeAttackLastState = PlayerState.None;

        // Game removes floor flag 
        if (player.PlayerState == PlayerState.Running || player.PlayerState == PlayerState.RunningAfterStart)
        {
            player.PlayerControlFlags |= PlayerControlFlags.IsFloored;
        }

        // TODO: The flag below can cause the game to crash for unexpected reasons; we're removing it for now.
        player.PlayerControlFlags &= ~PlayerControlFlags.TurbulenceHairpinTurnSymbol;
    }

    public bool IsDefault() => this.Equals(new UnreliablePacketPlayer());
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
}
