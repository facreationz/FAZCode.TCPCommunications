using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FAZCode.TCPCommunications
{
    public class EventHandlers
    {

        // Delegates
        public delegate void MessageReceiveHandler(object sender, ConnectionObject connection, string message);
        public delegate void ClientErrorHandler(object sender, ConnectionObject connection, Exception ex);

        // Server-specific Events
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;
        public event EventHandler<Exception> ServerError;

        // Client-specific Events
        public event EventHandler<ConnectionObject> ClientConnected;
        public event EventHandler<ConnectionObject> ClientDisconnected;
        public event ClientErrorHandler ClientError;

        // Common
        public event MessageReceiveHandler MessageReceived;                   
        public event EventHandler<ConnectionObject> MessageSent;

        private System.Windows.Forms.Label EventFiringControl { get; set; }

        internal void Initialise()
        {
            // Event firing control helps preventing cross-threaded events
            EventFiringControl = new System.Windows.Forms.Label();
            EventFiringControl.CreateControl();
        }

        #region Server Events

        internal void OnServerStarted(object sender)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ServerStarted?.Invoke(sender, EventArgs.Empty);
            }));
        }

        internal void OnServerStopped(object sender)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ServerStopped?.Invoke(sender, EventArgs.Empty);
            }));
        }


        internal void OnServerError(object sender, Exception e)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ServerError?.Invoke(sender, e);
            }));
        }

        #endregion

        #region Client Events

        internal void OnClientConnected(object sender, ConnectionObject connection)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ClientConnected?.Invoke(sender, connection);
            }));
        }

        internal void OnClientDisconnected(object sender, ConnectionObject connection)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ClientDisconnected?.Invoke(sender, connection);
            }));
        }

        internal void OnClientError(object sender, ConnectionObject connection, Exception ex)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                ClientError?.Invoke(sender, connection, ex);
            }));
        }

        #endregion

        #region Communication

        internal void OnMessageReceived(object sender, ConnectionObject connection, string message)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                MessageReceived?.Invoke(sender, connection, message);
            }));
        }

        internal void OnMessageSent(object sender, ConnectionObject connection)
        {
            EventFiringControl.Invoke(new MethodInvoker(delegate
            {
                MessageSent?.Invoke(sender, connection);
            }));
        }


        #endregion


    }
}
