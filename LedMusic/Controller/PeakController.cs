using LedMusic.Models;
using System;

namespace LedMusic.Controller
{
    [Serializable()]
    class PeakController : ControllerBase
    {

        public PeakController()
        {

            PropertyModel p = new PropertyModel("Amplitude", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Lower Threshold", 0, 1);
            AnimatableProperties.Add(p);

            p = new PropertyModel("Upper Threshold", 0, 1);
            AnimatableProperties.Add(p);

        }

        public override double GetValueAt(int frameNumber)
        {

            float level = SoundEngine.Instance.GetCurrentSample();
            double returnValue;

            double upperThreshold = AnimatableProperties.GetProperty("Upper Threshold").Value;
            double lowerThreshold = AnimatableProperties.GetProperty("Lower Threshold").Value;
            double amplitude = AnimatableProperties.GetProperty("Amplitude").Value;

            if (level > upperThreshold)
            {
                returnValue = amplitude;
            } else if (level < lowerThreshold)
            {
                returnValue = 0;
            } else
            {
                returnValue = amplitude * ((level - lowerThreshold) / (upperThreshold - lowerThreshold));
            }

            if (double.IsNaN(returnValue))
                return 0;

            return Math.Min(1, Math.Max(0, returnValue));

        }

    }
}
