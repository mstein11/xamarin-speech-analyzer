using System;
using Happimeter.Models;
using Happimeter.Shared;

namespace Happimeter.Services
{
    public interface ITurnTakingService
    {
        event Action<MeasurementMessage, bool> OnTurnTakingUpdate;
        void Start(string groupName = null);
        void Stop();
        bool IsRunning();
    }
}