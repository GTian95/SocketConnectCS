using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharp_Server
{
    class Client
    {
        private Socket clientSocket;
        private Thread thread;
        private byte[] data = new byte[1024];
        int length;
        public Client(Socket socket)
        {
            clientSocket = socket;
            thread = new Thread(ReceiveMessage);
            thread.Start();
        }
        public void ReceiveMessage()
        {
            //监听接受客户信息
            while (true)
            {
                if (clientSocket.Poll(10, SelectMode.SelectRead))
                {
                    Console.WriteLine("用户链接超时！");    
                    break;
                }
                length = clientSocket.Receive(data);
                string message = Encoding.UTF8.GetString(data, 0, length);
                //接收到服务器数据的时候，分发到其他客户端
                Console.WriteLine("收到了消息：" + message);
                //广播消息 
                Program.BroadcastMessage(message);
            }
        }
        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            Console.WriteLine("广播消息:" + message);
            clientSocket.Send(data);
        }
        public bool IsConnect
        {
            get {
                bool res = true;
                if (clientSocket.Connected)
                {
                    if (clientSocket.Poll(1, SelectMode.SelectRead))
                    {
                        res = clientSocket.Receive(data) != 0;
                    }
                }
                else
                    res = false;
                return res;
            }
        }
    }
}
