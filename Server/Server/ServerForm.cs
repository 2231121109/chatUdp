using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    enum Command
    {
        Login,
        Logout,
        Message,
        List,
        Null
    }

    public partial class Server : Form
    {
        public struct CInfo
        {
            public EndPoint EP;
            public string CName;
        }

        public ArrayList CList;

        Socket SSocket;

        byte[] byteData = new byte[1024];

        public Server()
        {
            CList = new ArrayList();
            InitializeComponent();
        }

        private int port = 1000;
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                lblStatusServer.Visible = false;
                CheckForIllegalCrossThreadCalls = false;


                SSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);


                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);


                SSocket.Bind(ipEndPoint);
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                SSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender, new AsyncCallback(Receive), epSender);
                lblStatusServer.Text = "đã khởi động server với port: " + port;
                lblStatusServer.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServerUDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSend_Click(object sender, EventArgs e)
        {

            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                Data msgToSend = new Data();


                msgToSend.Message = txtMessage.Text;
                msgToSend.cmd = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                foreach (CInfo clientInfo in CList)
                {
                    if (clientInfo.EP != epSender ||
                        msgToSend.cmd != Command.Login)
                    {
                        SSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, clientInfo.EP, new AsyncCallback(Send), clientInfo.EP);
                    }
                }

                txtLog.Text += msgToSend.Message + "\r\n";
                txtMessage.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể gửi tin nhắn tới client .", "ServerUDP: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Send(IAsyncResult ar)
        {
            try
            {
                SSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServerUDP: ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Receive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                SSocket.EndReceiveFrom(ar, ref epSender);

                Data msgReceived = new Data(byteData);

                Data msgToSend = new Data();

                byte[] message;

                msgToSend.cmd = msgReceived.cmd;
                msgToSend.Name = msgReceived.Name;

                switch (msgReceived.cmd)
                {
                    case Command.Login:


                        CInfo clientInfo = new CInfo();
                        clientInfo.EP = epSender;
                        clientInfo.CName = msgReceived.Name;

                        CList.Add(clientInfo);
                        lstChatters.Items.Add(msgReceived.Name);

                        msgToSend.Message = "***" + msgReceived.Name + " đã tham gia phòng chat ***";
                        break;

                    case Command.Logout:


                        int nIndex = 0;
                        lstChatters.Items.Remove(msgReceived.Name);
                        foreach (CInfo client in CList)
                        {

                            if (client.EP == epSender)
                            {

                                CList.RemoveAt(nIndex);

                                break;
                            }
                            ++nIndex;
                        }

                        msgToSend.Message = "***" + msgReceived.Name + " đã rời phòng chat ***";

                        break;

                    case Command.Message:

                        msgToSend.Message = msgReceived.Name + ": " + msgReceived.Message;
                        break;

                    case Command.List:

                        msgToSend.cmd = Command.List;
                        msgToSend.Name = null;
                        msgToSend.Message = null;

                        foreach (CInfo client in CList)
                        {
                            msgToSend.Message += client.CName + "*";
                        }

                        message = msgToSend.ToByte();

                        SSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, epSender, new AsyncCallback(Send), epSender);

                        break;
                }

                if (msgToSend.cmd != Command.List)
                {
                    message = msgToSend.ToByte();

                    foreach (CInfo clientInfo in CList)
                    {
                        if (clientInfo.EP != epSender ||
                            msgToSend.cmd != Command.Login)
                        {
                            SSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.EP, new AsyncCallback(Send), clientInfo.EP);
                        }
                    }

                    txtLog.Text += msgToSend.Message + "\r\n";
                }

                if (msgReceived.cmd != Command.Logout)
                {

                    SSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender, new AsyncCallback(Receive), epSender);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ServerUDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void textBoxSend_TextChanged(object sender, EventArgs e)
        {
            if (txtMessage.Text.Length == 0)
                btnSend.Enabled = false;
            else
                btnSend.Enabled = true;
        }


        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, null);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void lstChatters_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }

    class Data
    {
        public Data()
        {
            this.cmd = Command.Null;
            this.Message = null;
            this.Name = null;
        }

        public Data(byte[] data)
        {
            this.cmd = (Command)BitConverter.ToInt32(data, 0);

            int nameLen = BitConverter.ToInt32(data, 4);
            int msgLen = BitConverter.ToInt32(data, 8);

            if (nameLen > 0)
                this.Name = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.Name = null;

            if (msgLen > 0)
                this.Message = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.Message = null;
        }

        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)cmd));

            if (Name != null)
                result.AddRange(BitConverter.GetBytes(Name.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (Message != null)
                result.AddRange(BitConverter.GetBytes(Message.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (Name != null)
                result.AddRange(Encoding.UTF8.GetBytes(Name));

            if (Message != null)
                result.AddRange(Encoding.UTF8.GetBytes(Message));

            return result.ToArray();
        }

        public string Name;
        public string Message;
        public Command cmd;
    }
}