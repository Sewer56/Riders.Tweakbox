namespace Riders.Tweakbox.Shared.Structs
{
    public struct DashPanelProperties
    {
        /// <summary>
        /// Mode describing how the dash panels operate.
        /// </summary>
        public DashPanelMode Mode;

        /// <summary>
        /// The amount of speed set when in fixed mode.
        /// </summary>
        public float FixedSpeed;

        /// <summary>
        /// The amount of speed set when in additive mode.
        /// </summary>
        public float AdditiveSpeed;

        /// <summary>
        /// The decimal by which to increase player's current speed.
        /// 1.2 indicates a speed increase of 20%.
        /// </summary>
        public float MultiplicativeSpeed;

        /// <summary>
        /// Minimum speed to allow in multiplicative mode.
        /// </summary>
        public float MultiplicativeMinSpeed;

        public static DashPanelProperties Default()
        {
            return new DashPanelProperties()
            {
                Mode = DashPanelMode.Vanilla,
                FixedSpeed = 1.075000f,
                AdditiveSpeed = 0.10f,
                MultiplicativeSpeed = 0.20f,
                MultiplicativeMinSpeed = 0.95f,
            };
        }
    }

    /// <summary>
    /// Mode for custom dash panel behaviour.
    /// </summary>
    public enum DashPanelMode
    {
        /// <summary>
        /// Fixed speed, vanilla values.
        /// </summary>
        Vanilla,

        /// <summary>
        /// Fixed speed.
        /// </summary>
        Fixed,

        /// <summary>
        /// Add a fixed amount of speed to
        /// </summary>
        Additive,

        /// <summary>
        /// Multiplies the player's current speed.
        /// </summary>
        Multiplicative,

        /// <summary>
        /// Multiplies the player's current speed or sets to a fixed speed.
        /// Whichever of the two will yield greater speed.
        /// </summary>
        MultiplyOrFixed,
    }
}