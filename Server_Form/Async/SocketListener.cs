using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using Server_Form;

namespace servercore
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.aspx
    /// Implements the connection logic for the socket server.  
    /// After accepting a connection, all data read from the client 
    /// is sent back to the client with an aditional word, to demonstrate how to manipulate the data. 
    /// The read and echo back to the client pattern is continued until the client disconnects.
    /// </summary>
    public sealed class SocketListener
    {
        TSocketServerBase<TTestSession, TTestAccessDatabase> m_socketServerBase;

        /// <summary>
        /// Represents a large reusable set of buffers for all socket operations.
        /// </summary>
        private BufferManagerA bufferManager;

        /// <summary>
        /// The socket used to listen for incoming connection requests.
        /// </summary>
        private Socket listenSocket;

        /// <summary>
        /// Mutex to synchronize server execution.
        /// </summary>
        private static Mutex mutex = new Mutex();

        /// <summary>
        /// The total number of clients connected to the server.
        /// </summary>
        public Int32 numConnectedSockets;

        /// <summary>
        /// the maximum number of connections the sample is designed to handle simultaneously.
        /// </summary>
        private Int32 numConnections;

        /// <summary>
        /// Read, write (don't alloc buffer space for accepts).
        /// </summary>
        private const Int32 opsToPreAlloc = 2;

        /// <summary>
        /// Pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations.
        /// </summary>
        private SocketAsyncEventArgsPool readWritePool;

        internal SocketAsyncEventArgsPool ReadWritePool
        {
            get { return readWritePool; }
            set { readWritePool = value; }
        }

        /// <summary>
        /// Controls the total number of clients connected to the server.
        /// </summary>
        private Semaphore semaphoreAcceptedClients;

        /// <summary>
        /// Total # bytes counter received by the server.
        /// </summary>
        private Int32 totalBytesRead;         

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method.
        /// </summary>
        /// <param name="numConnections">Maximum number of connections to be handled simultaneously.</param>
        /// <param name="receiveBufferSize">Buffer size to use for each socket I/O operation.</param>
        internal SocketListener(Int32 numConnections, Int32 receiveBufferSize, object pSocketServer)
        {
            this.totalBytesRead = 0;
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;
            this.m_socketServerBase = (TSocketServerBase<TTestSession, TTestAccessDatabase>)pSocketServer;

            // Allocate buffers such that the maximum number of sockets can have one outstanding read and 
            // write posted to the socket simultaneously .
            this.bufferManager = new BufferManagerA(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        public void PushArgs(SocketAsyncEventArgs e)
        {
            this.readWritePool.Push(e);
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        public void CloseClientSocket(SocketAsyncEventArgs e)
        {
            //if (null == e.UserToken)
            //{
            //    //this.readWritePool.Push(e);
            //    return;
            //}
            //Socket s = e.UserToken as Socket;
            TSessionBase TB = e.UserToken as TSessionBase;
            Socket s = TB.MSocket;
            try
            {
                s.Shutdown(SocketShutdown.Send);
            }
            
            catch (Exception) 
            {
                // Throws if client process has already closed.
            }
            s.Close();

            // Decrement the counter keeping track of the total number of clients connected to the server.
            this.semaphoreAcceptedClients.Release();

            Interlocked.Decrement(ref this.numConnectedSockets);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);

            //e.AcceptSocket = null;

            // Free the SocketAsyncEventArg so they can be reused by another client.
            this.readWritePool.Push(e);
        }

        public void CloseClientSocket(Socket s, SocketAsyncEventArgs ReceiveE, SocketAsyncEventArgs SendE)
        {
            try
            {
                s.Shutdown(SocketShutdown.Send);
            }

            catch (Exception)
            {
                // Throws if client process has already closed.
            }
            s.Close();

            // Decrement the counter keeping track of the total number of clients connected to the server.
            this.semaphoreAcceptedClients.Release();

            Interlocked.Decrement(ref this.numConnectedSockets);
            Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);

            //ReceiveE.AcceptSocket = null;
            //ReceiveE.UserToken = null;

            //SendE.AcceptSocket = null;
            //SendE.UserToken = null;
            // Free the SocketAsyncEventArg so they can be reused by another client.
            //在回收前先清空，查看是否会出错
            Array.Clear(ReceiveE.Buffer, ReceiveE.Offset,5000);
            Array.Clear(SendE.Buffer, SendE.Offset, 5000);
            //清空发送args的buffer
            //SendE.SetBuffer(null, 0, 0);

            this.readWritePool.Push(ReceiveE);
            this.readWritePool.Push_send(SendE);
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        internal void Init()
        {
            // Allocates one large Byte buffer which all I/O operations use a piece of. This guards 
            // against memory fragmentation.
            this.bufferManager.InitBuffer();

            // Preallocate pool of SocketAsyncEventArgs objects.
            SocketAsyncEventArgs readWriteEventArg;
            SocketAsyncEventArgs WriteEventArg;

            for (Int32 i = 0; i < this.numConnections; i++)
            {
                // Preallocate a set of reusable SocketAsyncEventArgs.
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                //readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Completed);

                // Assign a Byte buffer from the buffer pool to the SocketAsyncEventArg object.
                this.bufferManager.SetBuffer(readWriteEventArg);

                // Add SocketAsyncEventArg to the pool.
                this.readWritePool.Push(readWriteEventArg);

                // Preallocate a set of reusable SocketAsyncEventArgs.
                WriteEventArg = new SocketAsyncEventArgs();
                WriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                //readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Completed);

                //写args不在初始化时设置buffer
                //现在该为初始化时设置内存，尝试使用消息队列
                // Assign a Byte buffer from the buffer pool to the SocketAsyncEventArg object.
                this.bufferManager.SetBuffer(WriteEventArg);

                // Add SocketAsyncEventArg to the pool.
                this.readWritePool.Push_send(WriteEventArg);
            }
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// Callback method associated with Socket.AcceptAsync 
        /// operations and is invoked when an accept operation is complete.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            //this.ProcessAccept(e);
            // Determine which type of operation just completed and call the associated handler.
                switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    this.ProcessAccept(e);
                    break; 
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                //case SocketAsyncOperation.Send:
                //    this.ProcessSend(e);
                //    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        public void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    {
                        this.ProcessReceive(e);
                    }
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        /// <summary>
        /// Process the accept for the socket listener.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void ProcessAccept(SocketAsyncEventArgs e) 
        {
            //暂时取消同IP的限制
            if (e.AcceptSocket != null /*&& clientSocket.Connected*/)
            {
                if (m_socketServerBase.SessionCount >= m_socketServerBase.MaxSessionCount || !m_socketServerBase.CheckSocketIP(e.AcceptSocket))  // 当前列表已经存在该 IP 地址
                {
                    m_socketServerBase.OnSessionRejected(); // 拒绝登录请求
                    m_socketServerBase.CloseClientSocket(e.AcceptSocket);
                }
                else
                {
                    m_socketServerBase.AddSession(e);  //  [12/12/2011 test] 添加到队列中, 使用完成端口，不在此处调用异步接收方法
                }
            }
            else  // clientSocket is null or connected == false
            {
                m_socketServerBase.CloseClientSocket(e.AcceptSocket);
            }

            // Accept the next connection request.
            this.StartAccept(e);
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation.</param>
        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            TSessionBase TB = e.UserToken as TSessionBase;
            Socket s = TB.MSocket;

            if (TB.State != TSessionState.Active)
            {
                return;
            }

            if (!s.Connected)
            {
                TB.SetInactive();
                return;
            }

            try
            {
                if (e.BytesTransferred == 0)
                {
                    TB.DisconnectType = TDisconnectType.Normal;
                    TB.State = TSessionState.Inactive;
                }
                else  // 正常数据包
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        TB.LastSessionTime = DateTime.Now;
                        // 合并报文，按报文头、尾字符标志抽取报文，将包交给数据处理器
                        TB.ResolveSessionBuffer(e);
                        //TB.ReceiveDatagram();  // 继续接收

                        Boolean willRaiseEvent = s.ReceiveAsync(e);
                        if (!willRaiseEvent)
                        {
                            this.ProcessReceive(e);
                        }
                    }
                    else
                    {
                        this.CloseClientSocket(e);
                    }
                }
            }
            catch (Exception err)  // 读 socket 异常，关闭该会话，系统不认为是错误（这种错误可能太多）
            {
                if (TB.State == TSessionState.Active)
                {
                    TB.DisconnectType = TDisconnectType.Exception;
                    TB.State = TSessionState.Inactive;
                    this.CloseClientSocket(e);
                    TB.OnSessionReceiveException(err);
                }
            }
            //// Check if the remote host closed the connection.
            //if (e.BytesTransferred > 0)
            //{
            //    if (e.SocketError == SocketError.Success)
            //    {
            //        TSessionBase TB = e.UserToken as TSessionBase;
            //        Socket s = TB.MSocket;
            //        //Socket s = e.UserToken as Socket;

            //        Int32 bytesTransferred = e.BytesTransferred;

            //        // Get the message received from the listener.
            //        String received = Encoding.ASCII.GetString(e.Buffer, e.Offset, bytesTransferred);

            //        // Increment the count of the total bytes receive by the server.
            //        Interlocked.Add(ref this.totalBytesRead, bytesTransferred);
            //        Console.WriteLine("Received: \"{0}\". The server has read a total of {1} bytes.", received, this.totalBytesRead);

            //        // Format the data to send to the client.
            //        Byte[] sendBuffer = Encoding.ASCII.GetBytes("Returning " + received);

            //        // Set the buffer to send back to the client.
            //        e.SetBuffer(sendBuffer, 0, sendBuffer.Length);
            //        Boolean willRaiseEvent = s.SendAsync(e);
            //        if (!willRaiseEvent)
            //        {
            //            this.ProcessSend(e);
            //        }
            //    }
            //    else
            //    {
            //        this.CloseClientSocket(e);
            //    }
            //}
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        public void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Done echoing data back to the client.
                //Socket s = e.UserToken as Socket;
                //TSessionBase TB = e.UserToken as TSessionBase;
                //Socket s = TB.MSocket;

                //ReadWritePool.Push(e);
                //// Read the next block of data send from the client.
                //Boolean willRaiseEvent = s.ReceiveAsync(e);
                //if (!willRaiseEvent)
                //{
                //    this.ProcessReceive(e);
                //}
                TSessionBase TB = e.UserToken as TSessionBase;
                TB.BCanSend = true;
                //e.SetBuffer(null, 0, 0);
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }

        /// <summary>
        /// create and listen.    
        /// </summary>
        /// <param name="localEndPoint">The endpoint which the server will listening for connection requests on.</param>
        internal void CreateAndListen(Object data, Object nTime, ref Socket mSocket)
        {
            Int32 port = (Int32)data;

            // Get host related information.
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            // Get endpoint for the listener.
            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

            // Create the socket which listens for incoming connections.
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // Set dual-mode (IPv4 & IPv6) for the socket listener.
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                // based on http://blogs.msdn.com/wndp/archive/2006/10/24/creating-ip-agnostic-applications-part-2-dual-mode-sockets.aspx
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                // Associate the socket with the local endpoint.
                this.listenSocket.Bind(localEndPoint);
            }

            // Start the server with a listen backlog of 100 connections.
            this.listenSocket.Listen((Int32)nTime);
            mSocket = listenSocket;

            // Post accepts on the listening socket.
            this.StartAccept(null);

            mutex.WaitOne();
        }

        /// <summary>
        /// Starts the server such that it is listening for incoming connection requests.    
        /// </summary>
        /// <param name="localEndPoint">The endpoint which the server will listening for connection requests on.</param>
        internal void Start(Object data)
        {
            Int32 port = (Int32)data;

            // Get host related information.
            IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
            // Get endpoint for the listener.
            IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

            // Create the socket which listens for incoming connections.
            this.listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // Set dual-mode (IPv4 & IPv6) for the socket listener.
                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                // based on http://blogs.msdn.com/wndp/archive/2006/10/24/creating-ip-agnostic-applications-part-2-dual-mode-sockets.aspx
                this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                this.listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
            }
            else
            {
                // Associate the socket with the local endpoint.
                this.listenSocket.Bind(localEndPoint);
            }

            // Start the server with a listen backlog of 100 connections.
            this.listenSocket.Listen(00);

            // Post accepts on the listening socket.
            this.StartAccept(null);

            mutex.WaitOne();
        }

        /// <summary>
        /// Begins an operation to accept a connection request from the client.
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing 
        /// the accept operation on the server's listening socket.</param>
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            //while (!m_socketServerBase.MServerClosed)
            //{
            //    if (m_socketServerBase.MServerListenPaused)  // pause server
            //    {
            //        m_socketServerBase.CloseServerSocket();
            //        Thread.Sleep(m_socketServerBase.MAcceptListenTimeInterval);
            //        continue;
            //    }
            //    if (m_socketServerBase.ServerSocket.Poll(m_socketServerBase.MAcceptListenTimeInterval, SelectMode.SelectRead))
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        continue;
            //    }
            //}
            
            if (acceptEventArg == null)
            {
                //初次启动后，初始化Accept的AsyncEventArgs
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
                //acceptEventArg.UserToken = this;
            }
            else
            {
                //循环状态下，只要清除之前的AcceptSocket就可以，不用新建Accept的AsyncEventArgs
                // Socket must be cleared since the context object is being reused.
                acceptEventArg.AcceptSocket = null;
            }

            this.semaphoreAcceptedClients.WaitOne();
            Boolean willRaiseEvent = this.listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        internal void Stop()
        {
            mutex.ReleaseMutex();
        }
    }
}
