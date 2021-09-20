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

public class GameModifiers : IReliableMessage
{
    public bool DisableTornadoes;
    public bool DisableAttacks;
    public bool AlwaysTurbulence;
    public bool NoTurbulence;
    public bool DisableSmallTurbulence = true;

    public bool BerserkerTurbulenceFix = true;
    public bool NoScreenpeek;
    public bool NormalizedBoostDurations = true;

    public ushort DisableAttackDurationFrames;
    public bool OverridePitAirGain;
    public float PitAirGainMultiplier = 1.0f;

    public ReplaceItemSettings ReplaceRing100Settings = new ReplaceItemSettings();
    public ReplaceItemSettings ReplaceMaxAirSettings = new ReplaceItemSettings();
    public SlipstreamModifierSettings Slipstream = new SlipstreamModifierSettings();
    public RingLossBehaviour DeathRingLoss = new RingLossBehaviour()
    {
        Enabled = true,
        RingLossBefore = 0,
        RingLossPercentage = 0
    };

    public RingLossBehaviour HitRingLoss = new RingLossBehaviour();
    public ItemBoxProperties ItemBoxProperties = new ItemBoxProperties();

    public bool IgnoreTurbulenceOnToggle = true;
    
    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public MessageType GetMessageType() => MessageType.ServerGameModifiers;

    /// <summary>
    /// Gets the disable attack duration as a timespan.
    /// </summary>
    public TimeSpan GetDisableAttackDuration() => TimeSpan.FromMilliseconds((1000.0 / 60.0) * DisableAttackDurationFrames);

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        DisableTornadoes = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        DisableAttacks = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        AlwaysTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        NoTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        DisableSmallTurbulence = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));

        BerserkerTurbulenceFix = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        NoScreenpeek = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        NormalizedBoostDurations = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));

        DisableAttackDurationFrames = bitStream.Read<ushort>();
        OverridePitAirGain   = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        PitAirGainMultiplier = bitStream.ReadGeneric<float>();
        
        ReplaceRing100Settings.FromStream(ref bitStream);
        ReplaceMaxAirSettings.FromStream(ref bitStream);
        Slipstream.FromStream(ref bitStream);
        DeathRingLoss.FromStream(ref bitStream);
        HitRingLoss.FromStream(ref bitStream);
        ItemBoxProperties.FromStream(ref bitStream);

        IgnoreTurbulenceOnToggle = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Convert.ToByte(DisableTornadoes), 1);
        bitStream.Write(Convert.ToByte(DisableAttacks), 1);
        bitStream.Write(Convert.ToByte(AlwaysTurbulence), 1);
        bitStream.Write(Convert.ToByte(NoTurbulence), 1);
        bitStream.Write(Convert.ToByte(DisableSmallTurbulence), 1);

        bitStream.Write(Convert.ToByte(BerserkerTurbulenceFix), 1);
        bitStream.Write(Convert.ToByte(NoScreenpeek), 1);
        bitStream.Write(Convert.ToByte(NormalizedBoostDurations), 1);

        bitStream.Write<ushort>(DisableAttackDurationFrames);
        bitStream.Write(Convert.ToByte(OverridePitAirGain), 1);
        bitStream.WriteGeneric(PitAirGainMultiplier);
        
        ReplaceRing100Settings.ToStream(ref bitStream);
        ReplaceMaxAirSettings.ToStream(ref bitStream);
        Slipstream.ToStream(ref bitStream);
        DeathRingLoss.ToStream(ref bitStream);
        HitRingLoss.ToStream(ref bitStream);
        ItemBoxProperties.ToStream(ref bitStream);

        bitStream.Write(Convert.ToByte(IgnoreTurbulenceOnToggle), 1);
    }
}

public class ReplaceItemSettings
{
    public bool Enabled;
    public ItemBoxAttribute Replacement;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        Enabled = Convert.ToBoolean(bitStream.ReadGeneric<byte>(1));
        Replacement = bitStream.ReadGeneric<ItemBoxAttribute>(EnumNumBits<ItemBoxAttribute>.Number);
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write(Convert.ToByte(Enabled), 1);
        bitStream.WriteGeneric(Replacement, EnumNumBits<ItemBoxAttribute>.Number);
    }
}

public class ItemBoxProperties
{
    /// <summary>
    /// Amount of frames until an itembox respawns.
    /// </summary>
    public int RespawnTimerFrames = 180;

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        RespawnTimerFrames = bitStream.Read<int>();
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.Write<int>(RespawnTimerFrames);
    }
}

// TODO: Optimize Message Size of This Struct
public class SlipstreamModifierSettings
{
    /// <summary>
    /// True to enable slipstream.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// Maximum slipstream angle.
    /// Slipstream strength is scaled depending on the rotation.
    /// </summary>
    public float SlipstreamMaxAngle = 25.000f;

    /// <summary>
    /// Max multiplier of player speed every frame.
    /// The actual slip power is based on how perfectly you follow; with this being the upper bound.
    /// </summary>
    public float SlipstreamMaxStrength = 0.0082f;

    /// <summary>
    /// Max distance for slipstream.
    /// The closer the player, the more slipstream is applied.
    /// At the distance here, minimum slipstream is applied.
    /// </summary>
    public float SlipstreamMaxDistance = 100.000f;

    /// <summary>
    /// Easing functions allow you to apply custom mathematical formulas to your slipstream.
    /// </summary>
    public EasingSetting EasingSetting = EasingSetting.CircleEase;

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
public class RingLossBehaviour
{
    public const int MaxRingLoss = 100;
    public const int MinRingLoss = 0;

    public const float MaxRingLossPercent = 100;
    public const float MinRingLossPercent = 0;

    private static int RingLossNumBits = Misc.Utilities.GetMinimumNumberOfBits(MaxRingLoss);

    /// <summary>
    /// True if this override is enabled, else false.
    /// </summary>
    public bool Enabled = true;

    /// <summary>
    /// Flat amount of rings lost.
    /// This is applied after the percentage cut.
    /// </summary>
    public byte RingLossAfter = 10;

    /// <summary>
    /// Flat amount of rings lost.
    /// This is applied before the percentage cut.
    /// </summary>
    public byte RingLossBefore = 0;

    /// <summary>
    /// Percentage of rings lost, the result is rounded down to nearest integer.
    /// </summary>
    public float RingLossPercentage = 20f;

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