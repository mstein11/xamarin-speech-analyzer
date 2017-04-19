using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Models;
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
            //UdpClient = new UdpClient(15000) {EnableBroadcast = true};
            //_ipEndPoints.Add(new IPEndPoint(IPAddress.Broadcast, 15000));
            HttpClient = new HttpClient();
        }

        private void Initialize(CancellationToken token)
        {
            //Task.Factory.StartNew(async () => { await ListenOnNetwork(token); }, token);
            //Task.Factory.StartNew(async () => { await BroadcastToNetwork(token); }, token);
            Task.Factory.StartNew(() => { RefreshDataWithServer(token); }, token);
        }

        private void RefreshDataWithServer(CancellationToken token)
        {
            while (true)
            {
                Thread.Sleep(100);
                var messagesToSent = new List<MeasurementMessage>();
                MeasurementMessage messageToSent;
                while (_sendMessageQueue.TryDequeue(out messageToSent))
                {
                    messageToSent.TurnTakingGroupName = GroupName;
                    messagesToSent.Add(messageToSent);
                }

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
                    Debug.WriteLine(e.Message);
                }
                
                
            }
        }

        //private async Task ListenOnNetwork(CancellationToken token)
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            var result = await UdpClient.ReceiveAsync().WithCancellation(token);
        //            TryParseNetworkTraffic(result);
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            break;
        //        }
        //        Thread.Sleep(10);
        //    }
        //}

        //private async Task BroadcastToNetwork(CancellationToken token)
        //{
        //    while (true)
        //    {
        //        TurnTakingMessage messageToSend;
        //        while (_sendMessageQueue.TryDequeue(out messageToSend))
        //        {
        //            try
        //            {
        //                foreach (var ipEndPoint in _ipEndPoints)
        //                {
        //                    var toSendString = JsonConvert.SerializeObject(messageToSend);
        //                    var toSendByteArr = Encoding.ASCII.GetBytes(toSendString);
        //                    if (token.IsCancellationRequested)
        //                    {
        //                        return;
        //                    }
        //                    await UdpClient.SendAsync(toSendByteArr, toSendByteArr.Length, ipEndPoint)
        //                        .WithCancellation(token);
        //                }
        //            }
        //            catch (TaskCanceledException)
        //            {
        //                return;
        //            }
        //        }
        //        Thread.Sleep(10);
        //    }
        //}

        //private void TryParseNetworkTraffic(UdpReceiveResult result)
        //{
        //    try
        //    {
        //        var resultString = Encoding.ASCII.GetString(result.Buffer).Trim();
        //        if (!(resultString.StartsWith("{") && resultString.EndsWith("}")) || //For object
        //            (!resultString.StartsWith("[") && resultString.EndsWith("]"))) //For array
        //        {
        //            return;
        //        }
        //        var retrievedData = JsonConvert.DeserializeObject<TurnTakingMessage>(resultString);
        //        _receiveMessageQueue.Enqueue(retrievedData);
        //    }
        //    catch (JsonException)
        //    {
        //        return;
        //    }
        //}

        private void TryParseNetworkTraffic(HttpResponseMessage result)
        {
            try
            {
                var resultString = result.Content.ReadAsStringAsync().Result;
                Debug.WriteLine(resultString);
                var retrievedData = JsonConvert.DeserializeObject<List<MeasurementMessage>>(resultString);
                foreach (var turnTakingMessage in retrievedData)
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
