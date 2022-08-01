using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetworkManager : SingleTon<NetworkManager>
{
    private List<IObserver> m_observers;

    public static string IpAddress = @"127.0.0.1";
    private const int Port = 7788;
    private Socket tcpSocket;
    private Thread t;
    private byte[] data = new byte[1024];

    public void ConnectServer()
    {
        tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(IpAddress), Port));
        t = new Thread(ReceiveMessage);
        t.Start();
    }

    public void SendMSG(string message)
    {
        if (tcpSocket == null)
        {
            Debug.LogError("网络未连接!");
            return;
        }
        byte[] data = Encoding.UTF8.GetBytes(message);
        tcpSocket.Send(data);
    }

    public void SendIMG(string image_path)
    {
        if (tcpSocket == null)
        {
            Debug.LogError("网络未连接!");
            return;
        }
        FileStream fs = new FileStream(image_path, FileMode.Open);
        long contentLength = fs.Length;
        //第一次发送数据包的大小           
        tcpSocket.Send(BitConverter.GetBytes(contentLength));
        while (true)
        {
            //每次发送128字节               
            byte[] bits = new byte[128];
            int r = fs.Read(bits, 0, bits.Length);
            if (r <= 0) break;
            tcpSocket.Send(bits, r, SocketFlags.None);
        }
        fs.Close();
    }


    public void AddObserverHandle(IObserver _observer)
    {
        if (!m_observers.Contains(_observer))
            m_observers.Add(_observer);
    }

    public void RemoveObserverHandle(IObserver _observer)
    {
        if (m_observers.Contains(_observer))
            m_observers.Remove(_observer);
    }

    private void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                if (!tcpSocket.Connected)
                {
                    Debug.LogError("网络已断开!");
                    break;
                }

                int length = tcpSocket.Receive(data);
                foreach (var observer in m_observers)
                {
                    observer.Notify(data, length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error! " + e.ToString());
                if (tcpSocket != null)
                {
                    tcpSocket.Shutdown(SocketShutdown.Both);
                    tcpSocket.Close();
                }
            }
        }
    }

    private void OnDestroy()
    {
        tcpSocket.Shutdown(SocketShutdown.Both);
        tcpSocket.Close();
    }

}
