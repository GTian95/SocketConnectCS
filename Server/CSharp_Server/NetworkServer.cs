using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharp_Server
{
    class NetworkServer
    {
        private static NetworkServer instance;

        private Socket socket;
        private List<ConnectSocket> connectList;

        public string ip;
        public int port;

        public NetworkServer(string ip, int port)
        {

            string localIP = ip;
            //foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            //{
            //    if (address.AddressFamily.ToString() == "InterNetwork")
            //    {
            //        localIP = address.ToString();
            //    }
            //}

            this.ip = localIP;
            this.port = port;
            instance = this;
        }

        public static NetworkServer GetInstance()
        {
            if (instance == null)
            {
                Console.WriteLine("Error. need new NetworkServer first");
                return null;
            }
            return instance;
        }


        public Socket GetSocket()
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            return socket;
        }

        public IPEndPoint GetIPEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(ip), port);
        }


        public void Start()
        {
            connectList = new List<ConnectSocket>();

            StartListen();
        }

        public void StartListen()
        {
            Socket socket = GetSocket();
            // 将套接字与IPEndPoint绑定
            socket.Bind(this.GetIPEndPoint());
            // 开启监听
            socket.Listen(100);
            Console.WriteLine("Waiting for the connection...");

            Thread acceptThread = new Thread(TryAccept);
            acceptThread.Start();
        }

        public void TryAccept()
        {
            Socket socket = GetSocket();
            while (true)
            {
                try
                {
                    Socket connectedSocket = socket.Accept();

                    ConnectSocket clientSocket = new ConnectSocket(connectedSocket);
                    connectList.Add(clientSocket);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }

        public void BroadcastMessage(string message)
        {
            var unUseList = new List<ConnectSocket>();
            foreach (var item in connectList)
            {
                if (item.IsConnect)
                    item.Broadcast(message);
                else
                    unUseList.Add(item);
            }
            foreach (var item in unUseList)
            {
                connectList.Remove(item);
            }
        }
    }
}
