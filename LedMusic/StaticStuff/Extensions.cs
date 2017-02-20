using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LedMusic.Models;
using LedMusic.Interfaces;

namespace LedMusic
{
    public static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        public static PropertyModel GetProperty<T>(this ObservableCollection<T> collection, string propertyName) where T : PropertyModel
        {
            return collection.FirstOrDefault((p) => p.Name == propertyName);
        }

        public static ColorRGB Overlay(this ColorRGB a, ColorRGB b)
        {
            double alphaA = a.getColorHSV().V;
            double alphaB = a.getColorHSV().V;
            byte red = Convert.ToByte(alphaA * a.R + (1 - alphaA) * b.R);
            byte green = Convert.ToByte(alphaA * a.G + (1 - alphaA) * b.G);
            byte blue = Convert.ToByte(alphaA * a.B + (1 - alphaA) * b.B);
            return new ColorRGB(red, green, blue);
            //double alpha = a.A + (1 - a.A) * b.A;
            //byte red = Convert.ToByte((1 / alpha) * (a.A * a.R + (1 - a.A) * b.A * b.R));
            //byte green = Convert.ToByte((1 / alpha) * (a.A * a.G + (1 - a.A) * b.A * b.G));
            //byte blue = Convert.ToByte((1 / alpha) * (a.A * a.B + (1 - a.A) * b.A * b.B));
            //return new ColorRGB(alpha, red, green, blue);
        }

        public static ColorHSV Add(this ColorHSV a, ColorHSV b)
        {
            ColorRGB aRGB = a.getColorRGB();
            ColorRGB bRGB = b.getColorRGB();
            byte red = Convert.ToByte(Math.Min(aRGB.R + bRGB.R, 255));
            byte green = Convert.ToByte(Math.Min(aRGB.G + bRGB.G, 255));
            byte blue = Convert.ToByte(Math.Min(aRGB.B + bRGB.B, 255));
            return new ColorRGB(red, green, blue).getColorHSV();
        }

    }
}
