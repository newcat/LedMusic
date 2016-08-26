namespace LedMusic.Models
{
    class PropertyModel
    {

        public double CurrentValue { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public string Name { get; private set; }

        public PropertyModel(string _name, double _minValue, double _maxValue, double _currentValue)
        {
            Name = _name;
            MinValue = _minValue;
            MaxValue = _maxValue;
            CurrentValue = _currentValue;
        }

    }
}
