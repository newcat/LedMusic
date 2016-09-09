using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        private ObservableCollection<AnimatedProperty> _animatedProperties = new ObservableCollection<AnimatedProperty>();
        public ObservableCollection<AnimatedProperty> AnimatedProperties
        {
            get { return _animatedProperties; }
            set
            {
                _animatedProperties = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<IController> _controllers = new ObservableCollection<IController>();
        public ObservableCollection<IController> Controllers
        {
            get { return _controllers; }
            set
            {
                _controllers = value;
                NotifyPropertyChanged();
            }
        }

        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get { return _id; }
        }

        public string GeneratorName { get { return "Strip"; } }

        private int _startLED = 0;
        [Animatable()]
        public int StartLED
        {
            get { return _startLED; }
            set
            {
                _startLED = value;
                NotifyPropertyChanged();
            }
        }
        public int StartLED_MinValue { get { return 0; } }
        public int StartLED_MaxValue { get { return GlobalProperties.Instance.LedCount - 1; } }

        private int _endLED = 0;
        [Animatable()]
        public int EndLED
        {
            get { return _endLED; }
            set
            {
                _endLED = value;
                NotifyPropertyChanged();
            }
        }
        public int EndLED_MinValue { get { return 0; } }
        public int EndLED_MaxValue { get { return GlobalProperties.Instance.LedCount - 1; } }

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

        public StripGenerator() { }

        public Color[] getSample(int frame)
        {

            Color[] sample = new Color[GlobalProperties.Instance.LedCount];

            for (int i = StartLED; i <= EndLED; i++)
            {
                if (StartLED >= 0 && EndLED < sample.Length)
                    sample[i] = new ColorHSV(Hue, Saturation, Alpha * Value);
            }

            return sample;

        }

    }
}
