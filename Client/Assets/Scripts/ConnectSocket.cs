using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using static System.Text.Encoding;
using System.Net;
using System.IO;


public delegate void OnReceiveEventHandler(ChatType type, string msg);
public delegate void DisconnectEventHandler(Exception e);
public delegate void ConnectEventHandler();

public enum ChatType : byte
{
    TEXT,
    FILE,
}

public class ConnectSocket
{
    public bool AutoReConnect { get; set; } = true;

    public event ConnectEventHandler OnConnected;
    public event DisconnectEventHandler OnDisconnected;
    public event OnReceiveEventHandler OnReceived;

    private Socket socket;

    public string ip;
    public int port;
    public string dirName = "ChatFiles";

    public ConnectSocket(string ip, int port)
    {
        this.ip = ip;
        this.port = port;
    }
    public IPEndPoint GetIPEndPoint()
    {
        return new IPEndPoint(IPAddress.Parse(ip), port);
    }
    public Socket GetSocket()
    {
        if (socket == null)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UnityEngine.Debug.Log("socket == null");
        }
        return socket;
    }

    public void Start()
    {
        Socket socket = GetSocket();
        try
        {
            UnityEngine.Debug.Log("try connect ... ..");
            socket.Connect(this.GetIPEndPoint());
            OnConnected();
            Console.WriteLine("connected ... ..." + socket.RemoteEndPoint.ToString());
            StartReceive();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("connect exception : " + e.Message);
            //Thread.Sleep(TryConnectInterval);
        }
    }

    public void StartReceive()
    {
        Thread receiveThread = new Thread(OnReceive);
        receiveThread.Start();
    }

    public bool SendMessage(string msg)
    {
        if (socket != null && socket.Connected)
        {
            byte[] buffer = UTF8.GetBytes(msg);
            byte[] len = BitConverter.GetBytes((long)buffer.Length);
            byte[] content = new byte[1 + len.Length + buffer.Length];
            content[0] = (byte)ChatType.TEXT;
            Array.Copy(len, 0, content, 1, len.Length);
            Array.Copy(buffer, 0, content, 1 + len.Length, buffer.Length);

            try
            {
                socket.Send(content);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "   ooo");
            }
        }
        return false;
    }

    public bool SendFile(string path)
    {
        UnityEngine.Debug.Log("send file path:" + path);

        if (socket != null && socket.Connected)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                byte[] len = BitConverter.GetBytes(fi.Length);
                byte[] name = UTF8.GetBytes(fi.Name);
                byte[] nameLen = BitConverter.GetBytes(name.Length);
                byte[] head = new byte[1 + len.Length + nameLen.Length + name.Length];
                head[0] = (byte)ChatType.FILE;
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

    public void OnReceive()
    {
        if (socket == null)
        {
            Console.WriteLine("OnReceive Error, socket is null");
            return;
        }

        while (true)
        {
            try
            {
                byte[] head = new byte[9];
                socket.Receive(head, head.Length, SocketFlags.None);

                int len = BitConverter.ToInt32(head, 1);
                if (head[0] == (byte)ChatType.TEXT)
                {
                    byte[] buffer = new byte[len];
                    socket.Receive(buffer, len, SocketFlags.None);
                    OnReceived(ChatType.TEXT, UTF8.GetString(buffer));
                }
                else if (head[0] == (byte)ChatType.FILE)
                {
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    byte[] nameLen = new byte[4];
                    socket.Receive(nameLen, nameLen.Length, SocketFlags.None);

                    byte[] name = new byte[BitConverter.ToInt32(nameLen, 0)];
                    socket.Receive(name, name.Length, SocketFlags.None);

                    string fileName = UTF8.GetString(name);

                    int readByte = 0;
                    int count = 0;
                    byte[] buffer = new byte[1024 * 8];

                    string filePath = Path.Combine(dirName, fileName);
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
                    OnReceived(ChatType.FILE, fileName);
                }
                else
                {
                    // 未知类型
                }
            }
            catch (Exception e)
            {
                OnDisconnected(e);
                // 连接异常断开
                if (AutoReConnect)
                {
                    this.socket.Close();
                    this.socket = null;
                    this.Start();
                }
                break;
            }

        }
    }
}