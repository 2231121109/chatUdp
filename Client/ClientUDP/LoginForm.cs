using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace ClientUdp
{
    public partial class LoginForm : Form
    {
        public Socket clientSocket;
        public EndPoint epServer;
        public string strName;

        public LoginForm()
        {
            InitializeComponent();
            string localIpAddress = getLocalIpAddress();
            txtServerIP.Text = localIpAddress;
            txtName.Text = generateRandomName();
            txtPort.Text = "1000";
            //Console.WriteLine($"Local IP Address: {localIpAddress}");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            strName = txtName.Text;
            try
            {
    
                clientSocket = new Socket(AddressFamily.InterNetwork, 
                    SocketType.Dgram, ProtocolType.Udp);

       
                IPAddress ipAddress = IPAddress.Parse(txtServerIP.Text);
                int port = int.Parse(txtPort.Text);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);

                epServer = (EndPoint)ipEndPoint;
                
                Data msgToSend = new Data ();
                msgToSend.cmdCommand = Command.Login;
                msgToSend.strMessage = null;
                msgToSend.strName = strName;

                byte[] byteData = msgToSend.ToByte();
   
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, 
                    SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            { 
                MessageBox.Show(ex.Message, "ClientUdp", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error); 
            } 
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);                
                strName = txtName.Text;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClientUdp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            if (isAvailable())
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private void txtServerIP_TextChanged(object sender, EventArgs e)
        {
            if (isAvailable())
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private string getLocalIpAddress()
        {
            string ipAddress = "";

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipAddress = ipInfo.Address.ToString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        break;
                    }
                }
            }

            return ipAddress;
        }

        private string generateRandomName()
        {
            string[] adjectives = { "Happy", "Sunny", "Clever", "Colorful", "Gentle", "Wild", "Brave", "Cheerful" };
            string[] nouns = { "Dog", "Cat", "Bird", "Mountain", "River", "Star", "Ocean", "Tree" };

            Random random = new Random();
            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];

            return $"{adjective}{noun}";
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            if (isAvailable())
                btnOK.Enabled = true;
            else
                btnOK.Enabled = false;
        }

        private bool isAvailable()
        {
            return txtName.Text.Length > 0 && txtServerIP.Text.Length > 0 && txtPort.Text.Length > 0;
        }
    }
}