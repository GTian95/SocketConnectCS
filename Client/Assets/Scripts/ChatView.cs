using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatView : MonoBehaviour
{
    private ConnectSocket m_socket;

    public Text text_msg;
    public Image img_pic;

    public InputField input_msg;
    public Button btn_send;
    public Button btn_sendfila;

    // Start is called before the first frame update
    void Start()
    {
        m_socket = new ConnectSocket("192.168.2.156", 7789);

        AddHandleListener();

        Debug.Log("start connect");
        m_socket.Start();
        Debug.Log("start connect end");
    }

    private void AddHandleListener()
    {
        m_socket.OnConnected += OnConnect;
        m_socket.OnReceived += OnReceive;
        m_socket.OnDisconnected += OnDisconnect;

        btn_send.onClick.AddListener(() => {
            m_socket.SendMessage(input_msg.text);
        });
        btn_sendfila.onClick.AddListener(() =>
        {
            m_socket.SendFile(Application.dataPath + "/love3.png");
        });
    }

    private void RemoveHandleListener()
    {
        if (m_socket != null)
        {
            m_socket.OnConnected -= OnConnect;
            m_socket.OnReceived -= OnReceive;
            m_socket.OnDisconnected -= OnDisconnect;
        }
    }

    private void OnConnect()
    {
        Debug.Log("connect server succes");
    }


    private void OnReceive(ChatType type, string msg)
    {
        if (type == ChatType.TEXT)
        {
            Debug.Log("get text:" + msg);
            //text_msg.text = msg;
        }
        else if (type == ChatType.FILE)
        {
            Debug.Log("get img path:" + msg);
        }
    }

    private void OnDisconnect(Exception e)
    {
        Debug.LogError("connect server faild, error:" + e.ToString());
    }

    void OnDestroy()
    {
        RemoveHandleListener();
    }
}
