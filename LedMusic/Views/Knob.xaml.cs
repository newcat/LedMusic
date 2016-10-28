using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LedMusic.Views
{
    /// <summary>
    /// Interaction logic for Knob.xaml
    /// </summary>
    public partial class Knob : UserControl, INotifyPropertyChanged
    {

        private const double sensitivity = 0.1;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Value
        {
            get { return calculateValue(); }
            set { setValue(value); NotifyPropertyChanged(); }
        }

        private bool _isLogarithmic;
        public bool IsLogarithmic
        {
            get { return _isLogarithmic; }
            set
            {
                _isLogarithmic = value;
                NotifyPropertyChanged();
            }
        }

        private double _minValue;
        public double MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                NotifyPropertyChanged();
            }
        }

        private double _maxValue;
        public double MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                NotifyPropertyChanged();
            }
        }

        private double internalValue = 0; //Range 0..100, not logarithmic

        public Knob()
        {
            InitializeComponent();

            PropertyChanged += Knob_PropertyChanged;
        }

        private void Knob_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            internalValue += e.HorizontalChange * sensitivity;
            if (internalValue > 100)
                internalValue = 100;
            else if (internalValue < 0)
                internalValue = 0;
        }

        private double calculateValue()
        {
            return 0;
        }

        private void setValue(double newValue)
        {

        }
    }
}
