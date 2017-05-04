using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using Happimeter.Server.Data.HappimeterDatabase;
using Happimeter.Server.Services;

namespace Happimeter.Server.Controllers
{
    public class MeasurementStatisticsController : Controller
    {
        private MeasurementService _measurementService;
        private MovieService _movieService;
        public MeasurementStatisticsController()
        {
            _measurementService = new MeasurementService();
            _movieService = new MovieService();
        }

        public ActionResult Test()
        {
            var testMail = "pbudner@mit.edu";
            var testDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            var mood = DatabaseContext.Instance().GetMoodData(testMail, testDate);
            var sensor = DatabaseContext.Instance().GetSensorData(testMail, testDate);
            return null;
        }

        public ActionResult HappinessMovie()
        {
            var testMail = "pbudner@mit.edu";
            var testDate = new DateTime(2017, 05, 01, 6, 0, 0);
            var models = _movieService.GetMovieData(testMail, testDate);

            return View(models);
        }

        // GET: MeasurementStatistics
        public async Task<ActionResult> Index()
        {
            try
            {
                var measures = await _measurementService.GetMeasurements();
                return View(measures);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return View();
        }
    }
}