using LedMusic.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LedMusic.Models
{
    class Layer : INotifyPropertyChanged, IComparable<Layer>
    {
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

        public ObservableCollection<PropertyModel> _generatorProperties;
        public ObservableCollection<PropertyModel> GeneratorProperties
        {
            get { return _generatorProperties; }
            set
            {
                _generatorProperties = value;
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
            Generator = generator;
            updateGenProperties();
        }

        public void updateGenProperties()
        {
            ObservableCollection<PropertyModel> returnList = new ObservableCollection<PropertyModel>();

            foreach (PropertyInfo pi in Generator.GetType().GetProperties())
            {
                foreach (Attribute a in pi.GetCustomAttributes())
                {
                    if (a is AnimatableAttribute)
                    {
                        AnimatableAttribute aa = (AnimatableAttribute)a;
                        double minValue;
                        double maxValue;
                        if (aa.UpdateAtRuntime)
                        {
                            minValue = Convert.ToDouble(Generator.GetType().GetProperty(pi.Name + "_MinValue").GetValue(Generator));
                            maxValue = Convert.ToDouble(Generator.GetType().GetProperty(pi.Name + "_MaxValue").GetValue(Generator));
                        } else
                        {
                            minValue = aa.MinValue;
                            maxValue = aa.MaxValue;
                        }
                        returnList.Add(new PropertyModel(pi.Name, minValue, maxValue, Convert.ToDouble(pi.GetValue(Generator))));
                    }
                }
            }

            GeneratorProperties = returnList;

        }

    }
}
