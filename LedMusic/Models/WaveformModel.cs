using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LedMusic.Models
{
    public class WaveformModel : INotifyPropertyChanged
    {

        #region INotifyProperyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private float[] _samples;
        public float[] Samples
        {
            get { return _samples; }
            set
            {
                _samples = value;
                NotifyPropertyChanged();
            }
        }

    }
}
