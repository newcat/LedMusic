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

        //0: Sine
        //1: Square
        //2: Triangle
        //3: Sawtooth
        private int _waveform = 0;
        [Animatable(0, 3)]
        public int Waveform
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
        public double Amplitude_MaxValue { get { return maxValue - minValue; } }

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

        public Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }

        private double minValue;
        private double maxValue;

        public LFO()
        {
            AnimatedProperties = new ObservableCollection<AnimatedProperty>();
        }

        public void initialize(string propertyName, double minValue, double maxValue)
        {
            PropertyName = propertyName;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public double getValueAt(int frameNumber)
        {

            double freq = 0; // in Hz
            double phaseOffset; // in seconds
            double returnValue;

            if (IsSyncedToBeat)
            {

                double bpm = GlobalProperties.Instance.BPM;
                double bps = bpm / 60;
                
                switch (BeatFrequency)
                {
                    case 1:
                        freq = 8 * bps;
                        break;
                    case 2:
                        freq = 6 * bps;
                        break;
                    case 3:
                        freq = 4 * bps;
                        break;
                    case 4:
                        freq = 3 * bps;
                        break;
                    case 5:
                        freq = 2 * bps;
                        break;
                    case 6:
                        freq = bps;
                        break;
                    case 7:
                        freq = 0.5 * bps;
                        break;
                    case 8:
                        freq = 0.25 * bps;
                        break;
                }

                phaseOffset = GlobalProperties.Instance.BeatOffset * bps;

            } else
            {

                freq = Frequency;
                phaseOffset = 0;

            }

            double value = 0;
            double t = (double)frameNumber / GlobalProperties.Instance.FPS;

            Waveform wv = (Waveform)Waveform;

            switch (wv)
            {
                case Models.Waveform.SINE:
                    value = (Math.Sin(2 * Math.PI * freq * (t - phaseOffset) - 0.5 * Math.PI) * 0.5 + 0.5) * Amplitude + Offset;
                    break;
                case Models.Waveform.SAWTOOTH:
                    value = Amplitude * (freq * (t - phaseOffset) - Math.Floor(freq * (t - phaseOffset))) + Offset;
                    break;
            }

            if (IsInverted)
            {
                returnValue = (Amplitude + Offset) - value;
            } else
            {
                returnValue = value;
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
