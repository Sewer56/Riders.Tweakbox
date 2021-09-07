using System;

namespace Riders.Tweakbox.Interfaces.Structs.Enums;

/// <summary>
/// Describes the result of an Assembly Hook.
/// </summary>
public enum QueryResult
{
    /// <summary>
    /// Condition is false, run the "false" code.
    /// </summary>
    False = 0,

    /// <summary>
    /// Condition is true, run the "true" code.
    /// </summary>
    True = 1,

    /// <summary>
    /// Condition is neither true or false, do not override program behaviour.
    /// </summary>
    Indeterminate = 2,
}

public static class QueryResultExtensions
{
    /// <summary>
    /// Tries to convert the result as a true/false boolean.
    /// Returns false if value is out of range (indeterminate).
    /// </summary>
    public static bool TryConvertToBool(this QueryResult? queryResult, out bool result)
    {
        if (queryResult == null)
        {
            result = default;
            return false;
        }
        
        return TryConvertToBool(queryResult.Value, out result);
    }

    /// <summary>
    /// Tries to convert the result as a true/false boolean.
    /// Returns false if value is out of range (indeterminate).
    /// </summary>
    public static bool TryConvertToBool(this QueryResult queryResult, out bool result)
    {
        result = queryResult switch
        {
            QueryResult.False => false,
            QueryResult.True => true,
            _ => default
        };

        return queryResult <= QueryResult.True && queryResult >= 0;
    }
}