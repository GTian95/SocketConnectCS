using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace CSharp_Server
{
    class Program
    {
        static NetworkServer m_network;

        public static List<Client> clientList = new List<Client>();
        public static void BroadcastMessage(string message)
        {
            var unUseList = new List<Client>();
            foreach (Client item in clientList)
            {
                if (item.IsConnect)
                    item.SendMessage(message);
                else
                    unUseList.Add(item);
            }
            foreach (var item in unUseList)
            {
                clientList.Remove(item);
            }
        }
        static void Main(string[] args)
        {
            m_network = new NetworkServer("192.168.2.156", 7789);
            m_network.Start();
            
            /*
            //1.创建Socket
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string ip;
            Console.WriteLine("请输入Ip地址：");
            ip = Console.ReadLine();
            //2.绑定IP以及端口Port
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 7788);
            tcpServer.Bind(ipEndPoint);
            Console.WriteLine("Server Start Succeeded! Waiting Client Asspect ...");
            //建立最大连接数
            tcpServer.Listen(100);
            while (true)
            {
                Socket socket = tcpServer.Accept();
                Console.WriteLine("用户链接成功！");
                Client client = new Client(socket);
                clientList.Add(client);
            }
            */
        }
    }
}
