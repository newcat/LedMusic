using LedMusic.Models;
using System;

namespace LedMusic.Controller
{
    [Serializable()]
    class LFO : ControllerBase
    {

        public LFO()
        {

            //Register Properties
            PropertyModel p = new PropertyModel("Waveform", 0, 3);
            p.SetToStringFunc((d) => {
                if ((int)d == 0)
                    return "Sine";
                else if ((int)d == 1)
                    return "Square";
                else if ((int)d == 2)
                    return "Triangle";
                else if ((int)d == 3)
                    return "Sawtooth";
                else
                    return "Invalid";
            });
            AnimatableProperties.Add(p);

            p = new PropertyModel("IsInverted", 0, 1);
            p.SetToStringFunc((d) => (int)d == 0 ? "Not inverted" : "Inverted");
            AnimatableProperties.Add(p);

            p = new PropertyModel("Frequency", 0, 100);
            p.SetMappingFunc((d) => Math.Pow(100, d) / (1 / d));
            p.SetToStringFunc((d) => d.ToString("0.00") + " Hz");
            AnimatableProperties.Add(p);

            p = new PropertyModel("IsSyncedToBeat", 0, 1);
            p.SetToStringFunc((d) => (int)d == 0 ? "Unsynchronized" : "Synchronized");
            AnimatableProperties.Add(p);

            p = new PropertyModel("BeatFrequency", 0, 7);
            p.SetToStringFunc((d) =>
            {
                switch ((int)d)
                {
                    case 0:
                        return "1/8";
                    case 1:
                        return "1/6";
                    case 2:
                        return "1/4";
                    case 3:
                        return "1/3";
                    case 4:
                        return "1/2";
                    case 5:
                        return "1";
                    case 6:
                        return "2";
                    case 7:
                        return "4";
                    default:
                        return "Invalid";
                }
            });
            AnimatableProperties.Add(p);

            p = new PropertyModel("Amplitude", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Offset", 0, 1);
            AnimatableProperties.Add(p);

        }

        public override double GetValueAt(int frameNumber)
        {

            double freq = 0; // in Hz
            double phaseOffset; // in seconds
            double returnValue;

            if ((int)AnimatableProperties.GetProperty("IsSyncedToBeat").Value == 1)
            {

                double bpm = GlobalProperties.Instance.BPM;
                double bps = bpm / 60;
                
                switch ((int)AnimatableProperties.GetProperty("BeatFrequency").Value)
                {
                    case 1:
                        freq = 8 * bps;
                        break;
                    case 2:
                        freq = 6 * bps;
                        break;
                    case 3:
                        freq = 4 * bps;
                        break;
                    case 4:
                        freq = 3 * bps;
                        break;
                    case 5:
                        freq = 2 * bps;
                        break;
                    case 6:
                        freq = bps;
                        break;
                    case 7:
                        freq = 0.5 * bps;
                        break;
                    case 8:
                        freq = 0.25 * bps;
                        break;
                }

                phaseOffset = GlobalProperties.Instance.BeatOffset * bps;

            } else
            {

                freq = AnimatableProperties.GetProperty("Frequency").Value;
                phaseOffset = 0;

            }

            double amplitude = AnimatableProperties.GetProperty("Amplitude").Value;
            double offset = AnimatableProperties.GetProperty("Offset").Value;
            double value = 0;
            double t = (double)frameNumber / GlobalProperties.Instance.FPS;

            Waveform wv = (Waveform)AnimatableProperties.GetProperty("Waveform").Value;

            switch (wv)
            {
                case Waveform.SINE:
                    value = (Math.Sin(2 * Math.PI * freq * (t - phaseOffset) - 0.5 * Math.PI) * 0.5 + 0.5) * amplitude + offset;
                    break;
                case Waveform.SAWTOOTH:
                    value = amplitude * (freq * (t - phaseOffset) - Math.Floor(freq * (t - phaseOffset))) + offset;
                    break;
            }

            if ((int)AnimatableProperties.GetProperty("IsInverted").Value == 1)
            {
                returnValue = (amplitude + offset) - value;
            } else
            {
                returnValue = value;
            }

            if (double.IsNaN(returnValue))
                return 0;

            return Math.Min(1, Math.Max(0, returnValue));

        }

    }
}
