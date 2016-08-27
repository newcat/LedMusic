﻿using LedMusic.Models;
using System.Collections.ObjectModel;

namespace LedMusic.Interfaces
{
    interface IAnimatable
    {

        ObservableCollection<PropertyModel> AnimatableProperties { get; set; }
        ObservableCollection<AnimatedProperty> AnimatedProperties { get; set; }

    }
}
