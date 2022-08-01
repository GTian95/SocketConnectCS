using System;
using System.Net.Sockets;
using System.Threading;
using static System.Text.Encoding;
using System.IO;
using System.Text;

namespace CSharp_Server
{
    public enum MESSAGETYPE : byte
    {
        TEXT,
        PICTURE,
        VIDEO,
        FILE,
    }

    class ConnectSocket
    {
        private Socket socket;
        private Thread thread;

        public string connectedIP;
        public bool AutoReConnect { get; set; } = true;
        private string floderName = "SocketFiles";

        private MESSAGETYPE BroadcastType = MESSAGETYPE.TEXT;
        public ConnectSocket(Socket _socket)
        {
            socket = _socket;
            connectedIP = socket.RemoteEndPoint.ToString().Split(':')[0];
            Connected();

            thread = new Thread(OnReceive);
            thread.Start();            
        }

        public bool IsConnect
        {
            get
            {
                bool res = true;
                if (socket != null && socket.Connected)
                {
                    if (socket.Poll(1, SelectMode.SelectRead))
                    {
                        byte[] data = new byte[1024];
                        socket.Receive(data, data.Length, SocketFlags.None);
                        res = socket.Receive(data) != 0;
                    }
                }
                else
                    res = false;
                return res;
            }
        }

        public string GetConnectedIP()
        { 
            return connectedIP;
        }

        public void Connected()
        {
            Console.WriteLine("ip:{0} connect success", this.GetConnectedIP());
        }

        public void Disconnected(Exception e)
        {
            Console.WriteLine("ip:{0} Disconnected, more message:{1}", this.GetConnectedIP(), e.Message);
        }

        public void OnReceive()
        {
            if (socket == null)
            {
                Console.WriteLine("OnReceive Error, socket is null");
                Console.ReadKey();
                return;
            }
               
            while (true)
            {
                try
                {
                    if (socket.Poll(20, SelectMode.SelectRead))
                    {
                        Console.WriteLine("用户链接超时！");
                        break;
                    }

                    byte[] head = new byte[9];
                    socket.Receive(head, head.Length, SocketFlags.None);

                    int len = BitConverter.ToInt32(head, 1);
                    if (head[0] == (byte)MESSAGETYPE.TEXT)
                    {
                        byte[] buffer = new byte[len];
                        socket.Receive(buffer, len, SocketFlags.None);
                        ReceiveMessage(MESSAGETYPE.TEXT, UTF8.GetString(buffer));
                        
                    }
                    else if (head[0] == (byte)MESSAGETYPE.FILE)
                    {
                        if (!Directory.Exists(floderName))
                        {
                            Directory.CreateDirectory(floderName);
                        }

                        byte[] nameLen = new byte[4];
                        socket.Receive(nameLen, nameLen.Length, SocketFlags.None);

                        byte[] name = new byte[BitConverter.ToInt32(nameLen, 0)];
                        socket.Receive(name, name.Length, SocketFlags.None);

                        string fileName = UTF8.GetString(name);

                        int readByte = 0;
                        int count = 0;
                        byte[] buffer = new byte[1024 * 8];

                        Console.WriteLine("floderName:" + floderName);
                        Console.WriteLine("fileName:" + fileName);

                        string filePath = Path.Combine(floderName, fileName);
                        Console.WriteLine("filePath:" + filePath);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                        {
                            while (count != len)
                            {
                                int readLength = buffer.Length;
                                if (len - count < readLength)
                                {
                                    readLength = len - count;
                                }
                                readByte = socket.Receive(buffer, readLength, SocketFlags.None);
                                fs.Write(buffer, 0, readByte);
                                count += readByte;
                            }
                        }
                        ReceiveMessage(MESSAGETYPE.FILE, fileName);
                    }
                    else
                    {
                        // 未知类型
                    }
                }
                catch (Exception e)
                {
                    Disconnected(e);
                    // 连接异常断开
                    if (AutoReConnect)
                    {
                        this.socket.Close();
                        this.socket = null;

                    }
                    break;
                }
            }
        }

        public void ReceiveMessage(MESSAGETYPE type, string message)
        {
            Console.WriteLine(string.Format("{0}-Send:type={1},message={2}",connectedIP, type, message));
            BroadcastType = type;
            NetworkServer.GetInstance().BroadcastMessage(message);
        }

        public void Broadcast(string message)
        {
            if (BroadcastType == MESSAGETYPE.TEXT)
            {
                SendMessage(message);
            }
            else if (BroadcastType == MESSAGETYPE.FILE)
            { 
                SendFile(message);
            }
        }

        public bool SendMessage(string msg)
        {
            if (socket != null && socket.Connected)
            {
                Console.WriteLine("Broadcast message :" + msg);

                byte[] buffer = UTF8.GetBytes(msg);
                byte[] len = BitConverter.GetBytes((long)buffer.Length);
                byte[] content = new byte[1 + len.Length + buffer.Length];
                content[0] = (byte)MESSAGETYPE.TEXT;
                Array.Copy(len, 0, content, 1, len.Length);
                Array.Copy(buffer, 0, content, 1 + len.Length, buffer.Length);
                try
                {
                    socket.Send(content);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Send Error. message:" + e.Message);
                }
            }
            return false;
        }

        public bool SendFile(string path)
        {
            if (socket != null && socket.Connected)
            {
                Console.WriteLine("Broadcast file path:" + path);
                try
                {
                    FileInfo fi = new FileInfo(path);
                    byte[] len = BitConverter.GetBytes(fi.Length);
                    byte[] name = Encoding.UTF8.GetBytes(fi.Name);
                    byte[] nameLen = BitConverter.GetBytes(name.Length);
                    byte[] head = new byte[1 + len.Length + nameLen.Length + name.Length];
                    head[0] = (byte)MESSAGETYPE.FILE;
                    Array.Copy(len, 0, head, 1, len.Length);
                    Array.Copy(nameLen, 0, head, 1 + len.Length, nameLen.Length);
                    Array.Copy(name, 0, head, 1 + len.Length + nameLen.Length, name.Length);
                    socket.SendFile(
                        path,
                        head,
                        null,
                        TransmitFileOptions.UseDefaultWorkerThread
                    );
                    return true;
                }
                catch (Exception e)
                {
                    // 连接断开了
                    Console.WriteLine("send file exception : " + e.Message);
                }

            }
            return false;
        }
    }
}
