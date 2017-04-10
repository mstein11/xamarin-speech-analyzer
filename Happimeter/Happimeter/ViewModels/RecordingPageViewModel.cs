using System;
using System.Windows.Input;
using Happimeter.Models;
using Happimeter.Services;
using Xamarin.Forms;

namespace Happimeter.ViewModels
{
	public class RecordingPageViewModel : BaseViewModel
	{ 
        private IAudioAnalyzerService AudioAnalyzerService { get; set; }

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
		    AudioAnalyzerService = DependencyService.Get<IAudioAnalyzerService>();
		    AudioAnalyzerService.OnProcessAudioUpdate += UpdateSpeechEnergy;
			OpenWebCommand = new Command(ToggleRecord);
		}

		/// <summary>
		/// Command to open browser to xamarin.com
		/// </summary>
		public ICommand OpenWebCommand { get; }

	    public ICommand ScanNetworkCommand
	    {
	        get { return new Command(() =>
	        {

	        }); }
	    }

	    private void ToggleRecord()
	    {
	        if (!AudioAnalyzerService.IsRunning())
	        {
	            ButtonText = "Stop Recording";
                AudioAnalyzerService.Start();
	        }
	        else
	        {
                ButtonText = "Start Recording";
                AudioAnalyzerService.Stop();
	        }
	    }

	    private void UpdateSpeechEnergy(AnalyzedAudioModel model)
	    {            
	        var upscaledVolumen = (model.SpeechEnergyLastSample * 1000);
	        var averageUpscaledVolume = (model.SpeechEnergyLastMinute * 1000);
            SpeachEnergy = upscaledVolumen.ToString("N4");
            
	        AverageSpeechEnergy = averageUpscaledVolume.ToString("N4");
	    }
	}
}
