using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMusic.Models
{
    class Keyframe : IComparable<Keyframe>, IEquatable<Keyframe>, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private int _frame = 0;
        public int Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
                NotifyPropertyChanged();
            }
        }

        private double _value = 0;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyPropertyChanged();
            }
        }

        public int CompareTo(Keyframe other)
        {
            return Frame > other.Frame ? 1 : -1;
        }

        public bool Equals(Keyframe other)
        {
            return Frame == other.Frame;
        }

        public Keyframe(int frame, double value)
        {
            Frame = frame;
            Value = value;
        }
    }
}
