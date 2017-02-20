using LedMusic.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Models
{
    [Serializable()]
    public class Layer : INotifyPropertyChanged, IComparable<Layer>
    {
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private int _layerNumber;
        public int LayerNumber
        {
            get { return _layerNumber; }
            set
            {
                _layerNumber = value;
                NotifyPropertyChanged();
            }
        }

        private string _layerName;
        public string LayerName
        {
            get { return _layerName; }
            set
            {
                _layerName = value;
                NotifyPropertyChanged();
            }
        }

        private double _alpha = 1;
        public double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                NotifyPropertyChanged();
            }
        }

        private IGenerator _generator;
        public IGenerator Generator
        {
            get { return _generator; }
            set
            {
                _generator = value;
                NotifyPropertyChanged();
            }
        }

        private LayerColorMode _colorMode = LayerColorMode.OVERLAY;
        public LayerColorMode ColorMode
        {
            get { return _colorMode; }
            set
            {
                _colorMode = value;
                NotifyPropertyChanged();
            }
        }

        public int CompareTo(Layer other)
        {
            return LayerNumber - other.LayerNumber;
        }

        public Layer(int layerNumber, IGenerator generator)
        {
            LayerNumber = layerNumber;
            LayerName = "Layer " + LayerNumber.ToString();
            IAnimatable a = (IAnimatable)generator;
            Generator = (IGenerator)a;
        }

    }
}
