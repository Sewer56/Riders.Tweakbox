using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Riders.Tweakbox.Configs.Misc;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.API;
using Player = Sewer56.SonicRiders.Structures.Gameplay.Player;

namespace Riders.Tweakbox.Controllers;

public unsafe class RailController : IController
{
    public CustomRailConfiguration Configuration { get; private set; } = new CustomRailConfiguration();

    public void* LastRailFilePtr { get; private set; }

    private FramePacingController _framePacingController = IoC.GetSingleton<FramePacingController>();

    public RailController()
    {
        EventController.SetRailInitialSpeedCap += SetRailInitialSpeedCap;
        EventController.SetRailSpeedCap += SetRailSpeedCap;
        EventController.AfterSetRailSpeedCap += AfterSetRailSpeedCap;
        _framePacingController.OnEndFrame += OnEndFrame;
    }

    private void OnEndFrame()
    {
        var railPtr = *State.CurrentRailFile;
        if (railPtr == LastRailFilePtr) 
            return;

        // File changed.
        if (railPtr == (void*)0)
        {
            Configuration.Data = new CustomRailConfiguration.Internal();
        }
        else
        {
            // Load new file.
            LastRailFilePtr    = *State.CurrentRailFile;
            Configuration.Data = new CustomRailConfiguration.Internal(InMemorySplineFile.CurrentRail.NumSplines);
        }
    }

    /// <summary>
    /// Teleports a given player to a given rail.
    /// </summary>
    /// <param name="railIndex">Index of the rail to teleport to.</param>
    /// <param name="player">The player to teleport.</param>
    public void TeleportToRail(int railIndex, Player* player)
    {
        ref var playerRef = ref Unsafe.AsRef<Player>(player);
        var rail = InMemorySplineFile.CurrentRail.GetRail(railIndex);
        var vertex = *rail->VertexPtr;
        playerRef.Teleport(vertex.ToVector3(), default);
    }

    private void SetRailInitialSpeedCap(ref float value, Player* player, int numFramesOnRail)
    {
        if (!TryGetRail(player, out var rail))
            return;

        // Apply rail data.
        value = rail.SpeedCapInitial;
    }

    private void SetRailSpeedCap(ref float value, Player* player, int numFramesOnRail)
    {
        if (!TryGetRail(player, out var rail)) 
            return;

        // Apply rail data.
        value = rail.CalculateSpeed(numFramesOnRail);
    }

    private void AfterSetRailSpeedCap(ref float value, Player* player, int numFramesOnRail)
    {
        if (!TryGetRail(player, out var rail))
            return;

        // Fix: If game sets acceleration to 0 due to over speed
        // cap but we're below speed cap due to it increasing, set speed manually.
        if (Math.Abs(player->Acceleration) < float.Epsilon && player->Speed < player->SpeedCap)
            player->Speed = player->SpeedCap;
    }

    private bool TryGetRail(Player* player, out CustomRailConfiguration.RailEntry rail)
    {
        var railIndex = player->LastRailIndex;
        rail = default;

        // Bounds check for known rail.
        if (railIndex < 0 || Configuration.Data.Rails.Count <= railIndex)
            return false;

        // Get rail data.
        rail = Configuration.Data.Rails[railIndex];
        return rail.IsEnabled;
    }
}

// TODO: Move this code to Sewer56.SonicRiders after file is properly documented/reverse engineered.
public unsafe struct InMemorySplineFile
{
    /// <summary>
    /// Gets the current in memory layout.
    /// </summary>
    public static InMemorySplineFile CurrentRail => new InMemorySplineFile(*State.CurrentRailFile);

    /// <summary>
    /// Header belonging to the file.
    /// </summary>
    public SplineFileHeader* Header;

    /// <summary>
    /// Number of splines in this file.
    /// </summary>
    public int NumSplines => Header->NumSplines;

    public InMemorySplineFile(void* filePtr)
    {
        Header = (SplineFileHeader*) filePtr;
    }

    /// <summary>
    /// Gets the rail with a specified index.
    /// </summary>
    /// <returns></returns>
    public RailHeader* GetRail(int index)
    {
        var railOffsetPtr = (nint**)(Header + 1);
        return (RailHeader*)(*(railOffsetPtr + index));
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SplineFileHeader
    {
        public byte _pad_0;
        public byte SplineType;
        public int Magic;
        private ushort _pad_6;
        public int NumSplines;
    };

    public struct BoundingBox
    {
        // TODO: Confirm this is correct. Just a guess.
        public float MinX;
        public float MinZ;
        public float MaxX;
        public float MaxZ;
    };

    public struct RailHeader
    {
        public int BoundingBoxPtr;
        public Vector4* VertexPtr;
        public int NormalsPtr;

        public BoundingBox Box;
        public int NumVerts;

        public int Unknown0;
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
    };
}