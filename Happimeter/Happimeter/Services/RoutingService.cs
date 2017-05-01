using System.Linq;
using System.Threading.Tasks;
using Happimeter.Data;
using Happimeter.Views;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.RoutingService))]
namespace Happimeter.Services
{
    public class RoutingService : IRoutingService
    {
        public void HandleAppStart()
        {
            Application.Current.MainPage = new TabbedPage
            {
                Children =
                {
                    new NavigationPage(new RecordingPage())
                    {
                        Title = "Speech Analyzer",
                        Icon = Device.OnPlatform<string>("tab_about.png", null, null)
                    },
                    new NavigationPage(new TurnTakingPage())
                    {
                        Title = "Turn Taking",
                        Icon = Device.OnPlatform<string>("tab_about.png", null, null)
                    },
                }
            };
        }
    }
}
