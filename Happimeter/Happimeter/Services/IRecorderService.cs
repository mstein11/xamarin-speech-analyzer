using System;
using System.Collections.Generic;
using System.Text;

namespace Happimeter.Services
{
    public interface IRecorderService
    {
        bool Initialize();
        bool IsRunning();
        void SetOutputFormat(string format);
        void SetAudioEncoder(string encoder);
        void SetOutputPath(string path);

        bool Start();
        byte[] Stop();


    }
}
