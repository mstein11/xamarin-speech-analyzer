using System;
using System.Windows.Input;
using Happimeter.Models;
using Happimeter.Services;
using Happimeter.Shared;
using Xamarin.Forms;

namespace Happimeter.ViewModels
{
	public class TurnTakingPageViewModel : BaseViewModel
	{ 
        private ITurnTakingService TurnTakingService { get; set; }

	    private string _groupName = "";

        public string GroupName {
            get { return _groupName; }
            set { SetProperty(ref _groupName, value); }
        }

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

        string _origin = "-";

        public string Origin
        {
            get { return _origin; }
            set { SetProperty(ref _origin, value); }
        }

        /// <summary>
        /// Private backing field to hold the title
        /// </summary>
        string _buttonText = "Start Turntaking";
        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string ButtonText
        {
            get { return _buttonText; }
            set { SetProperty(ref _buttonText, value); }
        }

        public TurnTakingPageViewModel()
		{
			Title = "TurnTaking";
		    TurnTakingService = DependencyService.Get<ITurnTakingService>();
            TurnTakingService.OnTurnTakingUpdate += UpdateTurnTakingResult;
            TurnTakingButtonCommand = new Command(ToggleRecord);
		}

		/// <summary>
		/// Command to open browser to xamarin.com
		/// </summary>
		public ICommand TurnTakingButtonCommand { get; }

	    private void ToggleRecord()
	    {
	        if (!TurnTakingService.IsRunning())
	        {
	            ButtonText = "Stop Turntaking";
                TurnTakingService.Start(_groupName);
	        }
	        else
	        {
                ButtonText = "Start Turntaking";
                TurnTakingService.Stop();
	        }
	    }

	    private void UpdateTurnTakingResult(MeasurementMessage model, bool isMe)
	    {            
	        var upscaledVolumen = (model.ReportedSpeechEnergy * 1000);
            SpeachEnergy = upscaledVolumen.ToString("N4");

	        Origin = isMe ? "me" : model.CustomIdentifier;
	    }
	}
}
