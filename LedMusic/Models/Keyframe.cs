using LedMusic.Viewmodels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace LedMusic.Models
{
    [Serializable()]
    class Keyframe : IComparable<Keyframe>, IEquatable<Keyframe>, INotifyPropertyChanged
    {

        [field:NonSerialized]
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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged();
            }
        }

        private KeyframeMode _mode = KeyframeMode.LINEAR;
        public KeyframeMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                NotifyPropertyChanged();
            }
        }

        [field: NonSerialized]
        private RelayCommand<string> _cmdSetTypeTo;
        public RelayCommand<string> CmdSetTypeTo
        {
            get { return _cmdSetTypeTo; }
            set
            {
                _cmdSetTypeTo = value;
                NotifyPropertyChanged();
            }
        }

        public bool JustDragged { get; set; }

        [OnDeserialized]
        private void OnDeserializing(StreamingContext c)
        {
            CmdSetTypeTo = new RelayCommand<string>(setMode, setMode_CanExecute);
        }

        public int CompareTo(Keyframe other)
        {
            return Frame - other.Frame;
        }

        public bool Equals(Keyframe other)
        {
            return Frame == other.Frame;
        }

        public Keyframe(int frame, double value)
        {
            Frame = frame;
            Value = value;

            MainModel.Instance.PropertyChanged += MainModel_PropertyChanged;
            CmdSetTypeTo = new RelayCommand<string>(setMode, setMode_CanExecute);
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrackWidth")
            {
                NotifyPropertyChanged("Frame");
            }
        }

        public Keyframe Copy()
        {
            return new Keyframe(Frame, Value);
        }

        private void setMode(string s)
        {
            switch (s)
            {
                case "Linear":
                    Mode = KeyframeMode.LINEAR;
                    break;
                case "Hold":
                    Mode = KeyframeMode.HOLD;
                    break;
            }
        }

        private bool setMode_CanExecute(string s)
        {
            return (s == "Linear" && Mode != KeyframeMode.LINEAR) || (s == "Hold" && Mode != KeyframeMode.HOLD);
        }
    }
}
