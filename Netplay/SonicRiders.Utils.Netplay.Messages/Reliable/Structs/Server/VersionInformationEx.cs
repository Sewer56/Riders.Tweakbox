﻿using System;
using System.Linq;
using Riders.Netplay.Messages.Helpers;
using Riders.Netplay.Messages.Misc.BitStream;
using Sewer56.BitStream;
using Sewer56.BitStream.Interfaces;
using StructLinq;

namespace Riders.Netplay.Messages.Reliable.Structs.Server;

public struct VersionInformationEx : IReliableMessage
{
    /// <summary>
    /// The current game mode.
    /// </summary>
    public RaceMode GameMode;

    /// <summary>
    /// Lists the mods loaded in by the client.
    /// </summary>
    public string[] Mods;

    /// <inheritdoc />
    public MessageType GetMessageType() => MessageType.VersionEx;

    /// <inheritdoc />
    public void Dispose() { }

    /// <summary>
    /// Verifies the current version (host) against the other (client) version.
    /// </summary>
    /// <param name="other">The other (client) version.</param>
    /// <param name="errors">List of newline separated errors.</param>
    public bool Verify(VersionInformationEx other, out string errors)
    {
        bool areEqual = true;
        errors = "";

        if (other.GameMode != GameMode)
        {
            areEqual = false;
            errors += $"Game Mode does not match | Host: {GameMode}, Client: {other.GameMode}";
        }

        var modsMap = Mods.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in other.Mods)
        {
            if (!modsMap.Contains(mod))
            {
                errors += $"\nMod is missing: {mod}";
                areEqual = false;
            }
        }

        return areEqual;
    }

    /// <inheritdoc />
    public void FromStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        GameMode = bitStream.ReadGeneric<RaceMode>(EnumNumBits<RaceMode>.Number);
        Mods = bitStream.ReadStringArray();
    }

    /// <inheritdoc />
    public void ToStream<TByteStream>(ref BitStream<TByteStream> bitStream) where TByteStream : IByteStream
    {
        bitStream.WriteGeneric(GameMode, EnumNumBits<RaceMode>.Number);
        bitStream.WriteStringArray(Mods);
    }

    /// <summary>
    /// Race modes corresponding to the game's own internal modes.
    /// </summary>
    public enum RaceMode : byte
    {
        FreeRace = 0,
        TimeTrial = 1,
        GrandPrix = 2,
        StoryMode = 3,
        RaceStage = 4,
        BattleStage = 5,
        MissionMode = 6,
        TagMode = 7,
        Demo = 8,
    }
}
