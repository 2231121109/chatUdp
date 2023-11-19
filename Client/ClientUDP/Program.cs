using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClientUdp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);            

            LoginForm loginForm = new LoginForm();

            Application.Run(loginForm);
            if (loginForm.DialogResult == DialogResult.OK)
            {
                ClientUdp ClientUdpForm = new ClientUdp();
                ClientUdpForm.clientSocket = loginForm.clientSocket;
                ClientUdpForm.strName = loginForm.strName;
                ClientUdpForm.epServer = loginForm.epServer;

                ClientUdpForm.ShowDialog();
            }

        }
    }
}