using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Controller
{
    [Serializable()]
    class LFO : INotifyPropertyChanged, IAnimatable, IController
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Waveforms _waveform = Waveforms.SINE;
        public Waveforms Waveform
        {
            get { return _waveform; }
            set
            {
                _waveform = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isInverted = false;
        [Animatable(0, 1)]
        public bool IsInverted
        {
            get { return _isInverted; }
            set
            {
                _isInverted = value;
                NotifyPropertyChanged();
            }
        }


        private double _frequency = 10;
        [Animatable(0.01, 1000)]
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isSyncedToBeat = false;
        [Animatable(0, 1)]
        public bool IsSyncedToBeat
        {
            get { return _isSyncedToBeat; }
            set
            {
                _isSyncedToBeat = value;
                NotifyPropertyChanged();
            }
        }

        //1 = 1/8
        //2 = 1/6
        //3 = 1/4
        //4 = 1/3
        //5 = 1/2
        //6 = 1
        //7 = 2
        //8 = 4
        private int _beatFrequency = 6;
        [Animatable(1,8)]
        public int BeatFrequency
        {
            get { return _beatFrequency; }
            set
            {
                _beatFrequency = value;
                NotifyPropertyChanged();
            }
        }

        private double _amplitude = 1d;
        [Animatable()]
        public double Amplitude
        {
            get { return _amplitude; }
            set
            {
                _amplitude = value;
                NotifyPropertyChanged();
            }
        }
        public double Amplitude_MinValue { get { return 0; } }
        public double Amplitude_MaxValue { get { return minValue - maxValue; } }

        private double _offset = 0;
        [Animatable()]
        public double Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                NotifyPropertyChanged();
            }
        }
        public double Offset_MinValue { get { return 0; } }
        public double Offset_MaxValue { get { return maxValue - minValue; } }

        public string PropertyName { get; private set; }

        public ObservableCollection<AnimatedProperty> AnimatedProperties { get; set; }

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

        private double minValue;
        private double maxValue;

        public LFO(double minValue, double maxValue, string propertyName)
        {
            AnimatedProperties = new ObservableCollection<AnimatedProperty>();
            PropertyName = propertyName;
        }

        public LFO() { }

        public double getValueAt(int frameNumber)
        {
            
            if (IsSyncedToBeat)
            {

                return 0;

            } else
            {
                return 0;
            }

        }

    }
}
