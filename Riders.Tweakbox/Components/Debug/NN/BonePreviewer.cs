using System;
using System.Collections.Generic;
using System.Numerics;
using DearImguiSharp;
using Riders.Tweakbox.Misc;
using Riders.Tweakbox.Misc.Extensions;
using Sewer56.SonicRiders.Utility.Math;
using static Riders.Tweakbox.Components.Debug.NN.Native.MEM_PROTECTION;
using Constants = Sewer56.Imgui.Misc.Constants;
using Reflection = Sewer56.Imgui.Controls.Reflection;

namespace Riders.Tweakbox.Components.Debug.NN;

public unsafe class BonePreviewer : ComponentBase
{
    public override string Name { get; set; } = "Bone Previewer";

    public int Address;

    public const int BoneCount = 71;

    public override void Render()
    {
        int step = 1;
        if (ImGui.Begin(Name, ref IsEnabled(), 0))
        {
            fixed (int* addressPtr = &Address)
            {
                ImGui.InputScalar("Address: ", (int)ImGuiDataType.ImGuiDataTypeU32, (IntPtr)(addressPtr), (IntPtr)(&step), IntPtr.Zero, "%08X", (int)(ImGuiInputTextFlags.ImGuiInputTextFlagsCharsHexadecimal));

                if (!IsBadReadPtr((IntPtr)Address))
                    RenderMenu();
            }
        }

        ImGui.End();
    }

    private void RenderMenu()
    {
        var bones    = (Bone*) Address;
        int uiIndex  = 0;
        var matrices = GetMatricesForEachBoneViaChildren(bones);

        bool compToMatrix = ImGui.Button("Component To Matrix", Constants.ButtonSize);
        ImGui.SameLine(0, Constants.Spacing);
        bool matrixToComp = ImGui.Button("Matrix To Component", Constants.ButtonSize);

        // Render Bone UI
        for (int x = 0; x < BoneCount; x++)
        {
            var bonePtr = bones + x;
            RenderBone($"Bone [{x}]", bonePtr, bones, x, matrices, ref uiIndex);

            // Component to Matrix
            if (compToMatrix)
            {
                var customMatrix = Combine(GetMatricesForBoneViaParent(bonePtr, bones));
                bonePtr->Matrix = customMatrix;
            }

            // Matrix to component.
            if (matrixToComp)
            {
                if (bonePtr->ParentIndex == -1)
                {
                    // Not sure what to do for root, will use identity.
                    bonePtr->Matrix = Matrix4x4.Identity;
                }
                else
                {
                    // Remove Parent 
                    // Parent * Child = Result
                    // AB = C
                    // (A^-1)AB = A^-1(C)
                    // IB = A^-1(C)
                    // B = A^-1(C)

                    var parent = bones + bonePtr->ParentIndex;
                    Matrix4x4.Invert(parent->Matrix, out var inverseParent);
                    var childOriginal = inverseParent * bonePtr->Matrix;
                    Matrix4x4.Invert(childOriginal, out childOriginal);

                    // Apply properties
                    Matrix4x4.Decompose(childOriginal, out var scale, out Quaternion rotation, out Vector3 translation);
                    bonePtr->Scale = scale;
                    bonePtr->Position = new Vector3(translation.X, translation.Y, translation.Z);

                    CreateYawPitchRollXZY(rotation, out Vector3 rotationVector);

                    // Based on game code
                    if ((bonePtr->BoneFlags & 0x1C000) != 0)
                    {
                        rotationVector.Y = 0;
                        rotationVector.Z = 0;
                    }

                    // This is a hack that fixes rotations in certain bones.
                    if ((bonePtr->BoneFlags & 0x1C0000) == 0)
                    {
                        var secondByte = (byte*)(&bonePtr->BoneFlags);
                        if (secondByte[1] == 0x01)
                            secondByte[1] = 0x04;
                    }

                    if (float.IsNaN(rotationVector.X))
                        rotationVector.X = 0;

                    if (float.IsNaN(rotationVector.Y))
                        rotationVector.Y = 0;

                    if (float.IsNaN(rotationVector.Z))
                        rotationVector.Z = 0;

                    bonePtr->Rotation = VectorExtensions.RadiansToBamsInt(rotationVector);
                }
            }
        }
    }

    public static void CreateYawPitchRollXZY(Quaternion r, out Vector3 rotation)
    {
        rotation = new Vector3();
        rotation.Y = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
        rotation.X = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
        rotation.Z = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
    }

    private void RenderBone(string label, Bone* bonePtr, Bone* bones, int index, Matrix4x4[] boneMatrices, ref int uiIndex)
    {
        //var customMatrix = boneMatrices[index];
        if (ImGui.TreeNodeStr(label))
        {
            RenderBoneInternal(label, bonePtr, bones, index, boneMatrices, ref uiIndex);
            ImGui.TreePop();
        }
    }

    private void RenderBoneInternal(string label, Bone* bonePtr, Bone* bones, int index, Matrix4x4[] boneMatrices,
        ref int uiIndex)
    {
        ImGui.TextWrapped($"Index: {index} | {(nint)(bonePtr):X}");
        ImGuiExtensions.RenderIntAsBytesHex(&bonePtr->BoneFlags, 10, ref uiIndex);
        Reflection.MakeControl(&bonePtr->UsedIndex, nameof(Bone.UsedIndex), 0.1f, -1, BoneCount);
        Reflection.MakeControl(&bonePtr->ParentIndex, nameof(Bone.ParentIndex), 0.1f, -1, BoneCount);

        Reflection.MakeControl(bonePtr->ChildIndices, $"{nameof(Bone.ChildIndices)}[0]", 0.1f, -1, BoneCount);
        Reflection.MakeControl(bonePtr->ChildIndices + 1, $"{nameof(Bone.ChildIndices)}[1]", 0.1f, -1, BoneCount);

        ImGui.Custom.DragFloat3(nameof(Bone.Position), &bonePtr->Position, 0.1f);
        ImGui.Custom.DragInt3(nameof(Bone.Rotation), &bonePtr->Rotation.X, 10f, int.MinValue, int.MaxValue, null, (int)ImGuiSliderFlags.ImGuiSliderFlagsNone);
        ImGui.Custom.DragFloat3(nameof(Bone.Scale), &bonePtr->Scale, 0.1f);

        ImGui.Custom.DragFloat4($"{nameof(Bone.Matrix)} [0]", &bonePtr->Matrix.M11, 0.1f);
        ImGui.Custom.DragFloat4($"{nameof(Bone.Matrix)} [1]", &bonePtr->Matrix.M21, 0.1f);
        ImGui.Custom.DragFloat4($"{nameof(Bone.Matrix)} [2]", &bonePtr->Matrix.M31, 0.1f);
        ImGui.Custom.DragFloat4($"{nameof(Bone.Matrix)} [3]", &bonePtr->Matrix.M41, 0.1f);

        ImGui.Custom.DragFloat4($"{nameof(Bone.RenderData)} [0]", &bonePtr->RenderData[0], 0.1f);
        ImGui.Custom.DragFloat4($"{nameof(Bone.RenderData)} [4]", &bonePtr->RenderData[4], 0.1f);

        // Matrices
        if (bonePtr->ParentIndex != -1)
        {
            var customMatrix     = Combine(GetMatricesForBoneViaParent(bonePtr, bones));
            var vectorThisCustom = Vector3.Transform(Vector3.Zero, customMatrix);
            var vectorThisCustomTopDown = Vector3.Transform(Vector3.Zero, boneMatrices[index]);
            var vectorThis   = Vector3.Transform(Vector3.Zero, bonePtr->Matrix);
            var vectorParent = Vector3.Transform(Vector3.Zero, (bones + bonePtr->ParentIndex)->Matrix);

            ImGui.TextWrapped($"Transformed Vector (Custom): {ToString(vectorThisCustom)}");
            ImGui.TextWrapped($"Transformed Vector (Custom Top Down): {ToString(vectorThisCustomTopDown)}");
            ImGui.TextWrapped($"Transformed Vector (Matrix): {ToString(vectorThis)}");
            ImGui.TextWrapped($"Parent Vector: {ToString(vectorParent)}");
            ImGui.TextWrapped($"Vector Difference: {ToString(vectorThis - vectorParent)}");
        }

        var childOneIndex = *(bonePtr->ChildIndices);
        var childTwoIndex = *(bonePtr->ChildIndices + 1);
        var childOneLabel = label + ", Child [0]";
        var childTwoLabel = label + ", Child [1]";
        if (childOneIndex != -1 && ImGui.TreeNodeStr(childOneLabel))
        {
            RenderBoneInternal(childOneLabel, bones + childOneIndex, bones, childOneIndex, boneMatrices, ref uiIndex);
            ImGui.TreePop();
        }

        if (childTwoIndex != -1 && ImGui.TreeNodeStr(childTwoLabel))
        {
            RenderBoneInternal(childOneLabel, bones + childTwoIndex, bones, childTwoIndex, boneMatrices, ref uiIndex);
            ImGui.TreePop();
        }
    }

    public string ToString(Vector3 vector)
    {
        return $"({vector.X:0.000}, {vector.Y:0.000}, {vector.Z:0.000})";
    }

    public static Matrix4x4 Combine(Stack<Matrix4x4> matrices)
    {
        var result = Matrix4x4.Identity;
        while (matrices.TryPop(out var poppedMatrix))
        {
            //Matrix4x4.Invert(poppedMatrix, out poppedMatrix);
            result = result * poppedMatrix;
        }

        return result;
    }
    
    private static Stack<Matrix4x4> GetMatricesForBoneViaParent(Bone* bonePtr, Bone* bones)
    {
        var matrixList = new Stack<Matrix4x4>();

        while (true)
        {
            matrixList.Push(GetLocalMatrixForBone(bonePtr));
            if (bonePtr->ParentIndex < 0)
                break;

            bonePtr = &bones[bonePtr->ParentIndex];
        }

        return matrixList;
    }

    private static Matrix4x4 GetLocalMatrixForBone(Bone* bone)
    {
        if ((bone->BoneFlags & 0x100000) == 0)
        {
            // Hack for odd bones.
            //bone->Position.X = 0;
            //bone->Position.Y = 0;
            //bone->Position.Z = 0;
        }

        var position = bone->Position;
        var rotationRadians    = bone->Rotation.BamsToRadians();
        var rotationQuaternion = Quaternion.Identity;
        if ((bone->BoneFlags & 2) != 0)
        {
            // No change.
        }
        else
        {
            // Based on game code
            if ((bone->BoneFlags & 0x1C000) != 0)
            {
                rotationRadians.Y = 0;
                rotationRadians.Z = 0;
            }

            // Based on game code.
            switch (bone->BoneFlags & 0xF00)
            {
                case 0x100:
                    rotationQuaternion = EulerToQuat(rotationRadians.X, rotationRadians.Y, rotationRadians.Z, AxisOrder.XZY);
                    break;
                case 0x400:
                    rotationQuaternion = EulerToQuat(rotationRadians.X, rotationRadians.Y, rotationRadians.Z, AxisOrder.ZXY);
                    break;
                default:
                    rotationQuaternion = EulerToQuat(rotationRadians.X, rotationRadians.Y, rotationRadians.Z, AxisOrder.XYZ);
                    break;
            }

            rotationQuaternion = Quaternion.Inverse(rotationQuaternion);
        }

        var result = Matrix4x4.CreateTranslation(position) * Matrix4x4.CreateFromQuaternion(rotationQuaternion) * Matrix4x4.CreateScale(bone->Scale);
        Matrix4x4.Invert(result, out result);

        return result;
    }

    private enum AxisOrder
    {
        XYZ, XZY, YXZ, YZX, ZXY, ZYX, None
    }

    private static Quaternion EulerToQuat(float yaw, float pitch, float roll, AxisOrder rotationOrder)
    {
        var xRot = Quaternion.CreateFromAxisAngle(-Vector3.UnitX, yaw);
        var yRot = Quaternion.CreateFromAxisAngle(-Vector3.UnitY, pitch);
        var zRot = Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, roll);
        
        switch (rotationOrder)
        {
            case AxisOrder.XYZ: return (xRot * yRot) * zRot;
            case AxisOrder.XZY: return (xRot * zRot) * yRot;
            case AxisOrder.YXZ: return (yRot * xRot) * zRot;
            case AxisOrder.YZX: return (yRot * zRot) * xRot;
            case AxisOrder.ZXY: return (zRot * xRot) * yRot;
            case AxisOrder.ZYX: return (zRot * yRot) * xRot;
        }

        return Quaternion.Identity;
    }

    private static Matrix4x4[] GetMatricesForEachBoneViaChildren(Bone* boneBasePtr)
    {
        var results = new Matrix4x4[BoneCount];
        GetMatricesForEachBone_Recursive(boneBasePtr, 0, Matrix4x4.Identity, results);
        return results;
    }

    private static void GetMatricesForEachBone_Recursive(Bone* boneBasePtr, int boneIndex, Matrix4x4 parentTransform, Matrix4x4[] results)
    {
        var bonePtr = boneBasePtr + boneIndex;
        var globalTransform = parentTransform * GetLocalMatrixForBone(bonePtr);
        results[boneIndex] = globalTransform;

        var childOneIndex = *(bonePtr->ChildIndices);
        var childTwoIndex = *(bonePtr->ChildIndices + 1);
        if (childOneIndex >= 0)
            GetMatricesForEachBone_Recursive(boneBasePtr, childOneIndex, globalTransform, results);

        if (childTwoIndex >= 0)
            GetMatricesForEachBone_Recursive(boneBasePtr, childTwoIndex, globalTransform, results);
    }

    public struct Bone
    {
        public int BoneFlags;
        public short UsedIndex;
        public short ParentIndex;

        public fixed short ChildIndices[2];

        public Vector3 Position;
        public Vector3Int Rotation;

        public Vector3 Scale;
        public Matrix4x4 Matrix;

        public fixed float RenderData[8];
    }

    /// <summary>
    /// Checks whether a given address in memory can be read from.
    /// </summary>
    /// <param name="address">The target address.</param>
    internal static unsafe bool IsBadReadPtr(IntPtr address)
    {
        const Native.MEM_PROTECTION canReadMask = (PAGE_READONLY | PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READ | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY);
        Native.MEMORY_BASIC_INFORMATION mbi = default;

        if (Native.VirtualQuery(address, ref mbi, (UIntPtr)sizeof(Native.MEMORY_BASIC_INFORMATION)) != IntPtr.Zero)
        {
            bool badPtr = (mbi.Protect & canReadMask) == 0;

            // Check the page is not a guard page
            if ((mbi.Protect & (PAGE_GUARD | PAGE_NOACCESS)) > 0)
                badPtr = true;

            return badPtr;
        }

        return true;
    }
}