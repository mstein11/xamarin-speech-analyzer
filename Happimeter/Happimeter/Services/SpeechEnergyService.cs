using System;
using System.Diagnostics;
using Happimeter.Models;
using Happimeter.Shared;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.SpeechEnergyService))]
namespace Happimeter.Services
{
    public class SpeechEnergyService : ISpeechEnergyService
    {
        public INetworkService NetworkService { get; set; }

        public AudioAnalyzerService AudioAnalyzer { get; set; }

        private string CustomIdentifier { get; set; }

        public SpeechEnergyService()
        {
            AudioAnalyzer = DependencyService.Get<AudioAnalyzerService>();
            NetworkService = DependencyService.Get<INetworkService>();
            AudioAnalyzer.OnProcessAudioUpdate += OnAudioAnalyzerUpdate;
        }

        public void AddOnProcessAudioUpdate(Action<AnalyzedAudioModel> function)
        {
            AudioAnalyzer.OnProcessAudioUpdate += function;
        }

        private async void OnAudioAnalyzerUpdate(AnalyzedAudioModel model)
        {
            if (!IsRunning)
            {
                return;
            }

            if (true)
            {
                //if app is in foreground and internet connection is sufficient
                var measurementModel = new MeasurementMessage
                {
                    MeasurementTakenAtUtc = model.TimeStamp,
                    Id = Guid.NewGuid(),
                    ReportedSpeechEnergy = model.SpeechEnergyLastSample,
                    CustomIdentifier = CustomIdentifier,
                    IsSpeech = true,
                    IsTurnTaking = false
                };
                
                NetworkService.SendMessages(measurementModel);
            }
            else
            {
                //if app is in background or internet connection is insufficient

            }
        }

        public void Start(string customIdentifier = null)
        {
            CustomIdentifier = customIdentifier;
            if (!AudioAnalyzer.IsRunning())
            {
                AudioAnalyzer.Start();
            }

            if (!NetworkService.IsRunning())
            {
                NetworkService.Start();
            }

            IsRunning = true;
        }

        public void Stop()
        {
            if (AudioAnalyzer.IsRunning())
            {
                AudioAnalyzer.Stop();
            }

            if (NetworkService.IsRunning())
            {
                NetworkService.Stop();
            }

            IsRunning = false;
        }

        public bool IsRunning { get; set; }
    }
}
