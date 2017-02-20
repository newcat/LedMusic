using LedMusic.Models;
using System;

namespace LedMusic.Controller
{
    [Serializable]
    class SpectrumController : ControllerBase
    {

        public SpectrumController()
        {

            PropertyModel p = new PropertyModel("Amplitude", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Lower Frequency", 0, 1);
            p.SetMappingFunc((d) => (Math.Pow(21, d) - 1) * 1000);
            p.SetToStringFunc((d) => d.ToString("N2") + " Hz");
            AnimatableProperties.Add(p);

            p = new PropertyModel("Upper Frequency", 0, 1);
            p.SetMappingFunc((d) => (Math.Pow(21, d) - 1) * 1000);
            p.SetToStringFunc((d) => d.ToString("N2") + " Hz");
            AnimatableProperties.Add(p);

            p = new PropertyModel("Lower Threshold", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Upper Threshold", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Invert", 0, 1);
            p.SetToStringFunc((d) => (int)d == 0 ? "Inverted" : "Not inverted");
            AnimatableProperties.Add(p);

        }

        public override double GetValueAt(int frame)
        {
            int lowerFrequencyIndex = SoundEngine.Instance.GetFftBandIndex((float)AnimatableProperties.GetProperty("Lower Frequency").Value);
            int upperFrequencyIndex = SoundEngine.Instance.GetFftBandIndex((float)AnimatableProperties.GetProperty("Upper Frequency").Value);

            //Check if we already got a wave source and if not, return 0
            if (lowerFrequencyIndex == -1 || upperFrequencyIndex == -1)
                return 0;

            double amplitude = AnimatableProperties.GetProperty("Amplitude").Value;
            double lowerThreshold = AnimatableProperties.GetProperty("Lower Threshold").Value;
            double upperThreshold = AnimatableProperties.GetProperty("Upper Threshold").Value;

            double t = (double)frame / GlobalProperties.Instance.FPS;

            float[] fftData = SoundEngine.Instance.CurrentFftData;

            double totalLevel = 0;

            for (int i = lowerFrequencyIndex; i <= upperFrequencyIndex; i++)
            {
                    totalLevel += fftData[i] * 9;
            }

            double level = totalLevel / (upperFrequencyIndex - lowerFrequencyIndex);
            if (level < 0)
                return 0;

            double returnValue = amplitude * ((level - lowerThreshold) / (upperThreshold - lowerThreshold));
            if ((int)AnimatableProperties.GetProperty("Invert").Value == 1)
                returnValue = 1 - returnValue;

            if (double.IsNaN(returnValue))
                return 0;

            return Math.Min(1, Math.Max(0, returnValue));
        }

    }
}
