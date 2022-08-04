using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedisEndpoint;

public class RedisListener : MonoBehaviour
{
    [HideInInspector]
    private const string URL = "localhost";
    [HideInInspector]
    private const ushort PORT = 6379;
    private static Subscriber mySubscriber;
    public int IntMsg;

    private static void StartSub()
    {
        mySubscriber = new Subscriber(URL, PORT);
        mySubscriber.SubscribeTo("audio");
    }
    void Start()
    {
        StartSub();
    }
    private void Update()
    {
        //mySubscriber.SubscribeTo("audio");
        IntMsg = mySubscriber.IntMsg;
    }
}
