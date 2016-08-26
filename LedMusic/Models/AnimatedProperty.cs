using System.Collections.ObjectModel;

namespace LedMusic.Models
{
    class AnimatedProperty
    {

        public string PropertyName { get; set; }
        public ObservableCollection<Keyframe> Keyframes { get; set; }

        public AnimatedProperty(string propName, Keyframe defaultKeyframe)
        {
            PropertyName = propName;
            Keyframes = new ObservableCollection<Keyframe>();
            Keyframes.Add(defaultKeyframe);
        }

    }
}
