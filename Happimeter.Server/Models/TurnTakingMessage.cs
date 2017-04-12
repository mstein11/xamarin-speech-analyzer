using System;

namespace Happimeter.Server.Models
{
    public class TurnTakingMessage
    {
        public string UserId { get; set; }
        public string GroupName { get; set; }
        public double Volumne { get; set; }
        public DateTime AudioTimeStamp { get; set; }
        public Guid Id { get; set; }
    }
}
