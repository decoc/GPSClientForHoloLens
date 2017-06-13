using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    public UDP udp;

    [SerializeField]
    private string message;

    void Start()
    {
        if (udp == null) { udp = new GameObject("UDP Client").AddComponent<UDP>(); }
        udp.OnReceivedEventHandler += ReceivedEventHandler;
        udp.Init();
        udp.Connect();
    }

    void ReceivedEventHandler(object sender, ReceivedEventArgs args)
    {
        message = args.msg;
        Debug.Log(message);
    }
}
