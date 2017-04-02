using System;
using System.Windows.Input;
using Happimeter.Services;
using Xamarin.Forms;

namespace Happimeter.ViewModels
{
	public class RecordingPageViewModel : BaseViewModel
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
        string _averageSpeechEnergy = "-";

	    private double _sumOfReportedEnergy = 0;

	    private int _numberOfReports = 0;

        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string AverageSpeechEnergy
        {
            get { return _averageSpeechEnergy; }
            set { SetProperty(ref _averageSpeechEnergy, value); }
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

        public RecordingPageViewModel()
		{
			Title = "Speech Analyzer";
            RecordService = DependencyService.Get<IRecorderService>();
		    RecordService.Initialize(UpdateSpeechEnergy);
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
	            _sumOfReportedEnergy = 0;
	            _numberOfReports = 0;
	            ButtonText = "Stop Recording";
                RecordService.Start();
	        }
	        else
	        {
                ButtonText = "Start Recording";
                RecordService.Stop();
	        }
	    }

	    private void UpdateSpeechEnergy(byte[] data)
	    {
            long totalSquare = 0;
            for (int i = 0; i < data.Length; i += 2)
            {
                short sample = (short)(data[i] | (data[i + 1] << 8));
                totalSquare += sample * sample;
            }
            long meanSquare = 2 * totalSquare / data.Length;
            double rms = Math.Sqrt(meanSquare);
            double volume = rms / 32768.0;
	        var upscaledVolumen = (volume * 1000);
            SpeachEnergy = upscaledVolumen.ToString("N4");

	        _sumOfReportedEnergy += upscaledVolumen;
	        _numberOfReports++;

	        AverageSpeechEnergy = (_sumOfReportedEnergy / _numberOfReports).ToString("N4");
	    }
	}
}
