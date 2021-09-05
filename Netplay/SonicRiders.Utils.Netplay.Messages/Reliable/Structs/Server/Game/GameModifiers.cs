using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using EnumsNET;
using Riders.Netplay.Messages.Helpers;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using Sewer56.SonicRiders;
using Sewer56.SonicRiders.Parser.Layout.Objects.ItemBox;
using Riders.Netplay.Messages.Misc;
using static Riders.Netplay.Messages.Reliable.Structs.Server.Game.SlipstreamModifierSettings;

namespace Riders.Netplay.Messages.Reliable.Structs.Server.Game;

public struct GameModifiers : IReliableMessage
{
    public bool DisableTornadoes;
    public bool DisableAttacks;
    public bool AlwaysTurbulence;
    public bool NoTurbulence;
    public bool DisableSmallTurbulence;
    public bool NoScreenpeek;

    public bool ReplaceRing100Box;
    public ItemBoxAttribute Ring100Replacement;

    public bool ReplaceAirMaxBox;
    public ItemBoxAttribute AirMaxReplacement;

    public SlipstreamModifierSettings Slipstream;
    public RingLossBehaviour DeathRingLoss;
    public RingLossBehaviour HitRingLoss;

    /// <summary>
    /// Creates a <see cref="GameModifiers"/> struct with the default parameters.
    /// </summary>
    public static GameModifiers CreateDefault()
    {
        var result = new GameModifiers();
        result.Slipstream = SlipstreamModifierSettings.CreateDefault();
        result.DeathRingLoss = new RingLossBehaviour()
        {
            Enabled = true,
            RingLossBefore = 0,
            RingLossPercentage = 0
        };

        result.HitRingLoss = RingLossBehaviour.CreateDefault();
        return result;
    }

    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public MessageType GetMessageType() => MessageType.ServerGameModifiers;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        DisableTornadoes = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        DisableAttacks = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        AlwaysTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        NoTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        DisableSmallTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        NoScreenpeek = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));

        ReplaceRing100Box = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        Ring100Replacement = bitStream.ReadGeneric<ItemBoxAttribute>(EnumNumBits<ItemBoxAttribute>.Number);
        ReplaceAirMaxBox = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        AirMaxReplacement = bitStream.ReadGeneric<ItemBoxAttribute>(EnumNumBits<ItemBoxAttribute>.Number);

        Slipstream.FromStream(ref bitStream);
        DeathRingLoss.FromStream(ref bitStream);
        HitRingLoss.FromStream(ref bitStream);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Convert.ToByte(DisableTornadoes), 1);
        bitStream.Write(Convert.ToByte(DisableAttacks), 1);
        bitStream.Write(Convert.ToByte(AlwaysTurbulence), 1);
        bitStream.Write(Convert.ToByte(NoTurbulence), 1);
        bitStream.Write(Convert.ToByte(DisableSmallTurbulence), 1);
        bitStream.Write(Convert.ToByte(NoScreenpeek), 1);

        bitStream.WriteGeneric(ReplaceRing100Box, 1);
        bitStream.WriteGeneric(Ring100Replacement, EnumNumBits<ItemBoxAttribute>.Number);
        bitStream.WriteGeneric(ReplaceAirMaxBox, 1);
        bitStream.WriteGeneric(AirMaxReplacement, EnumNumBits<ItemBoxAttribute>.Number);

        Slipstream.ToStream(ref bitStream);
        DeathRingLoss.ToStream(ref bitStream);
        HitRingLoss.ToStream(ref bitStream);
    }
}

// TODO: Optimize Message Size of This Struct
public struct SlipstreamModifierSettings
{
    /// <summary>
    /// True to enable slipstream.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Maximum slipstream angle.
    /// Slipstream strength is scaled depending on the rotation.
    /// </summary>
    public float SlipstreamMaxAngle;

    /// <summary>
    /// Max multiplier of player speed every frame.
    /// The actual slip power is based on how perfectly you follow; with this being the upper bound.
    /// </summary>
    public float SlipstreamMaxStrength;

    /// <summary>
    /// Max distance for slipstream.
    /// The closer the player, the more slipstream is applied.
    /// At the distance here, minimum slipstream is applied.
    /// </summary>
    public float SlipstreamMaxDistance;

    /// <summary>
    /// Easing functions allow you to apply custom mathematical formulas to your slipstream.
    /// </summary>
    public EasingSetting EasingSetting;

    public static SlipstreamModifierSettings CreateDefault()
    {
        return new SlipstreamModifierSettings()
        {
            Enabled = true,
            SlipstreamMaxAngle = 24.00f,
            SlipstreamMaxStrength = 0.0072f,
            SlipstreamMaxDistance = 100.000f,
            EasingSetting = EasingSetting.CircleEase
        };
    }

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Enabled = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        SlipstreamMaxAngle = bitStream.ReadGeneric<float>();
        SlipstreamMaxStrength = bitStream.ReadGeneric<float>();
        SlipstreamMaxDistance = bitStream.ReadGeneric<float>();
        EasingSetting = bitStream.ReadGeneric<EasingSetting>(EnumNumBits<EasingSetting>.Number);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Convert.ToByte(Enabled), 1);
        bitStream.WriteGeneric<float>(SlipstreamMaxAngle);
        bitStream.WriteGeneric<float>(SlipstreamMaxStrength);
        bitStream.WriteGeneric<float>(SlipstreamMaxDistance);
        bitStream.WriteGeneric<EasingSetting>(EasingSetting, EnumNumBits<EasingSetting>.Number);
    }
}

public enum EasingSetting
{
    Linear,
    SineEase,
    QuadraticEase,
    CubicEase,
    CircleEase,
    QuarticEase,
    ExponentialEase
}

// Defines how ring loss is handled.
public struct RingLossBehaviour
{
    public const int MaxRingLoss = 100;
    public const int MinRingLoss = 0;

    public const float MaxRingLossPercent = 100;
    public const float MinRingLossPercent = 0;

    private static int RingLossNumBits = Misc.Utilities.GetMinimumNumberOfBits(MaxRingLoss);

    /// <summary>
    /// True if this override is enabled, else false.
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// Flat amount of rings lost.
    /// This is applied after the percentage cut.
    /// </summary>
    public byte RingLossAfter;

    /// <summary>
    /// Flat amount of rings lost.
    /// This is applied before the percentage cut.
    /// </summary>
    public byte RingLossBefore;

    /// <summary>
    /// Percentage of rings lost, the result is rounded down to nearest integer.
    /// </summary>
    public float RingLossPercentage;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Enabled = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        RingLossBefore = bitStream.Read<byte>(RingLossNumBits);
        RingLossAfter = bitStream.Read<byte>(RingLossNumBits);
        RingLossPercentage = bitStream.ReadGeneric<float>();
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Convert.ToByte(Enabled), 1);
        bitStream.Write(RingLossBefore, RingLossNumBits);
        bitStream.Write(RingLossAfter, RingLossNumBits);
        bitStream.WriteGeneric<float>(RingLossPercentage);
    }

    public static RingLossBehaviour CreateDefault()
    {
        return new RingLossBehaviour()
        {
            Enabled = true,
            RingLossBefore = 0,
            RingLossAfter = 10,
            RingLossPercentage = 20f
        };
    }
}

public static class GameModifiersExtensions
{
    private static EasingFunctionBase[] EasingFunctions;

    public static EasingFunctionBase GetEasingFunction(this EasingSetting setting) => EasingFunctions[(int)setting];

    static GameModifiersExtensions()
    {
        EasingFunctions = new EasingFunctionBase[(int)Enums.GetMembers<EasingSetting>().Last().Value + 1];
        EasingFunctions[(int)EasingSetting.Linear] = new LinearEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.SineEase] = new SineEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.QuadraticEase] = new QuadraticEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.CubicEase] = new CubicEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.CircleEase] = new CircleEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.QuarticEase] = new QuarticEase() { EasingMode = EasingMode.EaseIn };
        EasingFunctions[(int)EasingSetting.ExponentialEase] = new ExponentialEase() { EasingMode = EasingMode.EaseIn };
    }
}

public class LinearEase : EasingFunctionBase
{
    protected override Freezable CreateInstanceCore() => new LinearEase();

    protected override double EaseInCore(double normalizedTime) => normalizedTime;
}