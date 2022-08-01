using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTon<T>: MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = GameObject.FindObjectOfType<T>();
            if (instance == null)
            {
                GameObject go = new GameObject(typeof(T).ToString());
                instance = go.AddComponent<T>();
            }
        }
        return instance;
    }
}
