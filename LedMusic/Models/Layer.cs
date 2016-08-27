using LedMusic.Interfaces;
using LedMusic.StaticStuff;
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
            get { return "Layer " + LayerNumber.ToString(); }
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
            IAnimatable a = (IAnimatable)generator;
            PropertiesHelper.updateAnimatableProperties(ref a);
            Generator = (IGenerator)a;
        }

        #region Serialization
        //public Layer(SerializationInfo info, StreamingContext context) {
        //    LayerNumber = info.GetInt32("LayerNumber");
        //    _layerName = info.GetString("LayerName"); //TODO
        //    Alpha = info.GetDouble("Alpha");
        //    Generator = (IGenerator)info.GetValue("Generator", typeof(IGenerator));
        //    ColorMode = (LayerColorMode)info.GetValue("ColorMode", typeof(LayerColorMode));
        //}

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("LayerNumber", LayerNumber);
        //    info.AddValue("LayerName", LayerName);
        //    info.AddValue("Alpha", Alpha);
        //    info.AddValue("Generator", Generator, Generator.GetType());
        //    info.AddValue("ColorMode", ColorMode);
        //}
        #endregion

    }
}
