using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Controller
{
    [Serializable()]
    class PeakController : INotifyPropertyChanged, IAnimatable, IController
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        private string _propertyName = "";
        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                _propertyName = value;
                NotifyPropertyChanged();
            }
        }

        private double _amplitude;
        [Animatable(0,1)]
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                _amplitude = value;
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

        private double _upperThreshold;
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

        public Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }

        private double maxValue;
        private double minValue;

        public PeakController()
        {
            
        }

        public void initialize(string propertyName, double minValue, double maxValue)
        {
            PropertyName = propertyName;
            this.maxValue = maxValue;
            this.minValue = minValue;
        }

        public double getValueAt(int frameNumber)
        {
            int fps = GlobalProperties.Instance.FPS;
            float[] levels = BassEngine.Instance.getLevelAt(frameNumber / (double)fps, 1.0 / fps);
            double avgLevel = (levels[0] + levels[1]) / 2.0;
            double returnValue;

            if (avgLevel > UpperThreshold)
            {
                returnValue = Amplitude * maxValue;
            } else if (avgLevel < LowerThreshold)
            {
                returnValue = minValue;
            } else
            {
                returnValue = Amplitude * (maxValue - minValue) * ((avgLevel - LowerThreshold) / (UpperThreshold - LowerThreshold)) + minValue;
            }

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
