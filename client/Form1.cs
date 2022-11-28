using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {

        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBox_ip.Text;

            int portNum;
            if (IP != "")
            {
                if (Int32.TryParse(textBox_port.Text, out portNum))
                {
                    try
                    {
                        if (textBox_name.Text != "")
                        {
                            clientSocket.Connect(IP, portNum);

                            button_connect.Enabled = false;
                            button_disconnect.Enabled = true;
                            button_send.Enabled = true;
                            textBox_answer.Enabled = true;
                            textBox_ip.Enabled = false;
                            textBox_port.Enabled = false;
                            textBox_name.Enabled = false;
                            connected = true;
                            logs.AppendText("Connected to the server!\n");

                            Thread receiveThread = new Thread(Receive);
                            receiveThread.Start();

                            Byte[] nameBuffer = Encoding.Default.GetBytes(textBox_name.Text);
                            clientSocket.Send(nameBuffer);

                        }
                        else
                        {
                            logs.AppendText("Please enter your name\n");
                        }

                    }
                    catch
                    {
                        logs.AppendText("Could not connect to the server, check IP or port!\n");
                    }
                }
                else
                {
                    logs.AppendText("Please enter the port or check its correctness.\n");
                }
            }
            else
            {
                logs.AppendText("Please enter your IP address.\n");
            }
        }

        private void Receive()
        {
            if (connected)
            {
                Byte[] buffer = new Byte[64];
                clientSocket.Receive(buffer);
                string incomingMessage = Encoding.Default.GetString(buffer);
                incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));
                logs.AppendText(incomingMessage);
            }
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[256];
                    clientSocket.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);

                    logs.AppendText("Server: " + incomingMessage + "\n");
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("The server has disconnected\n");
                        textBox_ip.Enabled = true;
                        textBox_port.Enabled = true;
                        textBox_name.Enabled = true;
                        textBox_answer.Enabled = false;
                        button_connect.Enabled = true;
                        button_disconnect.Enabled = false;
                        button_send.Enabled = false;
                    }

                    clientSocket.Close();
                    connected = false;
                }

            }
        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            clientSocket.Close();
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = textBox_answer.Text;

            if (message != "" && message.Length <= 64)
            {
                Byte[] buffer = Encoding.Default.GetBytes(message);
                clientSocket.Send(buffer);
                logs.AppendText("My answer is: " + message + "\n");
                textBox_answer.Text = "";
            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}