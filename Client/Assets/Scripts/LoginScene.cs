using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    public Text text_err;
    public Button btn_contect;
    public InputField input_ip;

    // Start is called before the first frame update
    void Start()
    {
        btn_contect.onClick.AddListener(EnterChat);
    }

    void EnterChat()
    {
        if (string.IsNullOrEmpty(input_ip.text))
        {
            text_err.text = "IP 不能为空";
            return;
        }

        NetworkManager.IpAddress = input_ip.text;
        SceneManager.LoadScene("Chat");
    }
}
