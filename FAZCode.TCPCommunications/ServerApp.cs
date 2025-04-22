using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FAZCode.TCPCommunications
{
    public class ServerApp : CommunicationsBase
    {

        public List<ConnectionObject> Connections { get; private set; } = new List<ConnectionObject>();
        public IPAddress Address { get; protected set; }

        private TcpListener listener;

        #region Constra

        public ServerApp()
        {
            Events = new EventHandlers();
            Events.Initialise();
        }

        public override void Dispose()
        {
            // Clean Up
            if (IsRunning) StopServer();
        }

        #endregion

        #region Server Management

        public void StartServer(IPAddress address, int port)
        {
            if (IsRunning) return;

            // Configure
            this.Address = address;
            this.Port = port;
            this.Connections = new List<ConnectionObject>();

            try
            {
                // Build server
                IPEndPoint ep = new IPEndPoint(address, port);
                listener = new TcpListener(ep); // Instantiate the object  

                // Start
                listener.Start();
                
                IsRunning = true;
                Events.OnServerStarted(this);            // raise event

                // Go into background loop
                AcceptConnections();
            }
            catch (Exception ex)
            {
                // Failed to start server. Probably the port is already in use by another application
                IsRunning = false;
                Events.OnServerError(this, ex);
            }

        }

        private async void AcceptConnections()
        {
            while (IsRunning) 
            {
                try
                {
                    // Look out for new connection
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    
                    // Create connection
                    ConnectionObject connection = new ConnectionObject()
                    {
                        ConnectionId = Guid.NewGuid().ToString(),
                        ConnectedOn = DateTime.Now,
                        TcpClient = client,
                        ClientEndPoint = client.Client.RemoteEndPoint as IPEndPoint,
                    };
                    Connections.Add(connection);                            // add to clients list
                    OnClientConnect(connection);                            // raise event

                    // Move the client into its own thread
                    ThreadPool.QueueUserWorkItem(ReadMessagesThredProc, connection);

                }
                catch (ObjectDisposedException)
                {
                    // This is normal behavior when a server is switched off.
                }
                catch (Exception ex)
                {
                    Events.OnServerError(this, ex);
                }

            }
        }

        public void StopServer()
        {
            if (!IsRunning) return;

            // Close listener
            IsRunning = false;
            listener.Server.Close();
            listener.Stop();
            listener = null;

            // Disconnect all active connections
            DisconnectAllClients();

            // Done
            Events.OnServerStopped(this);           // raise event
        }

        private void DisconnectAllClients()
        {
            foreach (var connection in Connections.ToList())    // make a duplicate list
            {
                // Remove it from the list
                Connections.Remove(connection);

                // Kill the connection
                OnDisconnectClient(connection);
                
            }
        }

        protected override void OnClientDisconnect(ConnectionObject connection)
        {
            // Remove the client from the connection list
            Connections.Remove(connection);

            // Base code
            base.OnClientDisconnect(connection);
        }


        #endregion

        #region Communication

        public async Task BroadcastMessageAsync(string message)
        {
            foreach (ConnectionObject connection in Connections)
            {
                await SendMessageAsync(connection, message);
            }
        }

        public async Task<bool> SendMessageAsync(ConnectionObject connection, string message)
        {
            return await OnSendMessageAsync(connection, message);
        }


        #endregion

    }
}
