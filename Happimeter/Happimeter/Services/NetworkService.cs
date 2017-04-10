using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Happimeter.Extensions;
using Happimeter.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.NetworkService))]
namespace Happimeter.Services
{
    public class NetworkService : IDisposable, INetworkService
    {
        private UdpClient UdpClient { get; set; }
        private readonly IList<IPEndPoint> _ipEndPoints = new List<IPEndPoint>();
        private readonly ConcurrentQueue<TurnTakingMessage> _sendMessageQueue = new ConcurrentQueue<TurnTakingMessage>();
        private readonly ConcurrentQueue<TurnTakingMessage> _receiveMessageQueue = new ConcurrentQueue<TurnTakingMessage>();
        private bool _isRunning = false;
        private CancellationTokenSource CancelationTokenSource {get; set; }


        public NetworkService()
        {
            UdpClient = new UdpClient(15000) {EnableBroadcast = true};
            _ipEndPoints.Add(new IPEndPoint(IPAddress.Broadcast, 15000));
        }

        private void Initialize(CancellationToken token)
        {
            Task.Factory.StartNew(async () => { await ListenOnNetwork(token); }, token);
            Task.Factory.StartNew(async () => { await BroadcastToNetwork(token); }, token);
        }
        
        private async Task ListenOnNetwork(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    var result = await UdpClient.ReceiveAsync().WithCancellation(token);
                    TryParseNetworkTraffic(result);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        private async Task BroadcastToNetwork(CancellationToken token)
        {
            while (true)
            {
                TurnTakingMessage messageToSend;
                while (_sendMessageQueue.TryDequeue(out messageToSend))
                {
                    try
                    {
                        foreach (var ipEndPoint in _ipEndPoints)
                        {
                            var toSendString = JsonConvert.SerializeObject(messageToSend);
                            var toSendByteArr = Encoding.ASCII.GetBytes(toSendString);
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            await UdpClient.SendAsync(toSendByteArr, toSendByteArr.Length, ipEndPoint)
                                .WithCancellation(token);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void TryParseNetworkTraffic(UdpReceiveResult result)
        {
            try
            {
                var resultString = Encoding.ASCII.GetString(result.Buffer).Trim();
                if (!(resultString.StartsWith("{") && resultString.EndsWith("}")) || //For object
                    (!resultString.StartsWith("[") && resultString.EndsWith("]"))) //For array
                {
                    return;
                }
                var retrievedData = JsonConvert.DeserializeObject<TurnTakingMessage>(resultString);
                _receiveMessageQueue.Enqueue(retrievedData);
            }
            catch (JsonException)
            {
                return;
            }
        }

        public IList<TurnTakingMessage> GetMessages()
        {
            var toReturnList = new List<TurnTakingMessage>();
            lock (_receiveMessageQueue)
            {
                TurnTakingMessage message;
                while (_receiveMessageQueue.TryDequeue(out message))
                {
                    toReturnList.Add(message);
                }
            }
            return toReturnList;
        }

        public void SendMessages(TurnTakingMessage message)
        {
            _sendMessageQueue.Enqueue(message);
        }

        public void Dispose()
        {
            UdpClient?.Dispose();
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

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
