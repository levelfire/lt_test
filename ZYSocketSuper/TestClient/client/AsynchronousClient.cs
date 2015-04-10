using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient.client
{
    public class AsynchronousClient
    {
        private static object alock = new object();
        private static AsynchronousClient instance;
        public static AsynchronousClient GetInstance()
        {
            if (instance == null)
            {
                lock (alock)
                {
                    if (instance == null)
                    {
                        instance = new AsynchronousClient();
                    }
                }
            }
            return instance;
        }

        private static Socket clientSocket;
        private static ArrayBuffer<byte> arraybuffer = new ArrayBuffer<byte>();

        //标记多个连接之间不同偏移值
        private int m_recvBufferOffset;
        private int m_sendBufferOffset;
        private static BufferManager m_bufferManager;

        //标记与当前连接偏移的相对接受缓冲偏移
        private int m_curRecvBufferOffset = 0;
        //当前缓冲待解析起始位置
        private int m_curBufferhead = 0;
        //当前缓冲带解析长度
        private int m_curBufferLen = 0;

        // The port number for the remote device.
        private const int port = 6600;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String response = String.Empty;



        //单例的构造函数
        AsynchronousClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                //IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse("192.168.66.87");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                clientSocket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), clientSocket);
                connectDone.WaitOne();

                m_bufferManager = new BufferManager();
                m_bufferManager.GetOffset(ref m_recvBufferOffset, ref m_sendBufferOffset);

                //// Send test data to the remote device.
                //Send(client, "This is a test<EOF>");
                //sendDone.WaitOne();

                // Receive the response from the remote device.
                Receive(clientSocket);
                //receiveDone.WaitOne();

                //// Write the response to the console.
                //Console.WriteLine("Response received : {0}", response);

                //// Release the socket.
                //client.Shutdown(SocketShutdown.Both);
                //client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        //private static void StartClient()
        //{
        //    // Connect to a remote device.
        //    try
        //    {
        //        // Establish the remote endpoint for the socket.
        //        // The name of the 
        //        // remote device is "host.contoso.com".
        //        //IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
        //        //IPAddress ipAddress = ipHostInfo.AddressList[0];
        //        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        //        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        //        // Create a TCP/IP socket.
        //        Socket client = new Socket(AddressFamily.InterNetwork,
        //            SocketType.Stream, ProtocolType.Tcp);

        //        // Connect to the remote endpoint.
        //        client.BeginConnect(remoteEP,
        //            new AsyncCallback(ConnectCallback), client);
        //        connectDone.WaitOne();

        //        // Send test data to the remote device.
        //        Send(client, "This is a test<EOF>");
        //        sendDone.WaitOne();

        //        // Receive the response from the remote device.
        //        Receive(client);
        //        receiveDone.WaitOne();

        //        // Write the response to the console.
        //        Console.WriteLine("Response received : {0}", response);

        //        // Release the socket.
        //        client.Shutdown(SocketShutdown.Both);
        //        client.Close();

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        //关闭Socket

        public void Closed()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            clientSocket = null;
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                //// Create the state object.
                //StateObject state = new StateObject();
                //state.workSocket = client;

                //// Begin receiving the data from the remote device.
                //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //    new AsyncCallback(ReceiveCallback), state);

                //client.BeginReceive(m_bufferManager.RecvBuffer, m_recvBufferOffset, m_bufferManager.RecvBufferSize, 0,
                //    new AsyncCallback(ReceiveCallback), client);

                client.BeginReceive(m_bufferManager.RecvBuffer, m_recvBufferOffset + m_curRecvBufferOffset, m_bufferManager.RecvBufferSize - m_curRecvBufferOffset, 0,
                new AsyncCallback(ReceiveCallback), client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //// Retrieve the state object and the client socket 
                //// from the asynchronous state object.
                //StateObject state = (StateObject)ar.AsyncState;
                //Socket client = state.workSocket;

                Socket client = (Socket)ar.AsyncState;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    ////// There might be more data, so store the data received so far.
                    ////state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    //Debug.Log(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    //// Get the rest of the data.
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //    new AsyncCallback(ReceiveCallback), state);

                    SplitPackage(bytesRead);

                    // Get the rest of the data.
                    client.BeginReceive(m_bufferManager.RecvBuffer, m_recvBufferOffset + m_curRecvBufferOffset, m_bufferManager.RecvBufferSize - m_curRecvBufferOffset, 0,
                    new AsyncCallback(ReceiveCallback), client);
                }
                else
                {
                    //// All the data has arrived; put it in response.
                    //if (state.sb.Length > 1)
                    //{
                    //    response = state.sb.ToString();
                    //}
                    ////string str = System.Text.Encoding.Default.GetString(bytes);
                    //Debug.Log(response);
                    //// Signal that all bytes have been received.
                    ////receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SplitPackage(int newbufflen)
        {
            m_curRecvBufferOffset += newbufflen;
            m_curBufferLen += newbufflen;

            if(m_curBufferLen > 2)
            {
                short length = BitConverter.ToInt16(m_bufferManager.RecvBuffer, m_curBufferhead);
                if (length <= m_curBufferLen - 2)
                {
                    byte[] re = new byte[length];
                    Array.Copy(m_bufferManager.RecvBuffer, m_recvBufferOffset + m_curBufferhead + 2, re, 0, length);

                    BaseFrame bf = (BaseFrame)Convert.BytesToStruct(re, typeof(BaseFrame));
                    string recive = Encoding.UTF8.GetString(bf.mFrame, 0, bf.mFrame.Length);

                    m_curBufferLen -= 2 + length;
                    m_curBufferhead += 2 + length;

                    SplitPackage(0);
                }
                else
                {
                    m_curBufferLen += length;
                    m_curRecvBufferOffset += length;

                    if(m_curRecvBufferOffset > m_bufferManager.m_recvBufferSize)
                    {
                        //超长，有可能会影响到其他数据的连接的缓冲数据，丢弃之前数据
                        m_curBufferLen = 0;
                    }
                }
            }
            if(m_curBufferLen == 0)
            {
                m_curBufferLen = 0;
                m_curBufferhead = 0;
                m_curRecvBufferOffset = 0;
            }
        }	

        public void Send(String data)
        {
            if (!clientSocket.Connected)
            {
                clientSocket.Close();
                return;
            }

            try
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                IAsyncResult asyncSend = clientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), clientSocket);

                bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
                if (!success)
                {
                    clientSocket.Close();
                    //Debug.Log("Failed to SendMessage server.");
                }
            }
            catch
            {
                //Debug.Log("send message error");
            }
        }

        public void Send(byte[] byteData)
        {
            if (!clientSocket.Connected)
            {
                clientSocket.Close();
                return;
            }

            try
            {
                //// Convert the string data to byte data using ASCII encoding.
                //byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                IAsyncResult asyncSend = clientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), clientSocket);

                bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
                if (!success)
                {
                    clientSocket.Close();
                    //Debug.Log("Failed to SendMessage server.");
                }
            }
            catch
            {
                //Debug.Log("send message error");
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
