using System;
using System.IO;
using System.Windows.Forms;
using FAZCode.TCPCommunications;
using TestProject.Communications;

namespace TestProject
{
    public partial class ClientForm : Form
    {
        private ClientApp Client { get; set; }

        public ClientForm()
        {
            InitializeComponent();

            // Hook up
            Client = new ClientApp();

            Client.Events.ClientConnected += Client_ClientConnected;
            Client.Events.ClientDisconnected += Client_ClientDisconnected;
            Client.Events.ClientError += Client_ClientError;
            Client.Events.MessageReceived += Client_MessageReceived;
            Client.Events.MessageSent += Client_MessageSent;
        }


        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Client.IsRunning) Client.Disconnect();
        }

        private void Status(string text)
        {
            if (this.Disposing | this.IsDisposed) return;
            txtStatus.AppendText(text + Environment.NewLine);
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            Status("Connecting to server at port localhost:1982...");
            bool result = await Client.ConnectAsync("127.0.0.1", 1982);
            if (result)
            {

            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Status("Disconnecting...");
            Client.Disconnect();
        }

        private async void btnMessage_Click(object sender, EventArgs e)
        {
            // Create packet
            Packet packet = new Packet(0, "Howdy Server!");

            // Convert it to string message
            string message = packet.SerializeMessage(Client.EndOfLinesCharacters);

            // Send
            await Client.SendMessageAsync(message);
        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            string file = @"C:\Users\faraz\Downloads\CNT-0024506-02.PDF";
            byte[] data = File.ReadAllBytes(file);

            // Build file data packet
            FileDataPacket filePacket = new FileDataPacket()
            {
                Command = 10,
                BinaryData = data,
                FileName = file,
                FileSize  = data.Length,
                CreationTimeUtc = File.GetCreationTimeUtc(file),
                LastWriteTimeUtc = File.GetLastWriteTimeUtc(file),
            };

            // Serialize it to a message
            string message = filePacket.SerializeMessage(Client.EndOfLinesCharacters);

            // Send
            await Client.SendMessageAsync(message);
        }

        #region Client Events

        private void Client_MessageSent(object sender, ConnectionObject e)
        {
            Status("Message Sent.");
        }

        private void Client_MessageReceived(object sender, ConnectionObject connection, string message)
        {
            // Read message
            Packet packet = Packet.DeserializeMessage(message);

            // Display string message
            Status("Message Received: " + packet.DataString);
        }

        private void Client_ClientError(object sender, ConnectionObject connection, Exception ex)
        {
            Status("Client Error: " + ex.Message);
        }

        private void Client_ClientDisconnected(object sender, ConnectionObject e)
        {
            Status("Client Disconnected");
        }

        private void Client_ClientConnected(object sender, ConnectionObject e)
        {
            Status("Client Connected");
        }


        #endregion

    }
}
