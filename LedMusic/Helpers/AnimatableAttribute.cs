using System;

namespace LedMusic
{
    [AttributeUsage(AttributeTargets.Property)]
    class AnimatableAttribute : Attribute
    {

        public bool Logarithmic { get; private set; }
        public bool UpdateAtRuntime { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }

        public AnimatableAttribute(double minValue, double maxValue, bool log = false)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            UpdateAtRuntime = false;
            Logarithmic = log;
        }

        public AnimatableAttribute(bool log = false)
        {
            Logarithmic = log;
            UpdateAtRuntime = true;
        }

    }
}
