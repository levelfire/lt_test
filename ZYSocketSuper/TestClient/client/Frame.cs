using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.client
{
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
}
