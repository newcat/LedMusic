using System;

namespace LedMusic.Models
{
    [Serializable]
    class PropertyModel
    {

        public bool Logarithmic { get; private set; }
        public double CurrentValue { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public string Name { get; private set; }

        public PropertyModel(string _name, double _minValue, double _maxValue, double _currentValue, bool _logarithmic)
        {
            Name = _name;
            MinValue = _minValue;
            MaxValue = _maxValue;
            Logarithmic = _logarithmic;
            CurrentValue = _currentValue;
        }

    }
}
