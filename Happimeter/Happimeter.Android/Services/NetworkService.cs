using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Happimeter.Droid.Services;
using Happimeter.Services;
using Xamarin.Forms;


namespace Happimeter.Droid.Services
{

    public class NetworkService
    {
        private List<string> FoundIps { get; set; }
        private readonly UdpClient _udpClient = new UdpClient(15000);
        public NetworkService()
        {
            Task.Factory.StartNew(RunUdpListener);
        }

        private async Task RunUdpListener()
        {
            while (true)
            {
                var result = await _udpClient.ReceiveAsync();
                var message = Encoding.ASCII.GetString(result.Buffer);
            }
        }

        public Task<List<string>> ScanNetwork()
        {
            //var start = 0;
            //var end = 255;
            //var timeout = 1000;


            //for (var i = 0; i < end; i++)
            //{
            //    var ping = new Ping();
            //    var ip = "192.168.2." + i;
            //    var reply = ping.Send(ip,timeout);
            //    if (reply.Status == IPStatus.Success)
            //    {
            //        FoundIps.Add(ip);
            //    }
            //}

            return Task.FromResult(new List<string>());
        }
    }
}