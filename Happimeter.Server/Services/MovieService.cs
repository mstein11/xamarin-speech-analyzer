using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Server.Data.HappimeterDatabase;

namespace Happimeter.Server.Services
{
    public class MovieService
    {
        public List<MovieDataPoint> GetMovieData(string mail, DateTime referenceDate)
        {
            var mood = DatabaseContext.Instance().GetMoodData(mail, referenceDate);
            var sensor = DatabaseContext.Instance().GetSensorData(mail, referenceDate);


            var dataPoints = new List<MovieDataPoint>();
            foreach (var sensorData in sensor)
            {
                var relatedMood = mood.OrderBy(x => Math.Abs(x.Timestamp.Ticks - sensorData.Timestamp.Ticks)).FirstOrDefault();
                var maxMood = mood.Select(x => x.Activation + x.Pleasant).Max();
                var minMood = 0;
                var heatMapPoint = new HeatMapData
                {
                    MaxMood = maxMood,
                    MinMood = minMood,
                    Weight = (relatedMood?.Activation ?? 0) + relatedMood?.Pleasant ?? 0,
                    Pleasance = (relatedMood?.Pleasant ?? 0),
                    Activation = relatedMood?.Activation ?? 0,
                    GeoLat = sensorData.GeoLat,
                    GeoLng = sensorData.GeoLng
                };
                dataPoints.Add(new MovieDataPoint {SensorData = sensorData, MoodData = relatedMood, HeatMapData = heatMapPoint});
            }

            
            return dataPoints;
        }

        public class MovieDataPoint
        {
            public SensorData SensorData { get; set; }

            public MoodData MoodData { get; set; }

            public HeatMapData HeatMapData { get; set; }
        }
    }
}