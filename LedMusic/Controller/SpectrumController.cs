using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Controller
{
    [Serializable]
    class SpectrumController : INotifyPropertyChanged, IAnimatable, IController
    {

        //TODO: Implement Inverted in every controller

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

        private string _propertyName;
        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                _propertyName = value;
                NotifyPropertyChanged();
            }
        }

        private double _amplitude = 1;
        [Animatable(0, 1)]
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                _amplitude = value;
                NotifyPropertyChanged();
            }
        }

        private double _lowerFrequency;
        [Animatable(0, 14, true)]
        public double LowerFrequency
        {
            get { return _lowerFrequency; }
            set
            {
                _lowerFrequency = value;
                NotifyPropertyChanged();
            }
        }

        private double _upperFrequency = 7;
        [Animatable(0, 14, true)]
        public double UpperFrequency
        {
            get { return _upperFrequency; }
            set
            {
                _upperFrequency = value;
                NotifyPropertyChanged();
            }
        }

        private double _lowerThreshold;
        [Animatable(0, 1)]
        public double LowerThreshold
        {
            get { return _lowerThreshold; }
            set
            {
                _lowerThreshold = value;
                NotifyPropertyChanged();
            }
        }

        private double _upperThreshold = 1;
        [Animatable(0, 1)]
        public double UpperThreshold
        {
            get { return _upperThreshold; }
            set
            {
                _upperThreshold = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isInverted;
        [Animatable(0,1)]
        public bool IsInverted
        {
            get { return _isInverted; }
            set
            {
                _isInverted = value;
                NotifyPropertyChanged();
            }
        }

        private double minValue;
        private double maxValue;

        public SpectrumController() { }

        public void initialize(string propName, double minVal, double maxVal)
        {
            PropertyName = propName;
            minValue = minVal;
            maxValue = maxVal;
        }

        public double getValueAt(int frame)
        {
            int lowerFrequencyIndex = SoundEngine.Instance.GetFftBandIndex((float)Math.Pow(2, LowerFrequency));
            int upperFrequencyIndex = SoundEngine.Instance.GetFftBandIndex((float)Math.Pow(2, UpperFrequency));

            double t = (double)frame / GlobalProperties.Instance.FPS;

            float[] fftData = SoundEngine.Instance.CurrentFftData;

            double totalLevel = 0;

            for (int i = lowerFrequencyIndex; i <= upperFrequencyIndex; i++)
            {
                    totalLevel += fftData[i] * 9;
            }

            double level = totalLevel / (upperFrequencyIndex - lowerFrequencyIndex);
            if (level < 0)
                return 0;

            double returnValue = Amplitude * ((level - LowerThreshold) / (UpperThreshold - LowerThreshold));
            if (IsInverted)
                returnValue = maxValue - (returnValue - minValue);

            if (returnValue > maxValue)
                return maxValue;
            else if (returnValue < minValue)
                return minValue;
            else if (double.IsNaN(returnValue))
                return minValue;
            else
                return returnValue;
        }

    }
}
