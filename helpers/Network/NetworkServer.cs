using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helpers.Network
{
    public class NetworkServer
    {
        public static NetworkServer Singleton { get; } = new NetworkServer();
    }
}