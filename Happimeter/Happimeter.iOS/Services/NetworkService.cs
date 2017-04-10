using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Happimeter.iOS.Services;
using Happimeter.Services;
using Xamarin.Forms;

namespace Happimeter.iOS.Services
{

    public class NetworkService
    { 
        private UdpClient _udpClient = new UdpClient(15000);
        public async Task<List<string>> ScanNetwork()
        {
            using (var client = new UdpClient())
            {
                client.EnableBroadcast = true;
                var endpoint = new IPEndPoint(IPAddress.Broadcast, 15000);
                var message = Encoding.ASCII.GetBytes("Hello World - " + DateTime.Now.ToString());
                await client.SendAsync(message, message.Length, endpoint);
                client.Close();
            }
        

            return new List<string>();
        }

        private void ListenOnNetwork()
        {
            
        }

        public void Dispose()
        {
        }
    }
}