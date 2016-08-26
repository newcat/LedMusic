using System;

namespace LedMusic
{
    [AttributeUsage(AttributeTargets.Property)]
    class AnimatableAttribute : Attribute
    {

        public bool UpdateAtRuntime { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }

        public AnimatableAttribute(double minValue, double maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            UpdateAtRuntime = false;
        }

        public AnimatableAttribute(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            UpdateAtRuntime = false;
        }

        public AnimatableAttribute()
        {
            UpdateAtRuntime = true;
        }

    }
}
