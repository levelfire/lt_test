using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

namespace servercore
{
    /// <summary>
    /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.aspx
    /// Implements the connection logic for the socket server.  
    /// After accepting a connection, all data read from the client 
    /// is sent back to the client with an aditional word, to demonstrate how to manipulate the data. 
    /// The read and echo back to the client pattern is continued until the client disconnects.
    /// </summary>
    internal sealed class SocketListener
    {
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
        private Int32 numConnectedSockets;

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
        internal SocketListener(Int32 numConnections, Int32 receiveBufferSize)
        {
            this.totalBytesRead = 0;
            this.numConnectedSockets = 0;
            this.numConnections = numConnections;

            // Allocate buffers such that the maximum number of sockets can have one outstanding read and 
            // write posted to the socket simultaneously .
            this.bufferManager = new BufferManagerA(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            this.readWritePool = new SocketAsyncEventArgsPool(numConnections);
            this.semaphoreAcceptedClients = new Semaphore(numConnections, numConnections);
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;

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

            // Free the SocketAsyncEventArg so they can be reused by another client.
            this.readWritePool.Push(e);
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

            for (Int32 i = 0; i < this.numConnections; i++)
            {
                // Preallocate a set of reusable SocketAsyncEventArgs.
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);

                // Assign a Byte buffer from the buffer pool to the SocketAsyncEventArg object.
                this.bufferManager.SetBuffer(readWriteEventArg);

                // Add SocketAsyncEventArg to the pool.
                this.readWritePool.Push(readWriteEventArg);
            }
        }

        /// <summary>
        /// Callback method associated with Socket.AcceptAsync 
        /// operations and is invoked when an accept operation is complete.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
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
            if (e.BytesTransferred > 0)
            {
                Interlocked.Increment(ref this.numConnectedSockets);
                Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                    this.numConnectedSockets);
            }

            // Get the socket for the accepted client connection and put it into the 
            // ReadEventArg object user token.
            SocketAsyncEventArgs readEventArgs = this.readWritePool.Pop();
            readEventArgs.UserToken = e.AcceptSocket;

            // As soon as the client is connected, post a receive to the connection.
            Boolean willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                this.ProcessReceive(readEventArgs);
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
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Socket s = e.UserToken as Socket;

                    Int32 bytesTransferred = e.BytesTransferred;

                    // Get the message received from the listener.
                    String received = Encoding.ASCII.GetString(e.Buffer, e.Offset, bytesTransferred);

                    // Increment the count of the total bytes receive by the server.
                    Interlocked.Add(ref this.totalBytesRead, bytesTransferred);
                    Console.WriteLine("Received: \"{0}\". The server has read a total of {1} bytes.", received, this.totalBytesRead);

                    // Format the data to send to the client.
                    Byte[] sendBuffer = Encoding.ASCII.GetBytes("Returning " + received);

                    // Set the buffer to send back to the client.
                    e.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                    Boolean willRaiseEvent = s.SendAsync(e);
                    if (!willRaiseEvent)
                    {
                        this.ProcessSend(e);
                    }
                }
                else
                {
                    this.CloseClientSocket(e);
                }
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Done echoing data back to the client.
                Socket s = e.UserToken as Socket;
                // Read the next block of data send from the client.
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

            //// Post accepts on the listening socket.
            //this.StartAccept(null);

            //mutex.WaitOne();
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
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
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
