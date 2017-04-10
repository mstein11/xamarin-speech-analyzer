using System;
using System.Threading.Tasks;
using Android.Media;
using Happimeter.Droid.Services;
using Happimeter.Models;
using Happimeter.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorderService))]
namespace Happimeter.Droid.Services
{
    public class AudioRecorderService : IRecorderService
    {
        private const int SampleRate = 8000; // 44100 for music
        private const ChannelIn Channel = ChannelIn.Mono;
        private const Encoding Encoding = Android.Media.Encoding.Pcm16bit;
        private readonly int _minBufSize = 8000 * 2; //AudioRecord.GetMinBufferSize(SampleRate,Channel, Encoding);


        private AudioRecord _recorder;
        private bool _isRecord;

        private Action<byte[]> Callback { get; set; }

        public bool Initialize(Action<byte[]> callback = null)
        {
            Callback = callback;
            _recorder = new AudioRecord(AudioSource.Mic,SampleRate,Channel,Encoding,_minBufSize);
            return true;
        }


        public bool Start()
        {
            if (_recorder == null || _isRecord)
            {
                return false;
            }

            if (_recorder.State == State.Uninitialized)
            {

            }

            _recorder.StartRecording();
            _isRecord = true;
            NotifyCallback();
            return true;
        }

        private void NotifyCallback()
        {
            Task.Factory.StartNew(() =>
            {
                byte[] buffer = new byte[_minBufSize];
                while (_isRecord)
                {
                    var data = _recorder.Read(buffer, 0, buffer.Length);
                    if (data <= 0)
                    {
                        break;
                    }

                    //only call Callback if not null
                    Callback?.Invoke(buffer);
                    var timeStamp = DateTime.UtcNow;
                    var model = new RecordingSampleModel
                    {
                        AudioData = buffer,
                        TimeStamp = timeStamp
                    };


                    OnReceiveSampleEvent?.Invoke(model);
                }
            });
        }

        public event Action<RecordingSampleModel> OnReceiveSampleEvent;

        public void Stop()
        {
            if (_recorder == null || !_isRecord)
            {
                return;
            }

            _recorder.Stop();
            _isRecord = false;
        }

        public bool IsRunning()
        {
            return _isRecord;
        }
    }
}