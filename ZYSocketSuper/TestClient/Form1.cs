using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Text;
using TestClient.client;
using System.Runtime.InteropServices;

namespace TestClient
{
    public partial class Form1 : Form
    {
        public AsynchronousClient aSocketClient;

        public enum Protocol
        {
            Test
        }

        public enum ProtocolSub
        {
            Test
        }

        //结构体序列化
        [System.Serializable]
        //4字节对齐 iphone 和 android上可以1字节对齐
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BaseFrame
        {
            public Protocol mProtocol;
            public ProtocolSub mPprotocolsub;
            public byte[] mFrame;

            public BaseFrame(Protocol protocol, ProtocolSub protocolsub, byte[] frame)
            {
                mProtocol = protocol;
                mPprotocolsub = protocolsub;
                mFrame = frame;
            }
        };

        public class Convert
        {
            //结构体转字节数组
            public static byte[] StructToBytes(object structObj)
            {

                int size = Marshal.SizeOf(structObj);
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(structObj, buffer, false);
                    byte[] bytes = new byte[size];
                    Marshal.Copy(buffer, bytes, 0, size);
                    return bytes;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            //字节数组转结构体
            public static object BytesToStruct(byte[] bytes, Type strcutType)
            {
                int size = Marshal.SizeOf(strcutType);
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.Copy(bytes, 0, buffer, size);
                    return Marshal.PtrToStructure(buffer, strcutType);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string host = "192.168.66.87";
            int port = 6600;

            ////string result = SocketSendReceive(host, port);

            //string result = Connect(host, port, "123");
            //richTextBox1.Text = result;
            ////Console.WriteLine(result);

            aSocketClient = AsynchronousClient.GetInstance();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string sends = textBox1.Text;
            //string result = Send(sends);
            //richTextBox1.Text = result;

            byte[] bytef = System.Text.Encoding.ASCII.GetBytes(System.DateTime.UtcNow.ToString());
            BaseFrame bf = new BaseFrame(Protocol.Test,ProtocolSub.Test,bytef);
            byte[] body = Convert.StructToBytes(bf);
            byte[] head = BitConverter.GetBytes((short)body.Length);
            aSocketClient.Send(head.Concat(body).ToArray());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //string result = Disconnect();
            //richTextBox1.Text = result;

            if (aSocketClient != null)
            {
                aSocketClient.Closed();
            }
        }

        public TcpClient client;
        public NetworkStream stream;

        public string Connect(String server, int port, String message)
        {
            string result = string.Empty;
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                //Int32 port = 13000;
                client = new TcpClient(server, port);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();
                stream = client.GetStream();

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();

            return result;
        }

        public string Send(String message)
        {
            string result = string.Empty;
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                result = responseData;    
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
                result = string.Format("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                result = string.Format("SocketException: {0}", e);
            }

            return result;
        }

        public string Disconnect()
        {
            string result = string.Empty;
            try
            {
                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
                result = string.Format("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                result = string.Format("SocketException: {0}", e);
            }

            return result;
        }
        private static Socket ConnectSocket(string server, int port)
        {
            Socket s = null;
            IPHostEntry hostEntry = null;

            // Get host related information.
            hostEntry = Dns.GetHostEntry(server);

            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected)
                {
                    s = tempSocket;
                    break;
                }
                else
                {
                    continue;
                }
            }
            return s;
        }

        // This method requests the home page content for the specified server.
        private static string SocketSendReceive(string server, int port)
        {
            string request = "GET / HTTP/1.1\r\nHost: " + server +
                "\r\nConnection: Close\r\n\r\n";
            Byte[] bytesSent = Encoding.ASCII.GetBytes(request);
            Byte[] bytesReceived = new Byte[256];

            // Create a socket connection with the specified server and port.
            Socket s = ConnectSocket(server, port);

            if (s == null)
                return ("Connection failed");

            // Send request to the server.
            s.Send(bytesSent, bytesSent.Length, 0);

            // Receive the server home page content.
            int bytes = 0;
            string page = "Default HTML page on " + server + ":\r\n";

            // The following will block until te page is transmitted.
            do
            {
                bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                page = page + Encoding.ASCII.GetString(bytesReceived, 0, bytes);
            }
            while (bytes > 0);

            return page;
        }

        

    }
}
