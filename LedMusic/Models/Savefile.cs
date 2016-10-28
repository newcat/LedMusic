using System;
using System.Collections.Generic;

namespace LedMusic.Models
{
    [Serializable()]
    public class Savefile
    {

        //Global Properties
        public int fps = GlobalProperties.Instance.FPS;
        public int ledCount = GlobalProperties.Instance.LedCount;
        public double bpm = GlobalProperties.Instance.BPM;
        public double beatOffset = GlobalProperties.Instance.BeatOffset;
        public string musicFile = SoundEngine.Instance.File;

        public List<Layer> layers;

        public Savefile(List<Layer> layers)
        {
            this.layers = layers;
        }

        public Savefile() { }

    }
}
