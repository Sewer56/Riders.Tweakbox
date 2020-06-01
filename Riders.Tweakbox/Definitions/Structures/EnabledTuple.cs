namespace Riders.Tweakbox.Definitions.Structures
{
    public class EnabledTuple<T>
    {
        public bool Enabled;
        public T Value;
        
        public EnabledTuple(bool enabled, T value)
        {
            Enabled = enabled;
            Value = value;
        }

        public EnabledTuple(T value)
        {
            Enabled = true;
            Value = value;
        }
    }
}
