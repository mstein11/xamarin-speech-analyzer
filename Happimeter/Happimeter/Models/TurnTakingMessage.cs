using System;

namespace Happimeter.Models
{
    public class TurnTakingMessage
    {
        public string IpAdress { get; set; }
        public double Volumne { get; set; }
        public DateTime AudioTimeStamp { get; set; }
        public Guid Id { get; set; }
    }
}
