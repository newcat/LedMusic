namespace LedMusic.Interfaces
{
    interface IController
    {

        double getValueAt(int frameNumber);
        string PropertyName { get; }

        void initialize(string propertyName, double minValue, double maxValue);

    }
}
