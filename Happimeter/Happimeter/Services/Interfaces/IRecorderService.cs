using System;
using System.Collections.Generic;
using System.Text;
using Happimeter.Models;

namespace Happimeter.Services
{
    public interface IRecorderService
    {
        bool Initialize(Action<byte[]> callback = null);
        bool IsRunning();

        event Action<RecordingSampleModel> OnReceiveSampleEvent;
        bool Start();
        void Stop();
    }
}
