using LedMusic.Interfaces;
using LedMusic.Models;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace LedMusic.StaticStuff
{
    class PropertiesHelper
    {

        public static void updateAnimatableProperties(ref IAnimatable a)
        {

            ObservableCollection<PropertyModel> returnList = new ObservableCollection<PropertyModel>();

            foreach (PropertyInfo pi in a.GetType().GetProperties())
            {
                foreach (Attribute at in pi.GetCustomAttributes())
                {
                    if (at is AnimatableAttribute)
                    {
                        AnimatableAttribute aa = (AnimatableAttribute)at;
                        double minValue;
                        double maxValue;
                        if (aa.UpdateAtRuntime)
                        {
                            minValue = Convert.ToDouble(a.GetType().GetProperty(pi.Name + "_MinValue").GetValue(a));
                            maxValue = Convert.ToDouble(a.GetType().GetProperty(pi.Name + "_MaxValue").GetValue(a));
                        }
                        else
                        {
                            minValue = aa.MinValue;
                            maxValue = aa.MaxValue;
                        }
                        returnList.Add(new PropertyModel(pi.Name, minValue, maxValue, Convert.ToDouble(pi.GetValue(a))));
                    }
                }
            }

            a.AnimatableProperties = returnList;

        }

    }
}
