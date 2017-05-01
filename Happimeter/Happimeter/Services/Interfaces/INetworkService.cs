using System.Collections.Generic;
using System.Threading;
using Happimeter.Models;
using Happimeter.Shared;

namespace Happimeter.Services
{
    public interface INetworkService
    {
        IList<MeasurementMessage> GetMessages();
        void SendMessages(MeasurementMessage message);
        void Dispose();

        void Start(string groupName = null);
        void Stop();
        bool IsRunning();
    }
}