using System;

namespace Happimeter.Models
{
    public class AnalyzedAudioModel
    {
        public DateTime TimeStamp { get; set; }
        public double SpeechEnergyLastSample { get; set; }
        public double SpeechEnergyLastMinute { get; set; }
    }
}
