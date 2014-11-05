using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Data.Xml.Dom;

namespace HueAPI
{
    struct BridgeDescription
    {

    }

    public class BridgeConnection
    {
        private HttpClient _client = new HttpClient();
        
        public async Task DiscoverBridge(TimeSpan timeOut)
        {
            var multicastIP = new HostName("239.255.255.250");
            var foundBridge = false;

            using (var socket = new DatagramSocket())
            {
                socket.MessageReceived += async (sender, args) =>
                {
                    var reader = args.GetDataReader();
                    var bytesRemaining = reader.UnconsumedBufferLength;
                    foreach (var line in reader.ReadString(bytesRemaining).Split(new String[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.StartsWith("LOCATION"))
                        {
                            var address = line.Split(new char[1] { ':' }, 2)[1];
                            var message = await _client.GetAsync(address);
                            
                            if (message.IsSuccessStatusCode)
                            {
                                var messageContent = await message.Content.ReadAsStringAsync();
                                var buffer = Encoding.UTF8.GetBytes(messageContent);
                                
                                
                            }
                        }
                    }

                    foundBridge = true;
                };

                await socket.BindEndpointAsync(null, string.Empty);
                socket.JoinMulticastGroup(multicastIP);

                while(true)
                {
                    foundBridge = false;

                    using (var stream = await socket.GetOutputStreamAsync(multicastIP, "1900"))
                    using (var writer = new DataWriter(stream))
                    {
                        var request = new StringBuilder();
                        request.AppendLine("M-SEARCH * HTTP/1.1");
                        request.AppendLine("HOST: 239.255.255.250:1900");
                        request.AppendLine("MAN: ssdp:discover");
                        request.AppendLine("MX: 3");
                        request.AppendLine("ST: ssdp:all");

                        writer.WriteString(request.ToString());
                        await writer.StoreAsync();

                        if (timeOut > TimeSpan.Zero)
                        {
                            await Task.Delay(timeOut);
                        }
                            
                        if (foundBridge)
                        {
                            break;
                        }
                    }
                }
            }

        }
        
    }
}
