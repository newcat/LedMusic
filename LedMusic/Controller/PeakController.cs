using LedMusic.Interfaces;
using LedMusic.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Controller
{
    class PeakController : INotifyPropertyChanged, IAnimatable, IController
    {

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

        public double getValueAt(int frameNumber)
        {
            int fps = GlobalProperties.Instance.FPS;
            float[] levels = BassEngine.Instance.getLevelAt(frameNumber / (double)fps, 1.0 / fps);
            return Sensitivity * (maxValue - minValue) * ((levels[0] + levels[1]) / 2.0) + minValue;
        }

    }
}
