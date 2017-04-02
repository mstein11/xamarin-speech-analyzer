using System;
using System.Collections.Generic;
using System.Text;

namespace Happimeter.Services
{
    public interface IRecorderService
    {
        bool Initialize(Action<byte[]> callback = null);
        bool IsRunning();

        bool Start();
        void Stop();
    }
}
