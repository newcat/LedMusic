using System;

namespace LedMusic.StaticStuff
{
    class BeatHelper
    {

        public static bool IsFrameBeat(int frameNumber)
        {

            double t = (double)frameNumber / GlobalProperties.Instance.FPS;
            double frameDuration = 1.0 / GlobalProperties.Instance.FPS;

            return Math.Floor(getBeatNumberByTime(t)) != Math.Floor(getBeatNumberByTime(t - frameDuration));

        }

        public static double getTimeByBeatNumber(int beatNumber)
        {
            return (60 / GlobalProperties.Instance.BPM) * (beatNumber + GlobalProperties.Instance.BeatOffset);
        }

        public static double getBeatNumberByTime(double t)
        {
            return (t * GlobalProperties.Instance.BPM) / 60 - GlobalProperties.Instance.BeatOffset;
        }

    }
}
