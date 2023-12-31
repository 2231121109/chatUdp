﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ClientUdp
{
    
    enum Command
    {
        Login,     
        Logout,
        Message,   
        List,       
        Null        
    }

    public partial class ClientUdp : Form
    {
        public Socket clientSocket;
        public string strName;   
        public EndPoint epServer;  
        byte []byteData = new byte[1024];

        public ClientUdp()
        {
            InitializeComponent();
        }

    
        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {		
   
                Data msgToSend = new Data();
                
                msgToSend.strName = strName;
                msgToSend.strMessage = txtMessage.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

        
                clientSocket.BeginSendTo (byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                txtMessage.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Khong the gui tin nhan toi server.", "ClientUdp: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }  
        }
        
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClientUdp: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {            
            try
            {                
                clientSocket.EndReceive(ar);

           
                Data msgReceived = new Data(byteData);

         
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        lstChatters.Items.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        lstChatters.Items.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        lstChatters.Items.AddRange(msgReceived.strMessage.Split('*'));
                        lstChatters.Items.RemoveAt(lstChatters.Items.Count - 1);
                        txtChatBox.Text += "<<<" + strName + " da tham gia phong chat>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                    txtChatBox.Text += msgReceived.strMessage + "\r\n";

                byteData = new byte[1024];                

        
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer,
                                           new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClientUdp: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
	        try
            {
                CheckForIllegalCrossThreadCalls = false;

                this.Text = "ClientUdp: " + strName;


                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.List;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byteData = msgToSend.ToByte();

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer,
                    new AsyncCallback(OnSend), null);

                byteData = new byte[1024];
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
            }
            catch(Exception ex)
            {
                var messageResult = MessageBox.Show("đã có lỗi xảy ra có thể server chưa start", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if(messageResult == DialogResult.OK)
                {
                    Application.Exit();
                }
            }
        }

        private void txtMessage_TextChanged(object sender, EventArgs e)
        {
            if (txtMessage.Text.Length == 0)
                btnSend.Enabled = false;
            else
                btnSend.Enabled = true;
        }

        private void ClientUdp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to leave the chat room?", "ClientUdp: " + strName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            try
            {
            
                Data msgToSend = new Data ();
                msgToSend.cmdCommand = Command.Logout;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byte[] b = msgToSend.ToByte ();
                clientSocket.SendTo(b, 0, b.Length, SocketFlags.None, epServer);
                clientSocket.Close();
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClientUdp: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, null);
            }
        }

        private void txtChatBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
    
 
    class Data
    {
       
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        public Data(byte[] data)
        {
  
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

    
            int nameLen = BitConverter.ToInt32(data, 4);

    
            int msgLen = BitConverter.ToInt32(data, 8);


            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

 
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

   
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

  
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));


            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

          
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      
        public string strMessage;  
        public Command cmdCommand;  
    }
}