using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Models;
using Happimeter.Shared;
using Happimeter.Shared.DataStructures;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.TurnTakingService))]
namespace Happimeter.Services
{
    public class TurnTakingService : ITurnTakingService
    {
        private IAudioAnalyzerService AudioAnalyzer { get; set; }
        private INetworkService NetworkService { get; set; }

        private IDictionary<string, SlidingBuffer<MeasurementMessage>> _fromNetwork =
            new Dictionary<string, SlidingBuffer<MeasurementMessage>>();

        private SlidingBuffer<MeasurementMessage> _mine = new SlidingBuffer<MeasurementMessage>(60);
        private bool _isRunning = false;

        private string _currentIpAddress = Guid.NewGuid().ToString();

        public TurnTakingService()
        {
            AudioAnalyzer = DependencyService.Get<AudioAnalyzerService>();
            NetworkService = DependencyService.Get<INetworkService>();

            //_currentIpAddress = App.GetCurrentUser().Result.Email;

            AudioAnalyzer.OnProcessAudioUpdate += ReceiveFromAnalyzer;
            
        }

        private void ReceiveFromAnalyzer(AnalyzedAudioModel model)
        {
            if (!IsRunning())
            {
                return;
            }
            var turnTakingModel = new MeasurementMessage
            {
                MeasurementTakenAtUtc = model.TimeStamp,
                Id = Guid.NewGuid(),
                ReportedSpeechEnergy = model.SpeechEnergyLastSample,
                CustomIdentifier = _currentIpAddress,
            };
            NetworkService.SendMessages(turnTakingModel);
            _mine.Add(turnTakingModel);
        }

        private void GetFromNetwork()
        {
            _fromNetwork = new ConcurrentDictionary<string, SlidingBuffer<MeasurementMessage>>();
            var messagesGrouped = NetworkService.GetMessages().GroupBy(x => x.CustomIdentifier);

            foreach (var messageGroup in messagesGrouped.ToList())
            {
                if (!_fromNetwork.ContainsKey(messageGroup.Key))
                {
                    _fromNetwork.Add(messageGroup.Key,new SlidingBuffer<MeasurementMessage>(60));
                }

                foreach (var turnTakingMessage in messageGroup)
                {
                    _fromNetwork[messageGroup.Key].Add(turnTakingMessage);
                }
            }
        }

        private void CalculateLoudest()
        {
            while (true)
            {
               
                GetFromNetwork();

                var referenceTime = DateTime.UtcNow;



                var fromNetworkClosestToReferenceTime = new List<MeasurementMessage>();
                //todo: this does not take into account that a client might be once supper loud and then stops transmission
                foreach (var clientsMessages in _fromNetwork.Keys)
                {
                    var clientsClosest =
                        _fromNetwork[clientsMessages].Select(x => new {timeDiff = referenceTime - x.MeasurementTakenAtUtc, model = x})
                            .OrderBy(x => x.timeDiff).FirstOrDefault();
                    if (clientsClosest == null)
                    {
                        continue;
                    }
                    fromNetworkClosestToReferenceTime.Add(clientsClosest.model);
                }

                var loudest = fromNetworkClosestToReferenceTime.OrderByDescending(x => x.ReportedSpeechEnergy).FirstOrDefault();

                if (loudest == null)
                {
                    continue;
                }

                OnTurnTakingUpdate?.Invoke(loudest,loudest.CustomIdentifier == _currentIpAddress);

                Thread.Sleep(10);
            }
        }


        public event Action<MeasurementMessage, bool> OnTurnTakingUpdate;

        public void Start(string groupName = null)
        {
            if (!AudioAnalyzer.IsRunning())
            {
                AudioAnalyzer.Start();
            }

            if (!NetworkService.IsRunning())
            {
                NetworkService.Start(groupName);
            }

            Task.Factory.StartNew(CalculateLoudest);
            _isRunning = true;
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

            _isRunning = false;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
    }
}
