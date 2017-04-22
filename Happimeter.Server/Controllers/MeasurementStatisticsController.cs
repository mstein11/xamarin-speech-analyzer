using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using Happimeter.Server.Services;

namespace Happimeter.Server.Controllers
{
    public class MeasurementStatisticsController : Controller
    {
        private MeasurementService _measurementService;
        public MeasurementStatisticsController()
        {
            _measurementService = new MeasurementService();
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