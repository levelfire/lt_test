
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.client
{
    public sealed class BufferManager
    {
        // ...成员字段见其构造函数
        public int m_maxSessionCount;
        public int m_recvBufferSize;
        public int m_sendBufferSize;

        public int m_availableRecvOffset;
        public int m_availbleSendOffset;

        public Stack<int> m_availbleOffsetStack;

        public int m_totalRecvLength;
        public int m_totalSendLength;

        public byte[] m_recvBuffer;
        public byte[] m_sendBuffer;

        public BufferManager(int maxSessionCount = 100, int recvBufferSize = 4096, int sendBufferSize = 4096)
        {
            m_maxSessionCount = maxSessionCount; // 服务器允许的最大连接（会话）数

            m_recvBufferSize = recvBufferSize; // 接收缓冲区大小
            m_sendBufferSize = sendBufferSize;

            m_availableRecvOffset = 0; // 当前可以的接收缓冲区偏移地址
            m_availbleSendOffset = 0;

            m_availbleOffsetStack = new Stack<int>(); // 可重复使用的缓冲区偏移地址

            m_totalRecvLength = m_recvBufferSize * m_maxSessionCount;
            m_totalSendLength = m_sendBufferSize * m_maxSessionCount;

            m_recvBuffer = new byte[m_totalRecvLength]; // 接收缓冲区
            m_sendBuffer = new byte[m_totalSendLength];
        }

        public int RecvBufferSize
        {
            get { return m_recvBufferSize; }
        }

        public int SendBufferSize
        {
            get { return m_sendBufferSize; }
        }

        public byte[] RecvBuffer
        {
            get { return m_recvBuffer; }
        }

        public byte[] SendBuffer
        {
            get { return m_sendBuffer; }
        }

        public void FreeOffset(int recvOffset, int sendOffset)
        {
            lock (this)
            {
                m_availbleOffsetStack.Push(recvOffset); // 回收缓冲区偏移地址
                m_availbleOffsetStack.Push(sendOffset);
            }
        }

        public void GetOffset(ref int recvOffset, ref int sendOffset)
        {
            lock (this)
            {
                if (m_availbleOffsetStack.Count > 0) // 有释放的可重用缓冲区
                {
                    sendOffset = m_availbleOffsetStack.Pop(); // 再次使用
                    recvOffset = m_availbleOffsetStack.Pop();
                }
                else
                {
                    if (m_totalRecvLength >= m_availableRecvOffset + m_recvBufferSize &&
                    m_totalSendLength >= m_availbleSendOffset + m_recvBufferSize) // 有空间
                    {
                        recvOffset = m_availableRecvOffset;
                        sendOffset = m_availbleSendOffset;

                        m_availableRecvOffset += m_recvBufferSize; // 调整可用块指针
                        m_availbleSendOffset += m_sendBufferSize;
                    }
                    else
                    {
                        throw new IndexOutOfRangeException("buffer index out of range.");
                    }
                }
            }
        }
    }
}
