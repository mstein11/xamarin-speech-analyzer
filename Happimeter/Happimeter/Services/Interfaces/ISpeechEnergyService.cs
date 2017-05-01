using System;
using Happimeter.Models;

namespace Happimeter.Services
{
    public interface ISpeechEnergyService
    {
        INetworkService NetworkService { get; set; }
        void AddOnProcessAudioUpdate(Action<AnalyzedAudioModel> function);
        AudioAnalyzerService AudioAnalyzer { get; set; }
        bool IsRunning { get; set; }
        void Start(string customIdentifier = null);
        void Stop();
    }
}