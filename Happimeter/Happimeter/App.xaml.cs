using System.Linq;
using System.Threading.Tasks;
using Happimeter.Data;
using Happimeter.Services;
using Happimeter.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Happimeter
{
	public partial class App : Application
	{

        private static Database _database;

	    private static bool _isRegisteredOnUpdateEvent = false;

        public App()
		{
			InitializeComponent();

			SetMainPage();
		}

	    

        public static Database Database
        {
            get
            {
                if (_database == null)
                {
                    _database = new Database(DependencyService.Get<IFileHelper>().GetLocalFilePath("Happimeter.db3"));
                }
                return _database;
            }
        }

        public static void SetMainPage() => DependencyService.Get<IRoutingService>().HandleAppStart();
    }
}
