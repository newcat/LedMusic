using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LedMusic.Models;

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

        public static ColorRGB Overlay(this ColorRGB a, ColorRGB b)
        {
            byte red = Convert.ToByte(a.A * a.R + (1 - a.A) * b.R);
            byte green = Convert.ToByte(a.A * a.G + (1 - a.A) * b.G);
            byte blue = Convert.ToByte(a.A * a.B + (1 - a.A) * b.B);
            return new ColorRGB(1, red, green, blue);
            //double alpha = a.A + (1 - a.A) * b.A;
            //byte red = Convert.ToByte((1 / alpha) * (a.A * a.R + (1 - a.A) * b.A * b.R));
            //byte green = Convert.ToByte((1 / alpha) * (a.A * a.G + (1 - a.A) * b.A * b.G));
            //byte blue = Convert.ToByte((1 / alpha) * (a.A * a.B + (1 - a.A) * b.A * b.B));
            //return new ColorRGB(alpha, red, green, blue);
        }

        //public static ColorRGB Add(this ColorRGB a, ColorRGB b)
        //{
        //    byte red = Convert
        //}
    }
}
