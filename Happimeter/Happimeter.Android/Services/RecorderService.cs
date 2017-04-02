using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Android.Media;
using Happimeter.Droid.Services;
using Happimeter.Services;
using Xamarin.Forms;
using Object = System.Object;

[assembly: Dependency(typeof(AudioRecorderService))]
namespace Happimeter.Droid.Services
{
    public class AudioRecorderService : IRecorderService
    {
        private static readonly Object _lock = new Object();
        private const int SampleRate = 44100; // 44100 for music
        private const ChannelIn Channel = ChannelIn.Mono;
        private const Encoding Encoding = Android.Media.Encoding.Pcm16bit;
        private readonly int _minBufSize = 44100 * 2; //AudioRecord.GetMinBufferSize(SampleRate,Channel, Encoding);

        private ConcurrentBag<byte> BytesRead = new ConcurrentBag<byte>();

        private AudioRecord _recorder;
        private bool _isRecord;

        private Action<byte[]> Callback { get; set; }

        public bool Initialize(Action<byte[]> callback = null)
        {
            Callback = callback;
            _recorder = new AudioRecord(AudioSource.Mic,SampleRate,Channel,Encoding,_minBufSize);
            return true;
        }

        public void SetOutputFormat(string format)
        {
            throw new System.NotImplementedException();
        }

        public void SetAudioEncoder(string encoder)
        {
            throw new System.NotImplementedException();
        }

        public void SetOutputPath(string path = "")
        {

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
                    lock (_lock)
                    {
                        foreach (var b in buffer)
                        {
                            BytesRead.Add(b);
                        }
                    }
                }
            });
        }

        public byte[] Stop()
        {
            if (_recorder == null || !_isRecord)
            {
                return default(byte[]);
            }

            _recorder.Stop();
            _isRecord = false;

            byte[] byteArr;
            lock (_lock)
            {
                byteArr = BytesRead.ToArray();
                BytesRead = new ConcurrentBag<byte>();
            }

            return byteArr;
        }

        public bool IsRunning()
        {
            return _isRecord;
        }
    }
}