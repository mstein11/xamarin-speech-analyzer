using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Happimeter.Services.AudioAnalyzerService))]
namespace Happimeter.Droid
{
    public class FileHelper : IFileHelper
    {
        public string GetLocalFilePath(string filename)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(path, filename);
        }
    }
}