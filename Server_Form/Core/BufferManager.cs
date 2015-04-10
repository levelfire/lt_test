//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace servercore
{
    /// <summary>
    /// 发送和接收公共缓冲区
    /// </summary>
    //public sealed class BufferManager
    //{
    //    private byte[] m_recvBuffer;
    //    private byte[] m_sendBuffer;

    //    private int m_maxSessionCount;// 最大可连接客户端数

    //    private int m_recvBufferSize;// 接收缓冲区大小
    //    private int m_sendBufferSize;

    //    private int m_totalRecvLength;
    //    private int m_totalSendLength;

    //    private int m_currentRecvIndex;
    //    private int m_currentSendIndex;

    //    private Stack<int> m_freeRecvIndexStack;
    //    private Stack<int> m_freeSendIndexStack;

    //    public BufferManager(int maxSessionCount, int recvBufferSize, int sendBufferSize)
    //    {
    //        m_maxSessionCount = maxSessionCount;

    //        m_recvBufferSize = recvBufferSize;
    //        m_sendBufferSize = sendBufferSize;

    //        m_currentRecvIndex = 0;
    //        m_currentSendIndex = 0;

    //        m_freeRecvIndexStack = new Stack<int>();
    //        m_freeSendIndexStack = new Stack<int>();

    //        m_totalRecvLength = m_recvBufferSize * m_maxSessionCount;
    //        m_totalSendLength = m_sendBufferSize * m_maxSessionCount;

    //        m_recvBuffer = new byte[m_totalRecvLength];
    //        m_sendBuffer = new byte[m_totalSendLength];
    //    }

    //    public int RecvBufferSize
    //    {
    //        get { return m_recvBufferSize; }
    //    }

    //    public int SendBufferSize
    //    {
    //        get { return m_sendBufferSize; }
    //    }

    //    public byte[] RecvBuffer
    //    {
    //        get { return m_recvBuffer; }
    //    }

    //    public byte[] SendBuffer
    //    {
    //        get { return m_sendBuffer; }
    //    }

    //    public void FreeRecvBuffer(int recvOffSet)
    //    {
    //        m_freeRecvIndexStack.Push(recvOffSet);
    //    }

    //    public void FreeSendBuffer(int sendOffSet)
    //    {
    //        m_freeSendIndexStack.Push(sendOffSet);
    //    }

    //    public int GetRecvBuffer()
    //    {
    //        int index = -1;
    //        lock (this)
    //        {
    //            if (m_freeRecvIndexStack.Count > 0)  // 有释放的缓冲块
    //            {
    //                index = m_freeRecvIndexStack.Pop();
    //            }
    //            else
    //            {
    //                if (m_totalRecvLength >= m_currentRecvIndex + m_recvBufferSize)  // 有空间了
    //                {
    //                    index = m_currentRecvIndex;
    //                    m_currentRecvIndex += m_recvBufferSize;  // 调整可用块指针
    //                }
    //            }
    //        }
    //        return index;
    //    }

    //    public int GetSendBuffer()
    //    {
    //        int index = -1;

    //        lock (this)
    //        {
    //            if (m_freeSendIndexStack.Count > 0)  // 有释放的缓冲块
    //            {
    //                index = m_freeSendIndexStack.Pop();
    //            }
    //            else
    //            {
    //                if (m_totalSendLength >= m_currentSendIndex + m_sendBufferSize)  // 有空间了
    //                {
    //                    index = m_currentSendIndex;
    //                    m_currentSendIndex += m_sendBufferSize;  // 调整可用块指针
    //                }
    //            }
    //        }

    //        return index;
    //    }
    //}

    ///<summary>
    ///发送和接收公共缓冲区
    ///</summary>
    public sealed class BufferManager
    {
        private byte[] m_receiveBuffer;
        private byte[] m_sendBuffer;

        private int m_maxSessionCount;
        private int m_receiveBufferSize;
        private int m_sendBufferSize;

        private int m_bufferBlockIndex;
        private Stack<int> m_bufferBlockIndexStack;

        public BufferManager(int maxSessionCount, int receivevBufferSize, int sendBufferSize)
        {
            m_maxSessionCount = maxSessionCount;
            m_receiveBufferSize = receivevBufferSize;
            m_sendBufferSize = sendBufferSize;

            m_bufferBlockIndex = 0;
            m_bufferBlockIndexStack = new Stack<int>();

            m_receiveBuffer = new byte[m_receiveBufferSize * m_maxSessionCount];
            m_sendBuffer = new byte[m_sendBufferSize * m_maxSessionCount];
        }

        public int ReceiveBufferSize
        {
            get { return m_receiveBufferSize; }
        }

        public int SendBufferSize
        {
            get { return m_sendBufferSize; }
        }

        public byte[] ReceiveBuffer
        {
            get { return m_receiveBuffer; }
        }

        public byte[] SendBuffer
        {
            get { return m_sendBuffer; }
        }

        public void FreeBufferBlockIndex(int bufferBlockIndex)
        {
            if (bufferBlockIndex == -1)
            {
                return;
            }

            lock (this)
            {
                m_bufferBlockIndexStack.Push(bufferBlockIndex);
            }
        }

        public int GetBufferBlockIndex()
        {
            lock (this)
            {
                int blockIndex = -1;

                if (m_bufferBlockIndexStack.Count > 0)  // 有用过释放的缓冲块
                {
                    blockIndex = m_bufferBlockIndexStack.Pop();
                }
                else
                {
                    if (m_bufferBlockIndex < m_maxSessionCount)  // 有未用缓冲区块
                    {
                        blockIndex = m_bufferBlockIndex++;
                    }
                }

                return blockIndex;
            }
        }

        public int GetReceivevBufferOffset(int bufferBlockIndex)
        {
            if (bufferBlockIndex == -1)  // 没有使用共享块
            {
                return 0;
            }

            return bufferBlockIndex * m_receiveBufferSize;
        }

        public int GetSendBufferOffset(int bufferBlockIndex)
        {
            if (bufferBlockIndex == -1)  // 没有使用共享块
            {
                return 0;
            }

            return bufferBlockIndex * m_sendBufferSize;
        }

        public void Clear()
        {
            lock (this)
            {
                m_bufferBlockIndexStack.Clear();
                m_receiveBuffer = null;
                m_sendBuffer = null;
            }
        }
    }
}
