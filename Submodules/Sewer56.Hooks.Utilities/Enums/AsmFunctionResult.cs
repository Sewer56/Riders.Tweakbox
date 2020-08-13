namespace Sewer56.Hooks.Utilities.Enums
{
    /// <summary>
    /// Describes the result of an Assembly Hook.
    /// </summary>
    public enum AsmFunctionResult
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
}