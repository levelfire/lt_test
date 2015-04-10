using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace ZYSocketSuper
{

    /// <summary>
    /// 连接的代理
    /// </summary>
    /// <param name="socketAsync"></param>
    public delegate bool ConnectionFilter(SocketAsyncEventArgs socketAsync);

    /// <summary>
    /// 数据包输入代理
    /// </summary>
    /// <param name="data">输入包</param>
    /// <param name="socketAsync"></param>
    public delegate void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync);

    /// <summary>
    /// 异常错误通常是用户断开的代理
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="socketAsync"></param>
    /// <param name="erorr">错误代码</param>
    public delegate void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr);

    public class ZYSocketSuper : IDisposable
    {

        #region 释放
        /// <summary>
        /// 用来确定是否以释放
        /// </summary>
        private bool isDisposed;


        ~ZYSocketSuper()
        {
            this.Dispose(false);

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                try
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                   
                }
                catch
                {
                }

                isDisposed = true;
            }
        }
        #endregion

        /// <summary>
        /// 数据包管理
        /// </summary>
        private BufferManager BuffManagers;

        /// <summary>
        /// Socket异步对象池
        /// </summary>
        private SocketAsyncEventArgsPool SocketAsynPool;

        /// <summary>
        /// SOCK对象
        /// </summary>
        private Socket sock;

        /// <summary>
        /// Socket对象
        /// </summary>
        public Socket Sock { get { return sock; } }


        /// <summary>
        /// 连接传入处理
        /// </summary>
        public ConnectionFilter Connetions { get; set; }

        /// <summary>
        /// 数据输入处理
        /// </summary>
        public BinaryInputHandler BinaryInput { get; set; }

        /// <summary>
        /// 异常错误通常是用户断开处理
        /// </summary>
        public MessageInputHandler MessageInput { get; set; }



        /// <summary>
        /// 是否开启SOCKET Delay算法
        /// </summary>
        public bool NoDelay
        {
            get
            {
                return sock.NoDelay;
            }

            set
            {
                sock.NoDelay = value;
            }

        }

        /// <summary>
        /// SOCKET 的  ReceiveTimeout属性
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return sock.ReceiveTimeout;
            }

            set
            {
                sock.ReceiveTimeout = value;

            }


        }

        /// <summary>
        /// SOCKET 的 SendTimeout
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return sock.SendTimeout;
            }

            set
            {
                sock.SendTimeout = value;
            }

        }


        /// <summary>
        /// 接收包大小
        /// </summary>
        private int MaxBufferSize;

        public int GetMaxBufferSize
        {
            get
            {
                return MaxBufferSize;
            }
        }

        /// <summary>
        /// 最大用户连接
        /// </summary>
        private int MaxConnectCout;

        /// <summary>
        /// 最大用户连接数
        /// </summary>
        public int GetMaxUserConnect
        {
            get
            {
                return MaxConnectCout;
            }
        }


        /// <summary>
        /// IP
        /// </summary>
        private string Host;

        /// <summary>
        /// 端口
        /// </summary>
        private int Port;



        #region 消息输出
        /// <summary>
        /// 输出消息
        /// </summary>
        public event EventHandler<LogOutEventArgs> MessageOut;


        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="o"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        protected void LogOutEvent(Object sender, LogType type, string message)
        {
            if (MessageOut != null)
                MessageOut.BeginInvoke(sender, new LogOutEventArgs(type, message), new AsyncCallback(CallBackEvent), MessageOut);

        }
        /// <summary>
        /// 事件处理完的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void CallBackEvent(IAsyncResult ar)
        {
            EventHandler<LogOutEventArgs> MessageOut = ar.AsyncState as EventHandler<LogOutEventArgs>;
            if (MessageOut != null)
                MessageOut.EndInvoke(ar);
        }
        #endregion

       

        public ZYSocketSuper(string host, int port, int maxconnectcout, int maxbuffersize)
        {
            this.Port = port;
            this.Host = host;
            this.MaxBufferSize = maxbuffersize;
            this.MaxConnectCout = maxconnectcout;          

        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (isDisposed == true)
            {
                throw new ObjectDisposedException("ZYServer is Disposed");
            }

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            IPEndPoint myEnd = (String.IsNullOrEmpty(Host)) ? (new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], Port)) : (new IPEndPoint(IPAddress.Parse(Host), Port));

            sock.Bind(myEnd);
            sock.Listen(20);
            SendTimeout = 1000;
            ReceiveTimeout = 1000;

            BuffManagers = new BufferManager(MaxConnectCout * MaxBufferSize, MaxBufferSize);
            BuffManagers.Inint();

            SocketAsynPool = new SocketAsyncEventArgsPool(MaxConnectCout);

            for (int i = 0; i < MaxConnectCout; i++)
            {
                SocketAsyncEventArgs socketasyn = new SocketAsyncEventArgs();
                socketasyn.Completed += new EventHandler<SocketAsyncEventArgs>(Asyn_Completed);
                SocketAsynPool.Push(socketasyn);
            }

            Accept();
        }



        void Accept()
        {
            if (SocketAsynPool.Count > 0)
            {
                SocketAsyncEventArgs sockasyn = SocketAsynPool.Pop();
                if (!Sock.AcceptAsync(sockasyn))
                {
                    BeginAccep(sockasyn);
                }
            }
            else
            {
                LogOutEvent(null, LogType.Error, "The MaxUserCout");
            }
        }

        void BeginAccep(SocketAsyncEventArgs e)
        {
            try
            {

                if (e.SocketError == SocketError.Success)
                {
                   

                    if (this.Connetions != null)
                        if (!this.Connetions(e))
                        {

                            LogOutEvent(null, LogType.Error, string.Format("The Socket Not Connect {0}", e.AcceptSocket.RemoteEndPoint));
                            e.AcceptSocket = null;
                            SocketAsynPool.Push(e);

                            return;
                        }

                   

                    if (BuffManagers.SetBuffer(e))
                    {
                        if (!e.AcceptSocket.ReceiveAsync(e))
                        {
                            BeginReceive(e);
                        }
                       
                    }

                }
                else
                {
                    e.AcceptSocket = null;
                    SocketAsynPool.Push(e);                 
                    LogOutEvent(null, LogType.Error, "Not Accep");
                }
            }
            finally
            {
                Accept();
            }

        }

       


        void BeginReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success&&e.BytesTransferred>0)
            {
                byte[] data = new byte[e.BytesTransferred];

                Array.Copy(e.Buffer, e.Offset, data, 0, data.Length);

                if (this.BinaryInput != null)
                    this.BinaryInput.BeginInvoke(data, e, RecevieCallBack, BinaryInput);

                if (!e.AcceptSocket.ReceiveAsync(e))
                {
                    BeginReceive(e);
                }
            }
            else
            {
                string message=string.Format("User Disconnect :{0}", e.AcceptSocket.RemoteEndPoint.ToString());

                LogOutEvent(null, LogType.Error, message);

                if (MessageInput != null)
                {
                    MessageInput(message, e, 0);
                }

                e.AcceptSocket = null;
                BuffManagers.FreeBuffer(e);
                SocketAsynPool.Push(e);
                if (SocketAsynPool.Count == 1)
                {
                    Accept();
                }
            }

        }

        void RecevieCallBack(IAsyncResult result)
        {
            this.BinaryInput.EndInvoke(result);

        }

        void Asyn_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    BeginAccep(e);
                    break;
                case SocketAsyncOperation.Receive:
                    BeginReceive(e);
                    break;
               
            }
            
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="data"></param>
        public void SendData(Socket sock, byte[] data)
        {
            sock.BeginSend(data, 0, data.Length, SocketFlags.None, AsynCallBack, sock);

        }

        void AsynCallBack(IAsyncResult result)
        {
            try
            {
                Socket sock = result.AsyncState as Socket;

                if (sock != null)
                {
                    sock.EndSend(result);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 断开此SOCKET
        /// </summary>
        /// <param name="sock"></param>
        public void Disconnect(Socket sock)
        {
            sock.BeginDisconnect(false, AsynCallBackDisconnect, sock);

        }

        void AsynCallBackDisconnect(IAsyncResult result)
        {
            Socket sock = result.AsyncState as Socket;

            if (sock != null)
            {
                sock.EndDisconnect(result);
            }
        }

    }

    public enum LogType
    {
        Error,
    }


    public class LogOutEventArgs : EventArgs
    {

        /// <summary>
        /// 消息类型
        /// </summary>     
        private LogType messClass;

        /// <summary>
        /// 消息类型
        /// </summary>  
        public LogType MessClass
        {
            get { return messClass; }
        }



        /// <summary>
        /// 消息
        /// </summary>
        private string mess;

        public string Mess
        {
            get { return mess; }
        }

        public LogOutEventArgs(LogType messclass, string str)
        {
            messClass = messclass;
            mess = str;

        }


    }
}
