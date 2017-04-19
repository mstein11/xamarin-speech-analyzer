using System;

namespace Happimeter.Server.Data
{
    public class MeasurementPoint
    {
        public Guid Id { get; set; }
        public double ReportedSpeechEnergy { get; set; }
        public bool IsSpeech { get; set; }
        public string CustomIdentifier { get; set; }
        public DateTime MeasurementTakenAtUtc { get; set; }
        public bool IsTurnTaking { get; set; }
        public string TurnTakingGroupName { get; set; }
    }
}