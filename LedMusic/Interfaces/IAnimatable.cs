using LedMusic.Models;
using System.Collections.ObjectModel;

namespace LedMusic.Interfaces
{
    interface IAnimatable
    {

        ObservableCollection<AnimatedProperty> AnimatedProperties { get; set; }

    }
}
