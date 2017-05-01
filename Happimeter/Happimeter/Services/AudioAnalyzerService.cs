using System;
using System.Diagnostics;
using System.Linq;
using Happimeter.Data;
using Happimeter.Models;
using Happimeter.Shared.DataStructures;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.AudioAnalyzerService))]
namespace Happimeter.Services
{
    public class AudioAnalyzerService : IAudioAnalyzerService
    {
        private SlidingBuffer<byte[]> _buffer = new SlidingBuffer<byte[]>(60);
        private IRecorderService RecorderService { get; }

        public AudioAnalyzerService()
        {
            RecorderService = DependencyService.Get<IRecorderService>();
            RecorderService.Initialize();
            RecorderService.OnReceiveSampleEvent += ProcessAudio;
        }

        private async void ProcessAudio(RecordingSampleModel inputData)
        {
            var data = inputData.AudioData;
            _buffer.Add(data);

            
            var outputModel = new AnalyzedAudioModel
            {
                SpeechEnergyLastSample = CalculateVolumeForData(data),
                SpeechEnergyLastMinute = CalculateVolumeForData(_buffer.SelectMany(x => x).ToArray()),
                TimeStamp = inputData.TimeStamp
            };

            OnProcessAudioUpdate?.Invoke(outputModel);
        }

        private double CalculateVolumeForData(byte[] data)
        {
            long totalSquare = 0;
            for (var i = 0; i < data.Length; i += 2)
            {
                var sample = (short)(data[i] | (data[i + 1] << 8));
                totalSquare += sample * sample;
            }
            double meanSquare = 2 * totalSquare / data.Length;
            var rms = Math.Sqrt(meanSquare);
            var volume = rms / 32768.0;

            return volume;
        }

        public void Start()
        {
            RecorderService.Start();
        }

        public void Stop()
        {
            RecorderService.Stop();
            _buffer = new SlidingBuffer<byte[]>(60);
        }

        public bool IsRunning()
        {
            return RecorderService.IsRunning();
        }

        public event Action<AnalyzedAudioModel> OnProcessAudioUpdate;

    }
}
