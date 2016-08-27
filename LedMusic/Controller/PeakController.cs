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

        private double _sensitivity;
        [Animatable(0,1)]
        public double Sensitivity
        {
            get { return _sensitivity; }
            set
            {
                _sensitivity = value;
                NotifyPropertyChanged();
            }
        }

        private double maxValue;
        private double minValue;

        public PeakController(string propertyName, double minValue, double maxValue)
        {
            PropertyName = propertyName;
            this.maxValue = maxValue;
            this.minValue = minValue;
        }

        public PeakController() { }

        public double getValueAt(int frameNumber)
        {
            int fps = GlobalProperties.Instance.FPS;
            float[] levels = BassEngine.Instance.getLevelAt(frameNumber / (double)fps, 1.0 / fps);
            return Sensitivity * (maxValue - minValue) * ((levels[0] + levels[1]) / 2.0) + minValue;
        }

    }
}
