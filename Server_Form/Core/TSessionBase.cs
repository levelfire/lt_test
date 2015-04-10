using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace servercore
{
    /// <summary>
    /// 会话基类(抽象类, 必须实现其 AnalyzeDatagram 方法)
    /// </summary>
    public abstract class TSessionBase : TSessionCoreInfo, ISessionEvent
    {
        #region  member fields

        private Socket m_socket;

        public Socket MSocket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }
        private int m_maxDatagramSize;

        // 替换buffermanager [12/12/2011 test]
        //private BufferManager m_bufferManager;

        //private int m_recvBufferOffSet;
        //private int m_sendBufferOffSet;

        // 替换buffermanager [12/12/2011 test]
        //private int m_bufferBlockIndex;
        //private byte[] m_receiveBuffer;
        //private byte[] m_sendBuffer;

        private SocketListener m_SocketLister;

        public SocketListener SocketLister
        {
            get { return m_SocketLister; }
            set { m_SocketLister = value; }
        }
        private SocketAsyncEventArgs m_ReceiveSocketArgs;
        private SocketAsyncEventArgs m_SendSocketArgs;

        private bool m_bCanSend;

        public bool BCanSend
        {
            get { return m_bCanSend; }
            set { m_bCanSend = value; }
        }

        public SocketAsyncEventArgs ReceiveSocketArgs
        {
            get { return m_ReceiveSocketArgs; }
            set { m_ReceiveSocketArgs = value; }
        }
        public SocketAsyncEventArgs SendSocketArgs
        {
            get { return m_SendSocketArgs; }
            set { m_SendSocketArgs = value; }
        } 

        private byte[] m_datagramBuffer;

        private TDatabaseBase m_databaseObj;
        private Queue<byte[]> m_datagramQueue;
        private Queue<byte[]> m_datagramQueue_send;

        public Queue<byte[]> DatagramQueue_send
        {
            get { return m_datagramQueue_send; }
            set { m_datagramQueue_send = value; }
        }

        private bool m_bFirstRe;

        public bool BFirstRe
        {
            get { return m_bFirstRe; }
            set { m_bFirstRe = value; }
        }

        #endregion

        #region class events

        public event EventHandler<TSessionExceptionEventArgs> SessionReceiveException;
        public event EventHandler<TSessionExceptionEventArgs> SessionSendException;
        public event EventHandler<TSessionEventArgs> DatagramDelimiterError;
        public event EventHandler<TSessionEventArgs> DatagramOversizeError;
        public event EventHandler<TSessionEventArgs> DatagramAccepted;
        public event EventHandler<TSessionEventArgs> DatagramError;
        public event EventHandler<TSessionEventArgs> DatagramHandled;

        //2011.11.16增加log输出
        public event EventHandler<TSessionEventArgs> DatagramLogout;

        public event EventHandler<TExceptionEventArgs> ShowDebugMessage;

        #endregion

        #region  class constructor
        /// <summary>
        /// 作泛型参数类型时, 必须有无参构造函数
        /// </summary>
        protected TSessionBase() { }

        public abstract void SubInit(object socket);
        /// <summary>
        /// 替构造函数初始化对象
        /// </summary>
        public virtual void Initiate(int maxDatagramsize, int id, Socket socket, TDatabaseBase database, SocketListener SListener/*BufferManager bufferManager*/)
        {
            base.ID = id;
            base.LoginTime = DateTime.Now;

            // 替换buffermanager [12/12/2011 test]
            //m_bufferManager = bufferManager;

            //m_recvBufferOffSet = bufferManager.GetRecvBuffer();
            //m_sendBufferOffSet = bufferManager.GetSendBuffer();

            // 替换buffermanager [12/12/2011 test]
            //m_bufferBlockIndex = bufferManager.GetBufferBlockIndex();

            //if (m_bufferBlockIndex == -1)  // 没有空块, 新建
            //{
            //    m_receiveBuffer = new byte[m_bufferManager.ReceiveBufferSize];
            //    m_sendBuffer = new byte[m_bufferManager.SendBufferSize];
            //}
            //else
            //{
            //    m_receiveBuffer = m_bufferManager.ReceiveBuffer;
            //    m_sendBuffer = m_bufferManager.SendBuffer;
            //}

            m_SocketLister = SListener;
            m_ReceiveSocketArgs = null;
            m_SendSocketArgs = null;

            m_bCanSend = true;

            m_maxDatagramSize = maxDatagramsize;

            m_socket = socket;
            m_databaseObj = database;

            m_datagramQueue = new Queue<byte[]>();
            //创建发送消息队列
            m_datagramQueue_send = new Queue<byte[]>();
            if (m_socket != null)
            {
                IPEndPoint iep = m_socket.RemoteEndPoint as IPEndPoint;
                if (iep != null)
                {
                    base.IP = iep.Address.ToString();
                }
            }
        }

        #endregion

        #region  properties

        public TDatabaseBase DatabaseObj
        {
            get { return m_databaseObj; }
        }

        #endregion

        #region  public methods

        public void Shutdown()
        {
            lock (this)
            {
                if (this.State != TSessionState.Inactive || m_socket == null)  // Inactive 状态才能 Shutdown
                {
                    return;
                }

                this.State = TSessionState.Shutdown;
                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);  // 目的：结束异步事件
                }
                catch (Exception) { }
            }
        }

        public virtual void Close()
        {
            lock (this)
            {
                if (this.State != TSessionState.Shutdown || m_socket == null)  // Shutdown 状态才能 Close
                {
                    return;
                }

                m_datagramBuffer = null;

                if (m_datagramQueue != null)
                {
                    while (m_datagramQueue.Count > 0)
                    {

                        m_datagramQueue.Dequeue();
                    }
                    m_datagramQueue.Clear();
                }

                 //清除发送消息队列
                if (m_datagramQueue_send != null)
                {
                    while (m_datagramQueue_send.Count > 0)
                    {

                        m_datagramQueue_send.Dequeue();
                    }
                    m_datagramQueue_send.Clear();
                }

                //m_bufferManager.FreeRecvBuffer(m_recvBufferOffSet);
                //m_bufferManager.FreeSendBuffer(m_sendBufferOffSet);

                // 替换buffermanager [12/12/2011 test]
                //m_bufferManager.FreeBufferBlockIndex(m_bufferBlockIndex);

                try
                {
                    this.State = TSessionState.Closed;

                    //SocketLister.CloseClientSocket(ReceiveSocketArgs);
                    //SocketLister.PushArgs(ReceiveSocketArgs);

                    SocketLister.CloseClientSocket(m_socket,ReceiveSocketArgs, SendSocketArgs);
                    //SocketLister.CloseClientSocket(m_socket, ReceiveSocketArgs);
                    // 使用完成端口的清理版本 [12/14/2011 test]
                    //m_socket.Close();
                }
                catch (Exception) { }
            }
        }

        public void SetInactive()
        {
            lock (this)
            {
                if (this.State == TSessionState.Active)
                {
                    this.State = TSessionState.Inactive;
                    this.DisconnectType = TDisconnectType.Normal;
                }
            }
        }

        public void HandleDatagram()
        {
            lock (this)
            {
                if (this.State != TSessionState.Active || m_datagramQueue.Count == 0)
                {
                    return;
                }

                byte[] datagramBytes = m_datagramQueue.Dequeue();
                this.AnalyzeDatagram(datagramBytes);
            }
        }

        public void HandleDatagram_Send()
        {
            lock (this)
            {
                if (m_bCanSend)
                {
                    if (this.State != TSessionState.Active || m_datagramQueue_send.Count == 0)
                    {
                        return;
                    }
                    Logout = "HandleDatagram_Send";
                    OnDatagramlogOut();

                    byte[] datagramBytes = m_datagramQueue_send.Dequeue();
                    //要做复制操作，不能把原来的内存替换掉，否则回收的时候，内存管理无法正确回收原来的内存段
                    //SendSocketArgs.SetBuffer(datagramBytes, 0, datagramBytes.Length);
                    Array.Copy(datagramBytes, 0, SendSocketArgs.Buffer, SendSocketArgs.Offset, datagramBytes.Length);

                    SendSocketArgs.SetBuffer(SendSocketArgs.Offset, datagramBytes.Length);
                    m_bCanSend = false;
                    Boolean willRaiseEvent = this.m_socket.SendAsync(SendSocketArgs);

                    if (!willRaiseEvent)
                    {
                        m_SocketLister.ProcessSend(SendSocketArgs);
                    }
                }
            }
        }

        //  使用完成端口[12/12/2011 test]
        public void ReceiveDatagram(SocketAsyncEventArgs e)
        {
            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                try  // 一个客户端连续做连接 或连接后立即断开，容易在该处产生错误，系统不认为是错误
                {
                    if (e.BytesTransferred > 0)
                    {
                        Interlocked.Increment(ref m_SocketLister.numConnectedSockets);
                        Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                            m_SocketLister.numConnectedSockets);
                    }

                    // Get the socket for the accepted client connection and put it into the 
                    // ReadEventArg object user token.

                    //此处用PopOrNew也没有什么用处，即使新建了args，buffermanager中不会创建额外内存空间，之后还是要改回pop
                    // 先留着，之后再改 [12/26/2011 test]
                    bool bNew;
                    SocketAsyncEventArgs readEventArgs = m_SocketLister.ReadWritePool.PopOrNew(out bNew);
                    if (bNew)
                    {
                        readEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_SocketLister.OnIOCompleted);
                    }
                    SocketAsyncEventArgs sendEventArgs = m_SocketLister.ReadWritePool.PopOrNew_send(out bNew);
                    if (bNew)
                    {
                        sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(m_SocketLister.OnIOCompleted);
                    }

                    //readEventArgs.UserToken = e.AcceptSocket;
                    readEventArgs.UserToken = this;
                    sendEventArgs.UserToken = this;
                    m_ReceiveSocketArgs = readEventArgs;
                    m_SendSocketArgs = sendEventArgs;

                    // As soon as the client is connected, post a receive to the connection.
                    Boolean willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
                    if (!willRaiseEvent)
                    {
                        m_SocketLister.ProcessReceive(readEventArgs);
                    }

                    // 改用完成端口 [12/12/2011 test]
                    //// 开始接受来自该客户端的数据
                    ////byte[] recvBuffer = m_bufferManager.RecvBuffer;
                    ////m_socket.BeginReceive(recvBuffer, m_recvBufferOffSet, m_bufferManager.RecvBufferSize, SocketFlags.None, this.EndReceiveDatagram, this);

                    //int bufferOffset = m_bufferManager.GetReceivevBufferOffset(m_bufferBlockIndex);
                    //m_socket.BeginReceive(m_receiveBuffer, bufferOffset, m_bufferManager.ReceiveBufferSize, SocketFlags.None, this.EndReceiveDatagram, this);
                }
                catch (Exception err)  // 读 Socket 异常，准备关闭该会话
                {
                    this.DisconnectType = TDisconnectType.Exception;
                    this.State = TSessionState.Inactive;

                    this.OnSessionReceiveException(err);
                }
            }
        }

        public void ReceiveDatagram()
        {
            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                try  // 一个客户端连续做连接 或连接后立即断开，容易在该处产生错误，系统不认为是错误
                {
                    //// 开始接受来自该客户端的数据
                    ////byte[] recvBuffer = m_bufferManager.RecvBuffer;
                    ////m_socket.BeginReceive(recvBuffer, m_recvBufferOffSet, m_bufferManager.RecvBufferSize, SocketFlags.None, this.EndReceiveDatagram, this);

                    //int bufferOffset = m_bufferManager.GetReceivevBufferOffset(m_bufferBlockIndex);
                    //m_socket.BeginReceive(m_receiveBuffer, bufferOffset, m_bufferManager.ReceiveBufferSize, SocketFlags.None, this.EndReceiveDatagram, this);
                }
                catch (Exception err)  // 读 Socket 异常，准备关闭该会话
                {
                    this.DisconnectType = TDisconnectType.Exception;
                    this.State = TSessionState.Inactive;

                    this.OnSessionReceiveException(err);
                }
            }
        }
        // 原架构只接受字符串，现在重载接受字节流，不走原来的转换流程，不使用buffermanager  [12/9/2011 test]
        public void SendDatagram(byte[] datagramByte)
        {
            //m_datagramQueue_send.Enqueue(datagramByte);

            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                try
                {
                    ////  [1/4/2012 test]尝试使用完成端口发送消息
                    // 目前在此处使用SendAsync方法时，会出现Args已使用的错误
                    // 可能是由于在未处理完上次操作时，又有新的send操作
                    // 如果要使用此方法，需要在之后尝试实现一个send消息队列，逐条发送
                    // 在ProcessSend中实现发送下一条，避免冲突
                    // 目前先使用BeginSend来代替[1/4/2012 test]
                    // SendSocketArgs.SetBuffer(datagramByte, 0, datagramByte.Length);
                    //Boolean willRaiseEvent = this.m_socket.SendAsync(SendSocketArgs);

                    //if (!willRaiseEvent)
                    //{
                    //    m_SocketLister.ProcessSend(SendSocketArgs);
                    //}

                    m_socket.BeginSend(datagramByte, 0, datagramByte.Length, SocketFlags.None, this.EndSendDatagram, this);
                }
                catch (Exception err)  // 写 socket 异常，准备关闭该会话
                {
                    this.DisconnectType = TDisconnectType.Exception;
                    this.State = TSessionState.Inactive;

                    this.OnSessionSendException(err);
                }
            }
        }

        public void SendDatagram(string datagramText)
        {
            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                try
                {
                    int byteLength = Encoding.ASCII.GetByteCount(datagramText);
                    //if (byteLength <= m_bufferManager.SendBufferSize)
                    //{
                    //    //byte[] sendBuffer = m_bufferManager.SendBuffer;
                    //    //Encoding.ASCII.GetBytes(datagramText, 0, byteLength, sendBuffer, m_sendBufferOffSet);
                    //    //m_socket.BeginSend(sendBuffer, m_sendBufferOffSet, byteLength, SocketFlags.None, this.EndSendDatagram, this);

                    //    int bufferOffset = m_bufferManager.GetSendBufferOffset(m_bufferBlockIndex);
                    //    Encoding.ASCII.GetBytes(datagramText, 0, byteLength, m_sendBuffer, bufferOffset);
                    //    m_socket.BeginSend(m_sendBuffer, bufferOffset, byteLength, SocketFlags.None, this.EndSendDatagram, this);
                    //}
                    //else
                    //{
                    //    byte[] data = Encoding.ASCII.GetBytes(datagramText);  // 获得数据字节数组
                    //    m_socket.BeginSend(data, 0, data.Length, SocketFlags.None, this.EndSendDatagram, this);
                    //}
                }
                catch (Exception err)  // 写 socket 异常，准备关闭该会话
                {
                    this.DisconnectType = TDisconnectType.Exception;
                    this.State = TSessionState.Inactive;

                    this.OnSessionSendException(err);
                }
            }
        }

        public void CheckTimeout(int maxSessionTimeout)
        {
            TimeSpan ts = DateTime.Now.Subtract(this.LastSessionTime);
            int elapsedSecond = Math.Abs((int)ts.TotalSeconds);

            if (elapsedSecond > maxSessionTimeout)  // 超时，则准备断开连接
            {
                this.DisconnectType = TDisconnectType.Timeout;
                this.SetInactive();  // 标记为将关闭、准备断开
            }
        }

        #endregion

        #region  private methods

        /// <summary>
        /// 发送数据完成处理函数, iar 为目标客户端 Session
        /// </summary>
        private void EndSendDatagram(IAsyncResult iar)
        {
            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                if (!m_socket.Connected)
                {
                    this.SetInactive();
                    return;
                }

                try
                {
                    m_socket.EndSend(iar);
                    iar.AsyncWaitHandle.Close();
                }
                catch (Exception err)  // 写 socket 异常，准备关闭该会话
                {
                    this.DisconnectType = TDisconnectType.Exception;
                    this.State = TSessionState.Inactive;

                    this.OnSessionSendException(err);
                }
            }
        }

        private void EndReceiveDatagram(IAsyncResult iar)
        {
            lock (this)
            {
                if (this.State != TSessionState.Active)
                {
                    return;
                }

                if (!m_socket.Connected)
                {
                    this.SetInactive();
                    return;
                }

                try
                {
                    // Shutdown 时将调用 ReceiveData，此时也可能收到 0 长数据包
                    int readBytesLength = m_socket.EndReceive(iar);
                    //                    iar.AsyncWaitHandle.Close();

                    if (readBytesLength == 0)
                    {
                        this.DisconnectType = TDisconnectType.Normal;
                        this.State = TSessionState.Inactive;
                    }
                    else  // 正常数据包
                    {
                        this.LastSessionTime = DateTime.Now;
                        // 合并报文，按报文头、尾字符标志抽取报文，将包交给数据处理器
                        this.ResolveSessionBuffer(readBytesLength);
                        this.ReceiveDatagram();  // 继续接收
                    }
                }
                catch (Exception err)  // 读 socket 异常，关闭该会话，系统不认为是错误（这种错误可能太多）
                {
                    if (this.State == TSessionState.Active)
                    {
                        this.DisconnectType = TDisconnectType.Exception;
                        this.State = TSessionState.Inactive;

                        this.OnSessionReceiveException(err);
                    }
                }
            }
        }

        /// <summary>
        /// 拷贝接收缓冲区的数据到数据缓冲区（即多次读一个包文）
        /// </summary>
        private void CopyToDatagramBuffer(SocketAsyncEventArgs e,int start, int length)
        {
            int datagramLength = 0;
            if (m_datagramBuffer != null)
            {
                datagramLength = m_datagramBuffer.Length;
            }

            //byte[] recvBuffer = m_bufferManager.RecvBuffer;
            Array.Resize(ref m_datagramBuffer, datagramLength + length);  // 调整长度（m_datagramBuffer 为 null 不出错）
            Array.Copy(e.Buffer, start, m_datagramBuffer, datagramLength, length);  // 拷贝到数据包缓冲区
        }
        /// <summary>
        /// 拷贝接收缓冲区的数据到数据缓冲区（即多次读一个包文）
        /// </summary>
        private void CopyToDatagramBuffer(int start, int length)
        {
            int datagramLength = 0;
            if (m_datagramBuffer != null)
            {
                datagramLength = m_datagramBuffer.Length;
            }

            ////byte[] recvBuffer = m_bufferManager.RecvBuffer;
            //Array.Resize(ref m_datagramBuffer, datagramLength + length);  // 调整长度（m_datagramBuffer 为 null 不出错）
            //Array.Copy(m_receiveBuffer, start, m_datagramBuffer, datagramLength, length);  // 拷贝到数据包缓冲区
        }

        #endregion

        #region protected methods

        /// <summary>
        /// 提取包时与包规则紧密相关，根据实际规则重定义
        /// </summary>
        public virtual void ResolveSessionBuffer(SocketAsyncEventArgs e)
        {
            // 上次留下包文非空, 必然含开始字符<
            bool hasHeadDelimiter = (m_datagramBuffer != null);

            int headDelimiter = 1;
            int tailDelimiter = 1;

            //byte[] recvBuffer = m_bufferManager.RecvBuffer;
            //int start = m_recvBufferOffSet;   // m_recvBuffer 缓冲区中包开始位置

            int bufferOffset = e.Offset;
            int start = bufferOffset;   // m_receiveBuffer 缓冲区中包开始位置

            int length = 0;  // 已经搜索的接收缓冲区长度

            int subIndex = bufferOffset;  // 缓冲区下标

            int endIndex = e.BytesTransferred + bufferOffset;  // 接收数据末尾

            while (subIndex < e.BytesTransferred + bufferOffset)
            {
                if (e.Buffer[subIndex] == '<')  // 数据包开始字符<，前面包文作废
                {
                    if (hasHeadDelimiter || length > 0)  // 如果 < 前面有数据，则认为错误包
                    {
                        this.OnDatagramDelimiterError();
                    }

                    m_datagramBuffer = null;  // 清空包缓冲区，开始一个新的包

                    start = subIndex;         // 新包起点，即<所在位置
                    length = headDelimiter;   // 新包的长度（即<）
                    hasHeadDelimiter = true;  // 新包有开始字符
                }
                else if (e.Buffer[subIndex] == '>')  // 数据包的结束字符>
                {
                    if (hasHeadDelimiter)  // 两个缓冲区中有开始字符<
                    {
                        length += tailDelimiter;  // 长度包括结束字符“>”

                        this.GetDatagramFromBuffer(e,start, length); // >前面的为正确格式的包

                        start = subIndex + tailDelimiter;  // 新包起点（一般一次处理将结束循环）
                        length = 0;  // 新包长度

                        // 处理一次后，之后的包肯定都是安游戏结构生成的 [11/30/2011 test]
                        if (BFirstRe)
                        {
                            BFirstRe = false;
                        }
                    }
                    else  // >前面没有开始字符，此时认为结束字符>为一般字符，待后续的错误包处理
                    {
                        length++;  //  hasHeadDelimiter = false;
                    }
                }
                else  // 即非 < 也非 >， 是一般字符，长度 + 1
                {
                    length++;
                    //  [11/30/2011 test]由于客户端使用的10版本，会在连接时默认丢一个请求上来，非游戏结构，所以对第一次的数据不能做优化处理
                    if (!BFirstRe)
                    {
                        //获取
                        // 根据游戏包结构，优化解析过程 [11/30/2011 test]
                        //此时为“< + 包长”
                        if (length == 5)
                        {
                            //获得包长度
                            int nLenth = System.BitConverter.ToInt32(e.Buffer, start + 1);
                            //长度小于接收末尾，直接跳到长度位置
                            if ((1 + nLenth) < endIndex)
                            {
                                length += (nLenth - 4);
                                subIndex += (1 + nLenth - 4);
                                continue;
                            }
                            //长度大等于接收末尾，跳到接收末尾
                            else
                            {
                                ////subIndex = endIndex;
                                length += (endIndex - (subIndex + 1));
                                subIndex = endIndex;
                                break;
                            }
                        }
                    }
                }
                ++subIndex;
            }

            if (length > 0)  // 剩下的待处理串，分两种情况
            {
                int mergedLength = length;
                if (m_datagramBuffer != null)
                {
                    mergedLength += m_datagramBuffer.Length;
                }

                // 剩下的包文含首字符且不超长，转存到包文缓冲区中，待下次处理
                if (hasHeadDelimiter && mergedLength <= m_maxDatagramSize)
                {
                    this.CopyToDatagramBuffer(e, start, length);
                }
                else  // 不含首字符或超长
                {
                    this.OnDatagramOversizeError();
                    m_datagramBuffer = null;  // 丢弃全部数据
                }
            }
        }
        /// <summary>
        /// 提取包时与包规则紧密相关，根据实际规则重定义
        /// </summary>
        public virtual void ResolveSessionBuffer(int readBytesLength)
        {
            //// 上次留下包文非空, 必然含开始字符<
            //bool hasHeadDelimiter = (m_datagramBuffer != null);

            //int headDelimiter = 1;
            //int tailDelimiter = 1;

            ////byte[] recvBuffer = m_bufferManager.RecvBuffer;
            ////int start = m_recvBufferOffSet;   // m_recvBuffer 缓冲区中包开始位置

            //int bufferOffset = m_bufferManager.GetReceivevBufferOffset(m_bufferBlockIndex);
            //int start = bufferOffset;   // m_receiveBuffer 缓冲区中包开始位置

            //int length = 0;  // 已经搜索的接收缓冲区长度

            ////int subIndex = m_recvBufferOffSet;  // 缓冲区下标
            //int subIndex = bufferOffset;  // 缓冲区下标

            ////int endIndex = readBytesLength + m_recvBufferOffSet;  // 接收数据末尾
            //int endIndex = readBytesLength + bufferOffset;  // 接收数据末尾

            //while (subIndex < readBytesLength + bufferOffset)
            //{
            //    if (m_receiveBuffer[subIndex] == '<')  // 数据包开始字符<，前面包文作废
            //    {
            //        if (hasHeadDelimiter || length > 0)  // 如果 < 前面有数据，则认为错误包
            //        {
            //            this.OnDatagramDelimiterError();
            //        }

            //        m_datagramBuffer = null;  // 清空包缓冲区，开始一个新的包

            //        start = subIndex;         // 新包起点，即<所在位置
            //        length = headDelimiter;   // 新包的长度（即<）
            //        hasHeadDelimiter = true;  // 新包有开始字符
            //    }
            //    else if (m_receiveBuffer[subIndex] == '>')  // 数据包的结束字符>
            //    {
            //        if (hasHeadDelimiter)  // 两个缓冲区中有开始字符<
            //        {
            //            length += tailDelimiter;  // 长度包括结束字符“>”

            //            this.GetDatagramFromBuffer(start, length); // >前面的为正确格式的包

            //            start = subIndex + tailDelimiter;  // 新包起点（一般一次处理将结束循环）
            //            length = 0;  // 新包长度

            //            // 处理一次后，之后的包肯定都是安游戏结构生成的 [11/30/2011 test]
            //            if (BFirstRe)
            //            {
            //                BFirstRe = false;
            //            }
            //        }
            //        else  // >前面没有开始字符，此时认为结束字符>为一般字符，待后续的错误包处理
            //        {
            //            length++;  //  hasHeadDelimiter = false;
            //        }
            //    }
            //    else  // 即非 < 也非 >， 是一般字符，长度 + 1
            //    {
            //        length++;
            //        //  [11/30/2011 test]由于客户端使用的10版本，会在连接时默认丢一个请求上来，非游戏结构，所以对第一次的数据不能做优化处理
            //        if (!BFirstRe)
            //        {
            //            //获取
            //            // 根据游戏包结构，优化解析过程 [11/30/2011 test]
            //            //此时为“< + 包长”
            //            if (length == 5)
            //            {
            //                //获得包长度
            //                int nLenth = System.BitConverter.ToInt32(m_receiveBuffer, start + 1);
            //                //长度小于接收末尾，直接跳到长度位置
            //                if ((1 + nLenth) < endIndex)
            //                {
            //                    length += (nLenth - 4);
            //                    subIndex += (1 + nLenth - 4);
            //                    continue;
            //                }
            //                //长度大等于接收末尾，跳到接收末尾
            //                else
            //                {
            //                    ////subIndex = endIndex;
            //                    length += (endIndex - (subIndex + 1));
            //                    subIndex = endIndex;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    ++subIndex;
            //}


            //if (length > 0)  // 剩下的待处理串，分两种情况
            //{
            //    int mergedLength = length;
            //    if (m_datagramBuffer != null)
            //    {
            //        mergedLength += m_datagramBuffer.Length;
            //    }

            //    // 剩下的包文含首字符且不超长，转存到包文缓冲区中，待下次处理
            //    if (hasHeadDelimiter && mergedLength <= m_maxDatagramSize)
            //    {
            //        this.CopyToDatagramBuffer(start, length);
            //    }
            //    else  // 不含首字符或超长
            //    {
            //        this.OnDatagramOversizeError();
            //        m_datagramBuffer = null;  // 丢弃全部数据
            //    }
            //}
        }

        /// <summary>
        /// Session重写入口, 基本功能: 
        /// 1) 判断包有效性与包类型(注意：包带起止符号); 
        /// 2) 分解包中的各字段数据
        /// 3) 校验包及其数据有效性
        /// 4) 发送确认消息给客户端(调用方法 SendDatagram())
        /// 5) 存储包数据到数据库中
        /// 6) 存储包原文到数据库中(可选)
        /// 7) 补充字段m_name, 表示数据包发送者的名称/编号
        /// 8) 其它相关方法
        /// </summary>
        protected abstract void AnalyzeDatagram(byte[] datagramBytes);

        
        protected virtual void GetDatagramFromBuffer(SocketAsyncEventArgs e,int startPos, int len)
        {
            //byte[] recvBuffer = m_bufferManager.RecvBuffer;
            byte[] datagramBytes;
            if (m_datagramBuffer != null)
            {
                datagramBytes = new byte[len + m_datagramBuffer.Length];
                Array.Copy(m_datagramBuffer, 0, datagramBytes, 0, m_datagramBuffer.Length);  // 先拷贝 Session 的数据缓冲区的数据
                Array.Copy(e.Buffer, startPos, datagramBytes, m_datagramBuffer.Length, len);  // 再拷贝 Session 的接收缓冲区的数据
            }
            else
            {
                datagramBytes = new byte[len];
                Array.Copy(e.Buffer, startPos, datagramBytes, 0, len);  // 再拷贝 Session 的接收缓冲区的数据
            }

            if (m_datagramBuffer != null)
            {
                m_datagramBuffer = null;
            }

            m_datagramQueue.Enqueue(datagramBytes);
        }
        protected virtual void GetDatagramFromBuffer(int startPos, int len)
        {
            ////byte[] recvBuffer = m_bufferManager.RecvBuffer;
            //byte[] datagramBytes;
            //if (m_datagramBuffer != null)
            //{
            //    datagramBytes = new byte[len + m_datagramBuffer.Length];
            //    Array.Copy(m_datagramBuffer, 0, datagramBytes, 0, m_datagramBuffer.Length);  // 先拷贝 Session 的数据缓冲区的数据
            //    Array.Copy(m_receiveBuffer, startPos, datagramBytes, m_datagramBuffer.Length, len);  // 再拷贝 Session 的接收缓冲区的数据
            //}
            //else
            //{
            //    datagramBytes = new byte[len];
            //    Array.Copy(m_receiveBuffer, startPos, datagramBytes, 0, len);  // 再拷贝 Session 的接收缓冲区的数据
            //}

            //if (m_datagramBuffer != null)
            //{
            //    m_datagramBuffer = null;
            //}

            //m_datagramQueue.Enqueue(datagramBytes);
        }

        protected virtual void OnDatagramDelimiterError()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramDelimiterError;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        protected virtual void OnDatagramOversizeError()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramOversizeError;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        protected virtual void OnDatagramAccepted()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramAccepted;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        protected virtual void OnDatagramError()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramError;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        protected virtual void OnDatagramHandled()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramHandled;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        protected virtual void OnDatagramlogOut()
        {
            EventHandler<TSessionEventArgs> handler = this.DatagramLogout;
            if (handler != null)
            {
                TSessionEventArgs e = new TSessionEventArgs(this);
                handler(this, e);
            }
        }

        public virtual void OnSessionReceiveException(Exception err)
        {
            EventHandler<TSessionExceptionEventArgs> handler = this.SessionReceiveException;
            if (handler != null)
            {
                TSessionExceptionEventArgs e = new TSessionExceptionEventArgs(err, this);
                handler(this, e);
            }
        }

        protected virtual void OnSessionSendException(Exception err)
        {
            EventHandler<TSessionExceptionEventArgs> handler = this.SessionSendException;
            if (handler != null)
            {
                TSessionExceptionEventArgs e = new TSessionExceptionEventArgs(err, this);
                handler(this, e);
            }
        }

        protected void OnShowDebugMessage(string message)
        {
            if (this.ShowDebugMessage != null)
            {
                TExceptionEventArgs e = new TExceptionEventArgs(message);
                this.ShowDebugMessage(this, e);
            }
        }

        #endregion
    }
}
