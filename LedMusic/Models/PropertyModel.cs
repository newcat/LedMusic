using LedMusic.Controller;
using LedMusic.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Models
{
    [Serializable]
    public class PropertyModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double _percentageValue = 0;
        public double PercentageValue
        {
            get { return _percentageValue; }
            set
            {
                _percentageValue = Math.Min(1, Math.Max(0, value));
                NotifyPropertyChanged();
                NotifyPropertyChanged("Value");
                NotifyPropertyChanged("ValueString");
            }
        }

        public double Value
        {
            get
            {
                if (mappingFunc != null)
                    return mappingFunc(PercentageValue);
                else
                    return (MaxValue - MinValue) * PercentageValue + MinValue;
            }
        }

        public string ValueString
        {
            get
            {
                if (toStringFunc != null)
                    return toStringFunc(Value);
                else
                    return Value.ToString("N2");
            }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private ControllerBase _controller;
        public ControllerBase Controller
        {
            get { return _controller; }
            set
            {
                _controller = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The value that will be used for rendering. Will be <see cref="PercentageValue"/> if
        /// no controller is set, else it will be the output of the controller.
        /// </summary>
        public double RenderValuePercentage { get; set; }
        public double RenderValue
        {
            get {
                double percVal = 0;
                if (Controller == null && Keyframes.Count == 0)
                    percVal = PercentageValue;
                else
                    percVal = RenderValuePercentage;

                if (mappingFunc != null)
                    return mappingFunc(percVal);
                else
                    return (MaxValue - MinValue) * percVal + MinValue;
            }
        }

        private Guid _id = Guid.NewGuid();
        public Guid Id { get { return _id; } }

        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public ObservableCollection<Keyframe> Keyframes { get; set; }

        private Func<double, string> toStringFunc = null;
        private Func<double, double> mappingFunc = null;
        
        public PropertyModel(string name, double minValue, double maxValue)
        {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            Keyframes = new ObservableCollection<Keyframe>();
        }

        public void SetMappingFunc(Func<double, double> f)
        {
            mappingFunc = f;
        }

        public void SetToStringFunc(Func<double, string> f)
        {
            toStringFunc = f;
        }

    }
}
