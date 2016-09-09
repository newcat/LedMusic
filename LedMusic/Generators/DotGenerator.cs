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

        private int _centerPosition = 0;
        [Animatable()]
        public int CenterPosition
        {
            get { return _centerPosition; }
            set
            {
                _centerPosition = value;
                NotifyPropertyChanged();
            }
        }
        public int CenterPosition_MinValue { get { return 0; } }
        public int CenterPosition_MaxValue { get { return GlobalProperties.Instance.LedCount - 1; } }

        private double _alpha = 1d;
        [Animatable(0d, 1d)]
        public double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                NotifyPropertyChanged();
            }
        }

        private double _hue = 0;
        [Animatable(0d, 360d)]
        public double Hue
        {
            get { return _hue; }
            set
            {
                _hue = value;
                NotifyPropertyChanged();
            }
        }

        private double _saturation = 0;
        [Animatable(0d, 1d)]
        public double Saturation
        {
            get { return _saturation; }
            set
            {
                _saturation = value;
                NotifyPropertyChanged();
            }
        }

        private double _value = 0;
        [Animatable(0d, 1d)]
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }

        private double _glow = 0;
        [Animatable()]
        public double Glow
        {
            get { return _glow; }
            set
            {
                _glow = value;
                NotifyPropertyChanged();
            }
        }
        public int Glow_MinValue { get { return 0; } }
        public int Glow_MaxValue { get { return GlobalProperties.Instance.LedCount; } }

        private bool _isSymmetric = false;
        [Animatable(0, 1)]
        public bool IsSymmectric
        {
            get { return _isSymmetric; }
            set
            {
                _isSymmetric = value;
                NotifyPropertyChanged();
            }
        }

        public string GeneratorName { get { return "Dot"; } }

        public ObservableCollection<PropertyModel> AnimatableProperties { get; set; }
        public ObservableCollection<AnimatedProperty> AnimatedProperties { get; set; }
        public ObservableCollection<IController> Controllers { get; set; }

        public Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }

        public DotGenerator()
        {
            AnimatedProperties = new ObservableCollection<AnimatedProperty>();
            Controllers = new ObservableCollection<IController>();
            AnimatableProperties = new ObservableCollection<PropertyModel>();
        }

        public Color[] getSample(int frame)
        {

            int ledCount = GlobalProperties.Instance.LedCount;

            ColorHSV[] colors = new ColorHSV[ledCount];

            for (int i = (int)Math.Floor(CenterPosition - Glow); i <= (int)Math.Ceiling(CenterPosition + Glow); i++)
            {
                if (i >= 0 && i < ledCount)
                    //(-abs(x - CenterPosition)) / (Glow + 1) + 1
                    colors[i] = new ColorHSV(Hue, Saturation, Alpha * Math.Max((-Math.Abs(i - CenterPosition)) / (Glow + 1) + 1, 0) * Value);
            }

            if (IsSymmectric)
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
