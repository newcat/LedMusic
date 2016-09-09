using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedMusic.Models
{
    public enum LayerColorMode {
        [Description("Overlay")]
        OVERLAY,
        [Description("Add")]
        ADD
    }

    public enum Waveform
    {
        SINE, SQUARE, TRIANGLE, SAWTOOTH
    }

    public enum KeyframeMode
    {
        LINEAR, HOLD
    }
}
