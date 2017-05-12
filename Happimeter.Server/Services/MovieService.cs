using System;
using System.Collections.Generic;
using System.Linq;
using Happimeter.Server.Data;
using Happimeter.Server.Data.HappimeterDatabase;

namespace Happimeter.Server.Services
{
    public class MovieService
    {

        private ApplicationDbContext DbContext { get; set; }
        public MovieService()
        {
            DbContext = ApplicationDbContext.Create();
        }

        public IList<HappimeterUserAccount> GetUsers()
        {
            return DbContext.HappimeterUserAccounts.ToList();
        }

        public void UpdateLastMovieMailSent(HappimeterUserAccount user)
        {
            user.LastSendMovie = DateTime.UtcNow;
            DbContext.SaveChanges();
        }

        public HappimeterUserAccount GetUserByEmail(string email)
        {
            return DbContext.HappimeterUserAccounts.FirstOrDefault(x => x.Email == email);
        }

        public MovieServiceModel GetMovieData(string mail, DateTime referenceDate)
        {
            var model = new MovieServiceModel();
            var mood = DatabaseContext.Instance().GetMoodData(mail, referenceDate);

            if (!mood.Any())
            {
                return new MovieServiceModel
                {
                    HasMoodDataToday = false
                };
            }
            model.HasMoodDataToday = true;
            var moodYesterday = DatabaseContext.Instance().GetMoodData(mail, referenceDate.Subtract(TimeSpan.FromDays(1)));

            var sensor = DatabaseContext.Instance().GetSensorData(mail, referenceDate);
            if (!sensor.Any())
            {
                return new MovieServiceModel
                {
                    HasSensorDataToday = false
                };
            }
            model.HasSensorDataToday = true;

            var dataPoints = new List<MovieDataPoint>();
            foreach (var sensorData in sensor.Where(x => x.GeoLat != null && x.GeoLng != null))
            {
                var relatedMood =
                    mood.Where(x => !x.IsCalculated && (x.Timestamp - sensorData.Timestamp).Duration() < TimeSpan.FromHours(1))
                        .OrderBy(x => x.Timestamp.Ticks - sensorData.Timestamp.Ticks)
                        .FirstOrDefault();
                if (relatedMood == null)
                {
                    relatedMood = mood.OrderBy(x => Math.Abs(x.Timestamp.Ticks - sensorData.Timestamp.Ticks)).FirstOrDefault();
                }
                
                var maxMood = mood.Select(x => x.Activation + x.Pleasant).Max();
                var minMood = 0;
                var heatMapPoint = new HeatMapData
                {
                    MaxMood = maxMood,
                    MinMood = minMood,
                    Weight = (relatedMood?.Activation ?? 0) + relatedMood?.Pleasant ?? 0,
                    Pleasance = (relatedMood?.Pleasant ?? 0),
                    Activation = relatedMood?.Activation ?? 0,
                    // ReSharper disable once PossibleInvalidOperationException
                    GeoLat = sensorData.GeoLat.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    GeoLng = sensorData.GeoLng.Value,
                    IsCalculated = relatedMood?.IsCalculated ?? true
                };
                dataPoints.Add(new MovieDataPoint {SensorData = sensorData, MoodData = relatedMood, HeatMapData = heatMapPoint});
            }

            model.Name = mood.First().Name;
            model.HappinessRatio = (int) ((double) mood.Count(x => x.Pleasant > 1) / mood.Count * 100);
            model.NumberLocations = sensor.Count(x => x.GeoLat != null && x.GeoLng != null);
            model.MovieDataPoints = dataPoints;

            model.ActivationToYesterday = (int)
                                          ((double) mood.Count(x => x.Activation > 1) / mood.Count * 100) -
                                          (!moodYesterday.Any()
                                              ? 0
                                              : (int)
                                              ((double) moodYesterday.Count(x => x.Activation > 1) /
                                               moodYesterday.Count *
                                               100));

            model.HappinessToYesterday = model.HappinessRatio -
                                         (!moodYesterday.Any()
                                             ? 0
                                             : (int)
                                             ((double) moodYesterday.Count(x => x.Pleasant > 1) / moodYesterday.Count *
                                              100));


            return model;
        }

        public class MovieDataPoint
        {
            public SensorData SensorData { get; set; }

            public MoodData MoodData { get; set; }

            public HeatMapData HeatMapData { get; set; }
        }

        public class MovieServiceModel
        {
            public bool HasMoodDataToday { get; set; }
            public bool HasMoodDataYesterday { get; set; }
            public bool HasSensorDataToday { get; set; }


            public string Name { get; set; }
            public int HappinessRatio { get; set; }
            public int HappinessToYesterday { get; set; }
            public double ActivationToYesterday { get; set; }
            public int NumberLocations { get; set; }

            public List<MovieDataPoint> MovieDataPoints { get; set; }
        }
    }
}