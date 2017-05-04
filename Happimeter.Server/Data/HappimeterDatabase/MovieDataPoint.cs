using System;

namespace Happimeter.Server.Data.HappimeterDatabase
{
    public class MovieDataPoint
    {
        
    }

    public class MoodData
    {
        public string Email { get; set; }

        public int Pleasant { get; set; }

        public int Activation { get; set; }

        public DateTime Timestamp { get; set; }
    }

    public class SensorData
    {
        public DateTime Timestamp { get; set; }

        public double GeoLat { get; set; }

        public double GeoLng { get; set; }

        public int Activity { get; set; }

        public long Vmc { get; set; }

        public int LightLevel { get; set; }

        public int AvgBpm { get; set; }
    }

    public class HeatMapData
    {
        public int MaxMood { get; set; }

        public int MinMood { get; set; }
        public int Weight { get; set; }

        public int Pleasance { get; set; }

        public int Activation { get; set; }

        public double GeoLat { get; set; }

        public double GeoLng { get; set; }
    }
}