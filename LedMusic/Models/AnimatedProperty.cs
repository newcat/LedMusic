using System;
using System.Collections.ObjectModel;

namespace LedMusic.Models
{
    [Serializable()]
    class AnimatedProperty
    {

        public bool Logarithmic { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public string PropertyName { get; set; }
        public ObservableCollection<Keyframe> Keyframes { get; set; }

        public AnimatedProperty(string propName, Keyframe defaultKeyframe, double minValue, double maxValue, bool log)
        {
            PropertyName = propName;
            MinValue = minValue;
            MaxValue = maxValue;
            Logarithmic = log;
            Keyframes = new ObservableCollection<Keyframe>();
            Keyframes.Add(defaultKeyframe);
        }

    }
}
