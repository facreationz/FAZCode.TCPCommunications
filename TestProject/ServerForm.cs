using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FAZCode.TCPCommunications;
using Newtonsoft.Json.Linq;
using TestProject.Communications;

namespace TestProject
{
    public partial class ServerForm : Form
    {

        private ServerApp Server { get; set; }

        #region Constra

        public ServerForm()
        {
            InitializeComponent();

            // Startup position
            Location = new Point(100, 100);
            
            // Hook it up
            Server = new ServerApp();
            Server.Events.ServerStarted += Server_ServerStarted;
            Server.Events.ServerStopped += Server_ServerStopped;
            Server.Events.ServerError += Server_ServerError;
            Server.Events.ClientConnected += Server_ClientConnected;
            Server.Events.ClientDisconnected += Server_ClientDisconnected;
            Server.Events.ClientError += Server_ClientError;
            Server.Events.MessageReceived += Server_MessageReceived;
            Server.Events.MessageSent += Server_MessageSent;
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Server.IsRunning) Server.StopServer();
        }

        #endregion

        #region UI Events

        private void Status(string text)
        {
            if (this.Disposing | this.IsDisposed) return;
            txtStatus.AppendText(text + Environment.NewLine);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

            // Check if already running
            if (Server.IsRunning)
            {
                Status("Server is already running. Stop server first before restarting.");
                return;
            }

            // Start now
            var serverHost = System.Net.IPAddress.Loopback;
            var serverPort = 1982;

            Status($"Starting server at port {serverPort}...");
            Server.StartServer(serverHost, serverPort);


        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Status("Stopping Server...");
            Server.StopServer();
        }

        private async void btnRespond_Click(object sender, EventArgs e)
        {
            // Pick the selected client from the list
            if (lvConnections.SelectedItems.Count == 0) return;
            string connID = lvConnections.SelectedItems[0].Name;

            var client = Server.Connections.FirstOrDefault(x => x.ConnectionId == connID);
            if (client == null) return;

            Status($"Messaging client [{client.ConnectionId}]");

            // Convert to packet
            Packet packet = new Packet(0, "Hello from server!");
            string message = packet.SerializeMessage(Server.EndOfLinesCharacters);

            // Send
            await Server.SendMessageAsync(client, message);

        }

        private void btnNewClient_Click(object sender, EventArgs e)
        {
            var clientForm = new ClientForm();
            clientForm.Show(this);
        }

        private void lvConnections_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRespond.Enabled = lvConnections.SelectedItems.Count > 0;
        }

        #endregion

        #region Server Events

        private void Server_ServerStarted(object sender, EventArgs e)
        {
            Status("Server Started.");
        }

      
        private void Server_ServerStopped(object sender, EventArgs e)
        {
            Status("Server Stopped.");
        }

        private void Server_ServerError(object sender, Exception e)
        {
            Status("Server Error: " + e.Message);
        }

        private void Server_ClientConnected(object sender, ConnectionObject e)
        {
            Status($"Client Connected Received [{e.ConnectionId}]");
            var iTm = lvConnections.Items.Add(e.ConnectionId, "", 0);
            iTm.Text = (lvConnections.Items.IndexOf(iTm) + 1).ToString();
            iTm.SubItems.Add(e.ConnectionId);
        }

        private void Server_ClientDisconnected(object sender, ConnectionObject e)
        {
            Status("Client Disconnected: " + e.ConnectionId);
            lvConnections.Items.RemoveByKey(e.ConnectionId);
        }

        private void Server_ClientError(object sender, ConnectionObject connection, Exception ex)
        {
            Status($"Client Error [{connection.ConnectionId}]: " + ex.Message);
        }

        private void Server_MessageReceived(object sender, ConnectionObject connection, string message, long bytesReceived)
        {
            // Convert message to generic packet
            JObject jsonPacket = HelperTools.MessageToJObject(message, Server.EndOfLinesCharacters);
            Packet packet = jsonPacket.ToObject<Packet>();
            

            // Read packet
            if (packet.Command == 10)
            {
                // This is a file packet
                FileDataPacket filePacket = jsonPacket.ToObject<FileDataPacket>();
                
                System.IO.File.WriteAllBytes(Path.Combine(@"C:\Temp", Path.GetFileName(filePacket.FileName)), filePacket.BinaryData);


                Status($"File received [{filePacket.FileName}], size [{filePacket.FileSize}].");
            }
            else
            {
                // Standard packet
                Status($"Message Received [{connection.ConnectionId}]: " + packet.DataString);
            }
        }

        private void Server_MessageSent(object sender, ConnectionObject connection, long bytesSent)
        {
            Status($"Message Sent [{connection.ConnectionId}]");
        }

        #endregion

    }
}
