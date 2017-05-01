using System;
using System.Collections.Generic;
using System.Text;
using Happimeter.Models;

namespace Happimeter.Services
{
    public interface IAudioAnalyzerService
    {
        event Action<AnalyzedAudioModel> OnProcessAudioUpdate;
        void Start();
        void Stop();
        bool IsRunning();
    }
}
