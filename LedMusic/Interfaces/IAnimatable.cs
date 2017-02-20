using LedMusic.Models;
using System;
using System.Collections.ObjectModel;

namespace LedMusic.Interfaces
{
    public interface IAnimatable
    {
        ObservableCollection<PropertyModel> AnimatableProperties { get; set; }
    }
}
