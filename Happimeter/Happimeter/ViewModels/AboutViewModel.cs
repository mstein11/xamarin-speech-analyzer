using System;
using System.Windows.Input;
using Happimeter.Services;
using Xamarin.Forms;

namespace Happimeter.ViewModels
{
	public class AboutViewModel : BaseViewModel
	{
        private IRecorderService RecordService { get; set; }

        /// <summary>
        /// Private backing field to hold the title
        /// </summary>
        string _speachEnergy = "-";
        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string SpeachEnergy
        {
            get { return _speachEnergy; }
            set { SetProperty(ref _speachEnergy, value); }
        }

        /// <summary>
        /// Private backing field to hold the title
        /// </summary>
        string _buttonText = "Start Recording";
        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string ButtonText
        {
            get { return _buttonText; }
            set { SetProperty(ref _buttonText, value); }
        }

        public AboutViewModel()
		{
			Title = "About";
            RecordService = DependencyService.Get<IRecorderService>();
		    RecordService.Initialize();
			OpenWebCommand = new Command(ToggleRecord);
		}

		/// <summary>
		/// Command to open browser to xamarin.com
		/// </summary>
		public ICommand OpenWebCommand { get; }

	    private void ToggleRecord()
	    {
	        if (!RecordService.IsRunning())
	        {
	            ButtonText = "Stop Recording";
                RecordService.Start();
	        }
	        else
	        {
                ButtonText = "Start Recording";
                var bytes = RecordService.Stop();

                long totalSquare = 0;
                for (int i = 0; i < bytes.Length; i += 2)
                {
                    short sample = (short)(bytes[i] | (bytes[i + 1] << 8));
                    totalSquare += sample * sample;
                }
                long meanSquare = 2 * totalSquare / bytes.Length;
                double rms = Math.Sqrt(meanSquare);
                double volume = rms / 32768.0;
                SpeachEnergy = volume.ToString("N4");
	        }
	    }
	}
}
