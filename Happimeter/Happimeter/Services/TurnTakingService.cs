﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.DataStructures;
using Happimeter.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.TurnTakingService))]
namespace Happimeter.Services
{
    public class TurnTakingService : ITurnTakingService
    {
        private IAudioAnalyzerService AudioAnalyzer { get; set; }
        private INetworkService NetworkService { get; set; }

        private IDictionary<string, SlidingBuffer<TurnTakingMessage>> _fromNetwork =
            new Dictionary<string, SlidingBuffer<TurnTakingMessage>>();

        private SlidingBuffer<TurnTakingMessage> _mine = new SlidingBuffer<TurnTakingMessage>(60);
        private bool _isRunning = false;

        private string _currentIpAddress = Guid.NewGuid().ToString();

        public TurnTakingService()
        {
            AudioAnalyzer = DependencyService.Get<AudioAnalyzerService>();
            NetworkService = DependencyService.Get<INetworkService>();

            AudioAnalyzer.OnProcessAudioUpdate += ReceiveFromAnalyzer;
            
        }

        private void ReceiveFromAnalyzer(AnalyzedAudioModel model)
        {
            var turnTakingModel = new TurnTakingMessage
            {
                AudioTimeStamp = model.TimeStamp,
                Id = Guid.NewGuid(),
                Volumne = model.SpeechEnergyLastSample,
                IpAdress = _currentIpAddress
            };
            NetworkService.SendMessages(turnTakingModel);
            _mine.Add(turnTakingModel);
        }

        private void GetFromNetwork()
        {
            var messagesGrouped = NetworkService.GetMessages().GroupBy(x => x.IpAdress);

            foreach (var messageGroup in messagesGrouped.ToList())
            {
                if (!_fromNetwork.ContainsKey(messageGroup.Key))
                {
                    _fromNetwork.Add(messageGroup.Key,new SlidingBuffer<TurnTakingMessage>(60));
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

                var mineClosest =
                    _mine.Select(x => new {timeDiff = referenceTime- x.AudioTimeStamp, model = x})
                        .OrderBy(x => x.timeDiff).FirstOrDefault();

                if (mineClosest == null)
                {
                    continue;
                }

                var fromNetworkClosestToReferenceTime = new List<TurnTakingMessage>();
                //todo: this does not take into account that a client might be once supper loud and then stops transmission
                foreach (var clientsMessages in _fromNetwork.Keys)
                {
                    var clientsClosest =
                        _fromNetwork[clientsMessages].Select(x => new {timeDiff = referenceTime - x.AudioTimeStamp, model = x})
                            .OrderBy(x => x.timeDiff).FirstOrDefault();
                    fromNetworkClosestToReferenceTime.Add(clientsClosest.model);
                }

                fromNetworkClosestToReferenceTime.Add(mineClosest.model);

                var loudest = fromNetworkClosestToReferenceTime.OrderByDescending(x => x.Volumne).FirstOrDefault();

                OnTurnTakingUpdate?.Invoke(loudest,loudest.IpAdress == _currentIpAddress);

                Thread.Sleep(10);
            }
        }


        public event Action<TurnTakingMessage, bool> OnTurnTakingUpdate;

        public void Start()
        {
            if (!AudioAnalyzer.IsRunning())
            {
                AudioAnalyzer.Start();
            }

            if (!NetworkService.IsRunning())
            {
                NetworkService.Start();
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