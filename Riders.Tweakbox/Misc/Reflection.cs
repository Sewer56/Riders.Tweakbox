using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Reloaded.Memory;
namespace Riders.Tweakbox.Misc;

public class Reflection
{
    /// <summary>
    /// Returns the list of all public instance fields present for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    public static List<string> GetAllInstanceFieldNames(Type type)
    {
        var result = new List<string>();
        foreach (var field in type.GetFields())
        {
            if (field.IsStatic)
                continue;

            result.Add(field.Name);
        }

        return result;
    }

    /// <summary>
    /// Swaps the endianness of a given struct using reflection.
    /// </summary>
    public static unsafe T SwapStructEndianness<T>(T value, int startOffset = 0) where T : unmanaged
    {
        var type = value.GetType();
        var span = new Span<byte>(&value, sizeof(T));
        SwapStructEndianness(type, span, startOffset);
        return value;
    }

    /// <summary>
    /// Swaps the endianness of a given struct using reflection.
    /// </summary>
    public static void SwapStructEndianness(Type type, Span<byte> data, int startOffset = 0)
    {
        foreach (var field in type.GetFields())
        {
            var fieldType = field.FieldType;

            // Don't process static fields.
            if (field.IsStatic)
                continue;

            var fieldOffset = Marshal.OffsetOf(type, field.Name).ToInt32();

            // Handle Enumerations.
            if (fieldType.IsEnum)
                fieldType = Enum.GetUnderlyingType(fieldType);

            // Check for sub-fields to recurse if necessary.
            var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
            var effectiveOffset = startOffset + fieldOffset;

            if (subFields.Length == 0)
                data.Slice(effectiveOffset, Marshal.SizeOf(fieldType)).Reverse();
            else
                SwapStructEndianness(fieldType, data, effectiveOffset);
        }
    }

    /// <summary>
    /// Performs a shallow copy of all matching fields between the first and second object.
    /// </summary>
    /// <param name="firstObject">Object to copy from.</param>
    /// <param name="secondObject">Object to copy to.</param>
    public static void ShallowCopyValues<T1, T2>(ref T1 firstObject, ref T2 secondObject)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        var firstFieldDefinitions = firstObject.GetType().GetFields(flags);
        var secondFieldDefinitions = secondObject.GetType().GetFields(flags);

        object secondObjectBoxed = secondObject;
        foreach (var fieldDefinition in firstFieldDefinitions)
        {
            var matchingFieldDefinition = secondFieldDefinitions.FirstOrDefault(fd => fd.Name == fieldDefinition.Name && fd.FieldType == fieldDefinition.FieldType);
            if (matchingFieldDefinition == null)
                continue;

            var value = fieldDefinition.GetValue(firstObject);
            matchingFieldDefinition.SetValue(secondObjectBoxed, value);
        }

        secondObject = (T2)secondObjectBoxed;
    }

}
