using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Generators
{
    class DotGenerator : IGenerator, IAnimatable, INotifyPropertyChanged
    {

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

        public ObservableCollection<AnimatedProperty> AnimatedProperties { get; set; }
        public ObservableCollection<IController> Controllers { get; set; }

        public DotGenerator()
        {
            AnimatedProperties = new ObservableCollection<AnimatedProperty>();
            Controllers = new ObservableCollection<IController>();
        }

        public Color[] getSample(int frame)
        {

            int ledCount = GlobalProperties.Instance.LedCount;

            ColorHSV[] colors = new ColorHSV[ledCount];

            for (int i = (int)Math.Floor(CenterPosition - Glow); i <= (int)Math.Ceiling(CenterPosition + Glow); i++)
            {
                if (i >= 0 && i < ledCount)
                    //(-abs(x - CenterPosition)) / (Glow + 1) + 1
                    colors[i] = new ColorHSV(Math.Max((-Math.Abs(i - CenterPosition)) / (Glow + 1) + 1, 0),
                        Hue, Saturation, Value);
            }

            return colors;


        }
    }
}
