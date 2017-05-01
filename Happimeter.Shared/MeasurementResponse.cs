using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Happimeter.Shared
{
    public class MeasurementResponse
    {
        public bool HasTurnTaking { get; set; }
        public bool HasSpeechEnergy { get; set; }

        public List<MeasurementMessage> AllLatestMessages { get; set; }
        public int SpeechEnergyPointsSaved { get; set; }
    }
}
