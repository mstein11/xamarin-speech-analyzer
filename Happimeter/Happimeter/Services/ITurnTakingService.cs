using System;
using Happimeter.Models;

namespace Happimeter.Services
{
    public interface ITurnTakingService
    {
        event Action<TurnTakingMessage, bool> OnTurnTakingUpdate;
        void Start();
        void Stop();
        bool IsRunning();
    }
}