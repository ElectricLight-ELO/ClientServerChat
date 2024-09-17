using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public static Socket client;
        public Form1()
        {
            InitializeComponent();
            updater();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
        }
        public async void updater()
        {
            await Task.Run(() =>
            {
                while(true)
                {
                    if (textBox5.Text == "")
                    {
                        MessageBox.Show("havent ip");
                        return;
                    }

                    Thread.Sleep(1000);
                    string ip = textBox5.Text.Split(':')[0];
                    int port = int.Parse(textBox5.Text.Split(':')[1]);
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    IPEndPoint server = new IPEndPoint(IPAddress.Parse(ip), port);
                    client.Connect(server);

                    byte[] bytesToSend = Encoding.UTF8.GetBytes($"<id>{textBox3.Text}</id><idTO>{textBox4.Text}</idTO><update>bb</update>");
                    client.Send(bytesToSend);

                    byte[] bytesReceived = new byte[1024];
                    int bytesReceivedCount = client.Receive(bytesReceived);
                    bytesReceived = RC4.CryptBytes(bytesReceived, RC4.getPass(textBox3.Text, textBox4.Text));
                    string response = Encoding.UTF8.GetString(bytesReceived, 0, bytesReceivedCount);

                    Invoke((Action)(() => {
                        textBox2.Text = $" \r\n{response}";

                    }));
                    
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            });
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox5.Text == "") {
                MessageBox.Show("havent ip");
                return;
            }

            string ip = textBox5.Text.Split(':')[0];
            int port = int.Parse(textBox5.Text.Split(':')[1]);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client.Connect(serverEndPoint);
            string message = textBox1.Text;
            message = RC4.Crypt(Encoding.UTF8.GetBytes(message), RC4.getPass(textBox3.Text, textBox4.Text));
            byte[] bytesToSend = Encoding.UTF8.GetBytes($"<id>{textBox3.Text}</id><idTO>{textBox4.Text}</idTO><message>{message}</message>");
            
            client.Send(bytesToSend);

            byte[] bytesReceived = new byte[1024];
            int ReceivedCount = client.Receive(bytesReceived);
            bytesReceived = RC4.CryptBytes(bytesReceived, RC4.getPass(textBox3.Text, textBox4.Text));
            string response = Encoding.UTF8.GetString(bytesReceived, 0, ReceivedCount);

            textBox2.Text = $" \r\n{response}";

            
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private void textBox3_MouseClick(object sender, MouseEventArgs e)
        {
            textBox3.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
             
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if(button2.Text == "apply")
            {
                textBox5.Enabled = false;
                button2.Text = "cancel";
            }
            else
            {
                textBox5.Enabled = true;
                button2.Text = "apply";
            }
        }
    }
}
