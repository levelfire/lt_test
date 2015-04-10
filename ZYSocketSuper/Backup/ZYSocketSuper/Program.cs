using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ZYSocketSuper
{
    class Program
    {
       static ZYSocketSuper socketserver;

        static void Main(string[] args)
        {
            socketserver = new ZYSocketSuper("192.168.200.100", 6600, 80000,4096);
            socketserver.MessageOut += new EventHandler<LogOutEventArgs>(socketserver_MessageOut);
            socketserver.BinaryInput = new BinaryInputHandler(BinaryInputHandler);
            socketserver.Connetions = new ConnectionFilter(ConnectionFilter);
            socketserver.MessageInput = new MessageInputHandler(MessageInputHandler);
            socketserver.Start();

            Console.ReadLine();
        }

        static void socketserver_MessageOut(object sender, LogOutEventArgs e)
        {
            Console.WriteLine(e.Mess);
        }

        /// <summary>
        /// 连接的代理
        /// </summary>
        /// <param name="socketAsync"></param>
        public static bool ConnectionFilter(SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine(socketAsync.AcceptSocket.RemoteEndPoint);
            return true;
        }

        /// <summary>
        /// 数据包输入代理
        /// </summary>
        /// <param name="data">输入包</param>
        /// <param name="socketAsync"></param>
        public static void BinaryInputHandler(byte[] data, SocketAsyncEventArgs socketAsync)
        {
            Console.WriteLine(string.Format("{0}:{1}",socketAsync.AcceptSocket.RemoteEndPoint,Encoding.Default.GetString(data)));

            if (Encoding.Default.GetString(data)[0] == 'd')
            {
                socketserver.Disconnect(socketAsync.AcceptSocket);
            }
            else
            {
                socketserver.SendData(socketAsync.AcceptSocket, new byte[]{0});
            }
            
        }

        /// <summary>
        /// 异常错误通常是用户断开的代理
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="socketAsync"></param>
        /// <param name="erorr">错误代码</param>
        public static void MessageInputHandler(string message, SocketAsyncEventArgs socketAsync, int erorr)
        {
            Console.Write(message);          
        }
    }
}
