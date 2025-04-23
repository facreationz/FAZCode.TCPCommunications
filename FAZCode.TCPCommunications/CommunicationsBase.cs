using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FAZCode.TCPCommunications
{
    public abstract class CommunicationsBase : IDisposable
    {

        #region Properties, Events and Delegates

        // Public properties
        [DefaultValue("<!EOM!>")] public string EndOfLinesCharacters { get; set; } = "<!EOL!>";
        public bool IsRunning { get; internal set; }
        [DefaultValue(3306)] public int Port { get; set; }
        [DefaultValue(1048576)] public int ReceiveBufferSize { get; set; } = 1048576; // 1024 * 1024;
        public long TotalBytesSent { get; private set; }
        public long TotalBytesReceived { get; private set; }
        public EventHandlers Events { get; internal set; }

        #endregion

        #region Common Code

        public abstract void Dispose();

        protected void OnDisconnectClient(ConnectionObject connection)
        {
            if (connection != null)
            {

                // Kill the connection
                if (connection.TcpClient != null)
                {
                    connection.TcpClient.Close();
                    connection.TcpClient.Dispose();
                    connection.TcpClient = null;
                    //ClientDisconnectedCT?.Invoke(this, connection);           // Duplication. This is already raised through the ReadMessagesThredProc function.
                }
            
            }

        }

        #endregion

        #region Events Management

        protected virtual void OnClientConnect(ConnectionObject connection)
        {
            // raise event
            Events.OnClientConnected(this, connection);
        }

        protected virtual void OnClientDisconnect(ConnectionObject connection)
        {
            // Terminate the connection
            connection.TcpClient?.Close();

            Events.OnClientDisconnected(this, connection);  
        }

        protected virtual void OnClientError(ConnectionObject connection, Exception ex)
        {
            // raise event
            Events.OnClientError(this, connection, ex);
        }

        #endregion

        #region Communication

        protected void ReadMessagesThredProc(object obj)
        {
            // Get the connection object
            ConnectionObject connection = (ConnectionObject)obj;
            TcpClient client = connection.TcpClient;

            // Communication loop
            try
            {
                if (IsRunning && client != null && client.Client != null && client.Connected)
                {
                    // Get a stream object for reading and writing 
                    NetworkStream netStream = client.GetStream();
                    string dataBuffer = "";
                    do
                    {

                        // Reads NetworkStream into a byte buffer.
                        byte[] bytes = new byte[ReceiveBufferSize];

                        // Read can return anything from 0 to numBytesToRead. 
                        // This method blocks until at least one byte is read.
                        if (!netStream.CanRead) break;
                        int i = netStream.Read(bytes, 0, ReceiveBufferSize);
                        if (i == 0) break;      // client has disconnected

                        // Append to the data buffer
                        string newMessage = Encoding.UTF8.GetString(bytes, 0, i);
                        dataBuffer += newMessage;
                        
                        // Note the number of total bytes received for the server
                        TotalBytesReceived += newMessage.Length;

                        // process message buffering
                        do
                        {
                            if (newMessage.Length == 0) break;
                            
                            if (dataBuffer.Contains(EndOfLinesCharacters))
                            {
                                // Strip off this message
                                string message = dataBuffer.Substring(0, dataBuffer.IndexOf(EndOfLinesCharacters));
                                dataBuffer = dataBuffer.Substring(dataBuffer.IndexOf(EndOfLinesCharacters) + EndOfLinesCharacters.Length);

                                // Note bytes received for this connection
                                int bytesReceived = (message.Length + EndOfLinesCharacters.Length);
                                connection.BytesReceived += bytesReceived;

                                // Raise event
                                Events.OnMessageReceived(this, connection, message);       // raise event (cross-threaded)
                            }
                            else
                                break;
                        } while (true);


                    } while (true);

                    // Closing the tcpClient instance does not close the network stream.
                    netStream.Close();
                }


            }
            catch (IOException ex)
            {
                // client has disconnected in an improper way.
                // Or due to the same reason given below for ObjectDisposedException.
                _ = ex;
            }
            catch (ObjectDisposedException ex)
            {
                // this happens when the NetworkStream.Read is called just after the connection was already closed
                // Sometimes in split of a second the Tcp.Closed() is called before the Do-Loop reaches the
                // NetworkStream.Read line for going into blocking mode. This throws error that underlying
                // connection and stream is already closed. Ideally this should have been caught by NetworkStream.CanRead
                // but for some reasons it doesn't prevent this error from happening
                _ = ex;
            }
            catch (Exception ex)
            {
                // Some unknown error occured
                OnClientError(connection, ex);                                  // raise event (cross-threaded)
            }

            // If flow reaches here, means remote has disconnected. we must close the connection
            OnClientDisconnect(connection);                                     // raise event (cross-threaded)

        }

        /// <summary>
        /// Sends a message to the client or server machine. It is the responsibility of your code to ensure that EndOfLineCharacters are escaped in your message.
        /// </summary>
        /// <param name="connection">Connection through which the message is to be sent.</param>
        /// <param name="message">String message to be sent to the connected service.</param>
        /// <returns>Returns true if message has been successfully transmitted to the connected service.</returns>
        protected async Task<bool> OnSendMessageAsync(ConnectionObject connection, string message)
        {
            try
            {

                if (IsRunning && connection.TcpClient != null && connection.TcpClient.Client != null && connection.TcpClient.Connected)
                {

                    // Append the EndOfLines characters at the end of this message
                    string m = message + EndOfLinesCharacters;

                    // Dispatch
                    var bytes = Encoding.UTF8.GetBytes(m);
                    await connection.TcpClient.Client.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None).ConfigureAwait(false);

                    // Note bytes sent
                    int bytesSent = m.Length;
                    TotalBytesSent += bytesSent;
                    connection.BytesSent += bytesSent;

                    // Done
                    Events.OnMessageSent(this, connection, bytesSent);         // raise event
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                OnClientError(connection, ex);                                  // raise event (cross-threaded)
                return false;
            }

        }

        #endregion

    }
}
