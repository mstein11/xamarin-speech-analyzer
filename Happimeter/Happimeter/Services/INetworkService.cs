using System.Collections.Generic;
using System.Threading;
using Happimeter.Models;

namespace Happimeter.Services
{
    public interface INetworkService
    {
        IList<TurnTakingMessage> GetMessages();
        void SendMessages(TurnTakingMessage message);
        void Dispose();

        void Start();
        void Stop();
        bool IsRunning();
    }
}