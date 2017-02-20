using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Generators
{
    [Serializable()]
    class DotGenerator : IGenerator, IAnimatable, INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string GeneratorName { get { return "Dot"; } }

        public ObservableCollection<PropertyModel> AnimatableProperties { get; set; }

        public DotGenerator()
        {
            AnimatableProperties = new ObservableCollection<PropertyModel>();

            PropertyModel p = new PropertyModel("Center Position", 0, GlobalProperties.Instance.LedCount - 1);
            p.SetToStringFunc((d) => "LED #" + ((int)d + 1));
            AnimatableProperties.Add(p);

            p = new PropertyModel("Alpha", 0, 1);
            p.PercentageValue = 1;
            AnimatableProperties.Add(p);

            p = new PropertyModel("Hue", 0, 360);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Saturation", 0, 1);
            p.PercentageValue = 0.5;
            AnimatableProperties.Add(p);

            p = new PropertyModel("Value", 0, 1);
            p.PercentageValue = 1;
            AnimatableProperties.Add(p);

            p = new PropertyModel("Glow", 0, GlobalProperties.Instance.LedCount);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Symmetric", 0, 1);
            p.SetToStringFunc((d) => (int)d == 0 ? "Not symmetric" : "Symmetric");
            AnimatableProperties.Add(p);
        }

        public Color[] getSample(int frame)
        {

            int ledCount = GlobalProperties.Instance.LedCount;
            int centerPosition = (int)AnimatableProperties.GetProperty("Center Position").RenderValue;
            double alpha = AnimatableProperties.GetProperty("Alpha").RenderValue;
            double glow = AnimatableProperties.GetProperty("Glow").RenderValue;
            double hue = AnimatableProperties.GetProperty("Hue").RenderValue;
            double saturation = AnimatableProperties.GetProperty("Saturation").RenderValue;
            double value = AnimatableProperties.GetProperty("Value").RenderValue;

            ColorHSV[] colors = new ColorHSV[ledCount];

            for (int i = (int)Math.Floor(centerPosition - glow); i <= (int)Math.Ceiling(centerPosition + glow); i++)
            {
                if (i >= 0 && i < ledCount)
                    colors[i] = new ColorHSV(hue, saturation, alpha * Math.Max((-Math.Abs(i - centerPosition)) / (glow + 1) + 1, 0) * value);
            }

            if ((int)AnimatableProperties.GetProperty("Symmetric").RenderValue == 1)
            {
                ColorHSV[] reverseColors = new ColorHSV[ledCount];
                for (int i = 0; i < ledCount; i++)
                {
                    if (colors[i] == null)
                        colors[i] = new ColorHSV(0, 0, 0);
                    reverseColors[ledCount - i - 1] = colors[i];
                }
                for (int i = 0; i < ledCount; i++)
                {
                    colors[i] = colors[i].Add(reverseColors[i]);
                }
            }

            return colors;

        }
    }
}
