using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FAZCode.TCPCommunications
{
    public class ClientApp : CommunicationsBase
    {
        public string Host { get; private set; }    

        private ConnectionObject connection;

        #region Constra

        public ClientApp()
        {
            CreateEventsHandler();
        }

        public ClientApp(string host, int port)
        {
            this.Host = host;
            this.Port = port;

            CreateEventsHandler();
        }

        public override void Dispose()
        {
            // Clean Up
            if (IsRunning) Disconnect();
        }

        private void CreateEventsHandler()
        {
            Events = new EventHandlers();
            Events.Initialise();
        }

        #endregion

        #region Client Management

        public async Task<bool> ConnectAsync()
        {
            if (IsRunning) return false;

            try
            {
                // Establish connection
                TcpClient client = new TcpClient();
                await client.ConnectAsync(Host, Port);

                // Connection successful
                IsRunning = true;
                connection = new ConnectionObject()
                {
                    ConnectionId = Guid.NewGuid().ToString(),
                    ConnectedOn = DateTime.Now,
                    TcpClient = client,
                };
                OnClientConnect(connection);                                            // raise event

            }
            catch (Exception ex)
            {
                // Could not connect
                OnClientError(connection, ex);                                                      // raise event
                return false;
            }

            // Enter communication loop
            ThreadPool.QueueUserWorkItem(ReadMessagesThredProc, connection);

            // Done
            return true;

        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            if (IsRunning) return false;

            // Validate
            if (string.IsNullOrEmpty(host)) throw new Exception("Hostname is not defined.");
            if (port == 0) throw new Exception("Port is not defined.");


            // Store these values
            this.Host = host;
            this.Port = port;

            // Perform connection
            return await ConnectAsync();

        }

        protected override void OnClientDisconnect(ConnectionObject connection)
        {
            IsRunning = false;
            base.OnClientDisconnect(connection);
        }

        public void Disconnect()
        {
            OnDisconnectClient(connection);
            //IsRunning = false;
        }

        #endregion

        #region Communication
       
        public async Task<bool> SendMessageAsync(string message)
        {
            return await OnSendMessageAsync(connection, message);
        }

        #endregion
    }
}
