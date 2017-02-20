using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Generators
{
    [Serializable]
    class StripGenerator : INotifyPropertyChanged, IGenerator, IAnimatable
    {

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ObservableCollection<PropertyModel> _animatableProperties = new ObservableCollection<PropertyModel>();
        public ObservableCollection<PropertyModel> AnimatableProperties
        {
            get { return _animatableProperties; }
            set
            {
                _animatableProperties = value;
                NotifyPropertyChanged();
            }
        }

        public string GeneratorName { get { return "Strip"; } }

        public StripGenerator() {

            PropertyModel p = new PropertyModel("Start LED", 0, GlobalProperties.Instance.LedCount - 1);
            p.SetToStringFunc((d) => "LED #" + ((int)d + 1));
            AnimatableProperties.Add(p);

            p = new PropertyModel("End LED", 0, GlobalProperties.Instance.LedCount - 1);
            p.SetToStringFunc((d) => "LED #" + ((int)d + 1));
            AnimatableProperties.Add(p);

            p = new PropertyModel("Alpha", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Hue", 0, 360);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Saturation", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Value", 0, 1);
            AnimatableProperties.Add(p);

        }

        public Color[] getSample(int frame)
        {

            Color[] sample = new Color[GlobalProperties.Instance.LedCount];

            double alpha = AnimatableProperties.GetProperty("Alpha").RenderValue;
            double hue = AnimatableProperties.GetProperty("Hue").RenderValue;
            double saturation = AnimatableProperties.GetProperty("Saturation").RenderValue;
            double value = AnimatableProperties.GetProperty("Value").RenderValue;

            int startLED = (int)AnimatableProperties.GetProperty("Start LED").RenderValue;
            int endLED = (int)AnimatableProperties.GetProperty("End LED").RenderValue;

            for (int i = startLED; i <= endLED; i++)
            {
                if (startLED >= 0 && endLED < sample.Length)
                    sample[i] = new ColorHSV(hue, saturation, alpha * value);
            }

            return sample;

        }

    }
}
