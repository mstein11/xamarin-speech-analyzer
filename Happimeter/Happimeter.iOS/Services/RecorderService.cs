using System;
using System.IO;
using AVFoundation;
using Foundation;
using Happimeter.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.iOS.Services.AudioRecorder))]
namespace Happimeter.iOS.Services
{
    public class AudioRecorder : IRecorderService
    {
        private NSDictionary _settings;
        private string _path;
        private NSUrl _url;
        private NSError _error;
        private AVAudioRecorder _recorder;
        private bool _isRecord;
        private AVAudioSession Session { get; set; }

        public bool Initialize()
        {
            Session = AVAudioSession.SharedInstance();
            var err = Session.SetCategory(AVAudioSessionCategory.Record);

            if (err != null)
            {
                Console.WriteLine("audioSession: {0}", err);

            }
            err = Session.SetActive(true);
            if (err != null)
            {
                Console.WriteLine("audioSession: {0}", err);

            }

            //Declare string for application temp path and tack on the file extension
            


            //set up the NSObject Array of values that will be combined with the keys to make the NSDictionary
            NSObject[] values = new NSObject[]
            {
                NSNumber.FromFloat (44100.0f), //Sample Rate
                NSNumber.FromInt32 ((int)AudioToolbox.AudioFormatType.LinearPCM), //AVFormat
                NSNumber.FromInt32 (2), //Channels
                NSNumber.FromInt32 (16), //PCMBitDepth
                NSNumber.FromBoolean (false), //IsBigEndianKey
                NSNumber.FromBoolean (false) //IsFloatKey
            };

            //Set up the NSObject Array of keys that will be combined with the values to make the NSDictionary
            NSObject[] keys = new NSObject[]
            {
                AVAudioSettings.AVSampleRateKey,
                AVAudioSettings.AVFormatIDKey,
                AVAudioSettings.AVNumberOfChannelsKey,
                AVAudioSettings.AVLinearPCMBitDepthKey,
                AVAudioSettings.AVLinearPCMIsBigEndianKey,
                AVAudioSettings.AVLinearPCMIsFloatKey
            };

            //Set Settings with the Values and Keys to create the NSDictionary
            _settings = NSDictionary.FromObjectsAndKeys(values, keys);
            SetOutputPath();
            //Set recorder parameters
            _recorder = AVAudioRecorder.Create(_url, new AudioSettings(_settings), out _error);

            //Set Recorder to Prepare To Record
            var tmp = _recorder.PrepareToRecord();

            return tmp;
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
            if (path.Length == 0)
            {
                path = Path.GetTempPath();
            }

            string fileName = string.Format("audio{0}.wav", DateTime.Now.ToString("yyyyMMddHHmmss"));
            string audioFilePath = Path.Combine(path, fileName);

            Console.WriteLine("Audio File Path: " + audioFilePath);
            _path = audioFilePath;
            _url = NSUrl.FromFilename(audioFilePath);
        }

        public bool Start()
        {
            if (_recorder == null || _isRecord)
            {
                return false;
            }

            _recorder.Record();
            _isRecord = true;
            return true;
        }

        public byte[] Stop()
        {
            if (_recorder == null || !_isRecord)
            {
                return default(byte[]);
            }

            _recorder.Stop();
            _isRecord = false;


            byte[] bytes;
            using (var streamReader = new StreamReader(_path))
            {
                using (var memstream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }
            }

            File.Delete(_path);

            return bytes;

        }

        public bool IsRunning()
        {
            return _isRecord;
        }
    }
}