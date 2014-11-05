using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HueAPI;

namespace HueLiveTestDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            BridgeConnection connection = new BridgeConnection();
            var blah = connection.DiscoverBridge(TimeSpan.FromSeconds(1));
            blah.Wait();
        }
    }
}
