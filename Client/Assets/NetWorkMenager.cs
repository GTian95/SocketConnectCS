using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;

public class NetWorkMenager : SingleTon<NetWorkMenager>
{ 
    public Text showInfoText;

    public InputField inputField;

    private string ipAdress = "192.168.2.156"; //"127.0.0.1";
    private int port = 7788;

    private Socket clientSocket;
    private byte[] readBuffer = new byte[1024];

    private string recieveData;

    void Update()
    {
        showInfoText.text = recieveData;
    }
    public void ConnectServer()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        clientSocket.Connect(ipAdress, port);

        clientSocket.BeginReceive(readBuffer, 0, 1024, SocketFlags.None, RecieveCbFunc, null);
    }
    public void RecieveCbFunc(IAsyncResult result)
    {
        try
        {
            int count = clientSocket.EndReceive(result);
            recieveData = Encoding.UTF8.GetString(readBuffer, 0, count); 
        }
        catch (Exception)
        {
            clientSocket.Close();
        }
    }
    public void SendMsg()
    {
        byte[] data = Encoding.UTF8.GetBytes(inputField.text);
        clientSocket.Send(data);
    }

    public void SendImg()
    {
        FileStream fs = new FileStream(Application.dataPath + "\\love3.png", FileMode.Open);
        long contentLength = fs.Length;
        //第一次发送数据包的大小           
        clientSocket.Send(BitConverter.GetBytes(contentLength));
        while (true)
        {
            //每次发送128字节               
            byte[] bits = new byte[128];
            int r = fs.Read(bits, 0, bits.Length);
            if (r <= 0) break;
            clientSocket.Send(bits, r, SocketFlags.None);
        }
        fs.Position = 0;
    }
}

