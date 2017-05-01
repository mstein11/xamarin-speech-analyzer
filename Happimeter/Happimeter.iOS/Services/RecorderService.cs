using System;
using System.Diagnostics;
using AudioToolbox;
using Foundation;
using Happimeter.Models;
using Happimeter.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.iOS.Services.AudioRecorder))]
namespace Happimeter.iOS.Services
{
    public class AudioRecorder : IRecorderService
    {
        private int _bufferCount = 3;
        private int _sampleRate = 8000;
        private int _audioBufferSize = 8000;

        private DateTime LastDateTime = DateTime.MinValue;

        private Action<byte[]> Callback { get; set; }
        private InputAudioQueue AudioQueue { get; set; }
        private bool _isRecord;

        public bool Initialize(Action<byte[]> callback = null)
        {
            Callback = callback;
            var recordFormat = new AudioStreamBasicDescription()
            {
                SampleRate = _sampleRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1,
                BitsPerChannel = 16,
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0,
            };

                AudioQueue = new InputAudioQueue(recordFormat);


            AudioQueue.InputCompleted += HandleInputCompleted;
            var bufferByteSize = _audioBufferSize * recordFormat.BytesPerPacket;

            for (var count = 0; count < _bufferCount; count++)
            {
                IntPtr bufferPointer;
                AudioQueue.AllocateBuffer(bufferByteSize, out bufferPointer);
                AudioQueue.EnqueueBuffer(bufferPointer, _audioBufferSize, null);
            }

            return true;
        }

        private void HandleInputCompleted(object sender, InputCompletedEventArgs e)
        {
            if (!_isRecord)
            {
                return;
            }

            var buffer = (AudioQueueBuffer)System.Runtime.InteropServices.Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
            if (OnReceiveSampleEvent != null)
            {
                var send = new byte[buffer.AudioDataByteSize];
                System.Runtime.InteropServices.Marshal.Copy(buffer.AudioData, send, 0, (int) buffer.AudioDataByteSize);

                var curretnTime = DateTime.UtcNow;
                LastDateTime = curretnTime;

                var model = new RecordingSampleModel
                {
                    AudioData = send,
                    TimeStamp = DateTime.UtcNow
                };

                OnReceiveSampleEvent?.Invoke(model);
            }

            var status = AudioQueue.EnqueueBuffer(e.IntPtrBuffer, _audioBufferSize, e.PacketDescriptions);
            if (status != AudioQueueStatus.Ok)
            {
                // todo: 
            }
        }

        public event Action<RecordingSampleModel> OnReceiveSampleEvent;

        public bool Start()
        {
            if (AudioQueue == null || _isRecord)
            {
                return false;
            }

            Initialize();
            var status = AudioQueue.Start();

            if (status == AudioQueueStatus.Ok)
            {
                _isRecord = true;
            }
            
            return _isRecord;
        }

        public void Stop()
        {
            if (AudioQueue == null || !_isRecord)
            {
                return;
            }

            //leave time to flush buffer, not stop immediatly
            AudioQueue.Stop(false);
            _isRecord = false;

        }

        public bool IsRunning()
        {
            return _isRecord;
        }
    }
}