using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using Happimeter.Server.Data.HappimeterDatabase;
using Happimeter.Server.Models;
using Happimeter.Server.Services;

namespace Happimeter.Server.Controllers
{
    public class MeasurementStatisticsController : Controller
    {
        private MeasurementService _measurementService;
        private MovieService _movieService;
        private EmailManager _emailManager;

        public MeasurementStatisticsController()
        {
            _measurementService = new MeasurementService();
            _movieService = new MovieService();
            _emailManager = new EmailManager();
        }

        public ActionResult Test()
        {
            var testMail = "pbudner@mit.edu";
            var testDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
            var mood = DatabaseContext.Instance().GetMoodData(testMail, testDate);
            var sensor = DatabaseContext.Instance().GetSensorData(testMail, testDate);
            return null;
        }

        public ActionResult HappinessMovieEmail(string mail, DateTime? date, string uid)
        {
            var user = _movieService.GetUserByEmail(mail);
            if (user == null || user.Id.ToString().ToLower() != uid.ToLower())
            {
                return null;
            }
            return PrepareHappinessMovie(mail, date);
        }

        [HttpPost]
        public ActionResult HappinessMovie(string mail, DateTime? date)
        {
            return PrepareHappinessMovie(mail, date);
        }

        private ActionResult PrepareHappinessMovie(string mail, DateTime? date)
        {
            if (mail == null)
            {
                ViewBag.Error = "No Mail Selected";
                return RedirectToAction("HappinessMovieInit");
            }
            if (date == null)
            {
                date = DateTime.UtcNow;
            }

            var model = _movieService.GetMovieData(mail, date.Value);
            if (!model.HasMoodDataToday)
            {
                ViewBag.Error = "No Mood Data found for that user on that day";
                return RedirectToAction("HappinessMovieInit");
            }

            return View("~/Views/MeasurementStatistics/HappinessMovie.cshtml", model);
        }

        public ActionResult HappinessMovie()
        {
            return RedirectToAction("HappinessMovieInit");
        }

        public ActionResult Cronjob()
        {
            var users = _movieService.GetUsers();
            foreach (var happimeterUserAccount in users)
            {
                if (false && (happimeterUserAccount.LastSendMovie.Date == DateTime.UtcNow.Date || DateTime.UtcNow.Hour < 17))
                {
                    continue;
                }
                var movieData = _movieService.GetMovieData(happimeterUserAccount.Email, DateTime.UtcNow);
                if (false && !movieData.HasMoodDataToday)
                {
                    continue;
                }

                var model = new MovieEmailViewModel
                {
                    Name = movieData.Name,
                    LinkUrl =
                        "http://happimeter-server.azurewebsites.net/MeasurementStatistics/HappinessMovieEmail?mail=" +
                        happimeterUserAccount.Email + "&date=" + DateTime.UtcNow.Date + "&uid=" +
                        happimeterUserAccount.Id
                };

                var body = _emailManager.GetTemplate(MovieEmailViewModel.TemplateKey, model);
                var receiverAddressObj = new MailAddress("mariusstein7@gmail.com");
                var message = new MailMessage
                {
                    From = new MailAddress("mariusstein7@gmail.com"),
                    To = { receiverAddressObj },
                    Subject = "You happiness movie",
                    Body = body,
                    IsBodyHtml = true
                };
                _emailManager.Send(message);

            }
            var emails = users.Select(x => x.Email);

            return null;
        }

        public ActionResult HappinessMovieInit()
        {
            return View();
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