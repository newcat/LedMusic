using System;

namespace LedMusic.Models
{

    public abstract class Color {

        public abstract ColorRGB getColorRGB();
        public abstract ColorHSV getColorHSV();

    }

    public class ColorRGB : Color
    {

        public double A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        /// <summary>
        /// Creates a new color with RGB-Parameters
        /// </summary>
        /// <param name="_a">Alpha</param>
        /// <param name="_r">Red</param>
        /// <param name="_g">Green</param>
        /// <param name="_b">Blue</param>
        public ColorRGB(double _a, byte _r, byte _g, byte _b)
        {
            A = _a;
            R = _r;
            G = _g;
            B = _b;
        }

        public override ColorRGB getColorRGB() { return this; }

        public override ColorHSV getColorHSV()
        {

            int max = Math.Max(R, Math.Max(G, B));
            int min = Math.Min(R, Math.Min(G, B));

            double hue = System.Drawing.Color.FromArgb(255, R, G, B).GetHue();
            double saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            double value = max / 255d;

            return new ColorHSV(A, hue, saturation, value);

        }

    }

    public class ColorHSV : Color
    {

        public double A { get; set; } //Range 0..1
        public double H { get; set; } //Range 0..359
        public double S { get; set; } //Range 0..1
        public double V { get; set; } //Range 0..1

        /// <summary>
        /// Creates a new color with HSV-Parameters
        /// </summary>
        /// <param name="_a">Alpha</param>
        /// <param name="_h">Hue</param>
        /// <param name="_s">Saturation</param>
        /// <param name="_v">Value</param>
        public ColorHSV(double _a, double _h, double _s, double _v)
        {
            A = _a;
            H = _h;
            S = _s;
            V = _v;
        }

        public override ColorRGB getColorRGB()
        {
            int hi = Convert.ToInt32(Math.Floor(H / 60)) % 6;
            double f = H / 60 - Math.Floor(H / 60);

            double value = V * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - S));
            byte q = Convert.ToByte(value * (1 - f * S));
            byte t = Convert.ToByte(value * (1 - (1 - f) * S));

            if (hi == 0)
                return new ColorRGB(A, v, t, p);
            else if (hi == 1)
                return new ColorRGB(A, q, v, p);
            else if (hi == 2)
                return new ColorRGB(A, p, v, t);
            else if (hi == 3)
                return new ColorRGB(A, p, q, v);
            else if (hi == 4)
                return new ColorRGB(A, t, p, v);
            else
                return new ColorRGB(A, v, p, q);
        }

        public override ColorHSV getColorHSV() { return this; }

    }
}
