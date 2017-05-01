using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Data;
using Happimeter.Shared;
using Newtonsoft.Json;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.NetworkService))]
namespace Happimeter.Services
{
    public class NetworkService : IDisposable, INetworkService
    {
        private HttpClient HttpClient { get; set; }

        private string GroupName { get; set; }
        
        private readonly ConcurrentQueue<MeasurementMessage> _sendMessageQueue = new ConcurrentQueue<MeasurementMessage>();
        private readonly ConcurrentQueue<MeasurementMessage> _receiveMessageQueue = new ConcurrentQueue<MeasurementMessage>();
        private bool _isRunning;
        private CancellationTokenSource CancelationTokenSource {get; set; }


        public NetworkService()
        {
            HttpClient = new HttpClient();
        }

        private void Initialize(CancellationToken token)
        {
            Task.Factory.StartNew(() => { RefreshDataWithServer(token); }, token);
        }

        private void RefreshDataWithServer(CancellationToken token)
        {
            while (true)
            {
                Thread.Sleep(10000);
                var messagesToSent = new List<MeasurementMessage>();
                MeasurementMessage messageToSent;
                while (_sendMessageQueue.TryDequeue(out messageToSent))
                {
                    messageToSent.TurnTakingGroupName = GroupName;
                    messagesToSent.Add(messageToSent);
                }
                
                var fromDatabaseToSend = GetFromDatabase();
                messagesToSent.AddRange(fromDatabaseToSend);

                if (messagesToSent.Count == 0)
                {
                    continue;
                }

                var json = JsonConvert.SerializeObject(messagesToSent);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var result =
                        HttpClient.PostAsync(
                                "http://happimeter-server.azurewebsites.net/api/measurement/ReportAndGetForGroup", content, token)
                            .Result;
                    TryParseNetworkTraffic(result);
                }
                catch (Exception e)
                {
                    var measurementPoints = messagesToSent.Select(x => new MeasurementPoint
                    {
                        CustomIdentifier = x.CustomIdentifier,
                        IsTurnTaking = x.IsTurnTaking,
                        IsSpeech = x.IsSpeech,
                        MeasurementTakenAtUtc = x.MeasurementTakenAtUtc,
                        ReportedSpeechEnergy = x.ReportedSpeechEnergy,
                        TurnTakingGroupName = x.TurnTakingGroupName
                    });
                    foreach (var measurementPoint in measurementPoints)
                    {
                        var result = App.Database?.SaveItemAsync(measurementPoint).Result;
                    }
                }
            }
        }

        private IList<MeasurementMessage> GetFromDatabase()
        {
            var fromDatabase =
                App.Database.GetEntitesAsync<MeasurementPoint>(1000).Result;

            foreach (var measurementPoint in fromDatabase)
            {
                var result = App.Database.DeleteItemAsync(measurementPoint).Result;
            }

            return fromDatabase.Select(x => new MeasurementMessage
            {
                CustomIdentifier = x.CustomIdentifier,
                IsTurnTaking = x.IsTurnTaking,
                IsSpeech = x.IsSpeech,
                MeasurementTakenAtUtc = x.MeasurementTakenAtUtc,
                ReportedSpeechEnergy = x.ReportedSpeechEnergy,
                TurnTakingGroupName = x.TurnTakingGroupName
            }).ToList();
        }

        private void TryParseNetworkTraffic(HttpResponseMessage result)
        {
            try
            {
                var resultString = result.Content.ReadAsStringAsync().Result;
                var retrievedData = JsonConvert.DeserializeObject<MeasurementResponse>(resultString);
                if (!retrievedData.HasTurnTaking) return;
                foreach (var turnTakingMessage in retrievedData.AllLatestMessages)
                {
                    _receiveMessageQueue.Enqueue(turnTakingMessage);
                }
            }
            catch (JsonException)
            {
                return;
            }
        }

        public IList<MeasurementMessage> GetMessages()
        {
            var toReturnList = new List<MeasurementMessage>();
            lock (_receiveMessageQueue)
            {
                MeasurementMessage message;
                while (_receiveMessageQueue.TryDequeue(out message))
                {
                    toReturnList.Add(message);
                }
            }
            return toReturnList;
        }

        public void SendMessages(MeasurementMessage message)
        {
            _sendMessageQueue.Enqueue(message);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }

        public void Start(string groupName = null)
        {
            if (_isRunning)
            {
                return;
            }

            GroupName = groupName;

            _isRunning = true;
            CancelationTokenSource = new CancellationTokenSource();
            Initialize(CancelationTokenSource.Token);
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            CancelationTokenSource.Cancel();
            _isRunning = false;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
    }
}
