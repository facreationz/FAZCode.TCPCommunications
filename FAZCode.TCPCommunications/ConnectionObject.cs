using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FAZCode.TCPCommunications
{
    public class ConnectionObject
    {
        public int ConnectionSerial { get; internal set; }
        public string ConnectionId { get; internal set; }
        public DateTime ConnectedOn { get; internal set; }
        public TcpClient TcpClient { get; internal set; }

        public long BytesReceived { get; internal set; }
        public long BytesSent { get; internal set;}

    }
}
