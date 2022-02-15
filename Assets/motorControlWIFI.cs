using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class motorControlWIFI : MonoBehaviour
{
    byte[] turnOn = new byte[3];
    byte[] turnOff = new byte[3];
    byte[] data = new byte[3];

    public float interval = 200.0f;
    public int motorNum = 4;

    private static string deviceIP = "192.168.50.20";
    private static int devicePort = 80;

    private Socket socket;

    private void Start()
    {
        turnOn[0] = System.Convert.ToByte((char)11);
        turnOn[1] = System.Convert.ToByte((char)(170 / 2));
        turnOn[2] = System.Convert.ToByte((char)30);
        turnOff[0] = System.Convert.ToByte((char)11);
        turnOff[1] = System.Convert.ToByte((char)(170 / 2));
        turnOff[2] = System.Convert.ToByte((char)0);
        data[0] = System.Convert.ToByte((char)11);
        data[1] = System.Convert.ToByte((char)(170 / 2));
        data[2] = System.Convert.ToByte((char)0);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("Establishing Connection to " + deviceIP);
        socket.Connect(deviceIP, devicePort);
        if (socket.Connected)
        {
            Debug.Log("Connection established!");
            socket.NoDelay = true;

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(socket.Connected)
            {
                socket.Close();
                Debug.Log("Closing socket!");
            }
            else
            {
                Debug.Log("Establishing Connection to " + deviceIP);
                socket.Connect(deviceIP, devicePort);
                if (socket.Connected)
                {
                    Debug.Log("Connection established!");
                    socket.NoDelay = true;

                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (socket.Connected)
            {
                socket.Send(turnOn);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (socket.Connected)
            {
                socket.Send(turnOff);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (socket.Connected)
            {
                StartCoroutine(LimitTest(interval / 1000, motorNum));
            }
        }
    }

    IEnumerator LimitTest(float interval = 0.1f, int motorNum = 1)
    {
        float duration = 5.0f, timer = 0;
        while(timer < duration)
        {
            for(int i = 0; i < motorNum; i++)
            {
                data[0] = System.Convert.ToByte((char)i);
                data[2] = System.Convert.ToByte((char)25);
                socket.Send(data);
            }
            timer += interval;
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < motorNum; i++)
            {
                data[0] = System.Convert.ToByte((char)i);
                data[2] = System.Convert.ToByte((char)0);
                socket.Send(data);
            }
            timer += interval;
            yield return new WaitForSeconds(interval);
        }
        yield break;
    }
}
