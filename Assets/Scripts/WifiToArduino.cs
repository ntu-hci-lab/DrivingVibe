using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class WifiToArduino : MonoBehaviour
{
    private int motorCount = 16;
    private byte[] setZero;
    public static string deviceIP = "192.168.50.20";
    public static int devicePort = 80;
    private Socket socket;
    public bool arduinoPaused = true;
    public bool showString = false;

    private VirtualLayer virtualLayer;

    // Start is called before the first frame update
    void Start()
    {
        virtualLayer = gameObject.GetComponent<VirtualLayer>();

        arduinoPaused = true;
        setZero = new byte[motorCount];
        for(int i = 0; i< motorCount; i++)
        {
            setZero[i] = System.Convert.ToByte((char)0);
        }
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("Establishing Connection to " + deviceIP);
        socket.Connect(deviceIP, devicePort);
        if (socket.Connected)
        {
            Debug.Log("Connection established!");
            socket.NoDelay = true;
            arduinoPaused = false;
            virtualLayer.StartMonitor();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!socket.Connected)
            {
                ReOpen();
            }
        }
    }
    public void startArduino()
    {
        arduinoPaused = false;
    }

    public void PauseArduino()
    {
        arduinoPaused = true;
        setAllToZero();
    }

    public void writeToArduinoByte(byte[] input)
    {
        if (showString && !arduinoPaused)
        {
            Debug.Log("Send to arduino: " + input[0].ToString() + " " + input[1].ToString() + " ... " + input[motorCount-1].ToString());
        }
        if (socket.Connected && !arduinoPaused)
        {
            socket.Send(input);
        }
    }

    public void ReOpen()
    {
        if (socket.Connected)
        {
            Debug.Log("Close connection.");
            socket.Close();
            arduinoPaused = true;
        }
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("Establishing Connection to " + deviceIP);
        socket.Connect(deviceIP, devicePort);
        if (socket.Connected)
        {
            Debug.Log("Connection established!");
            socket.NoDelay = true;
            arduinoPaused = false;
        }
    }

    public void setAllToZero()
    {
        Debug.Log("Set all to zero");
        for (int i = 0; i < motorCount; i++)
        {
            if (socket.Connected)
            {
                socket.Send(setZero);
            }
        }
    }


    private void OnApplicationQuit()
    {
        if (socket.Connected)
        {
            setAllToZero();
            Debug.Log("Close connection.");
            socket.Close();
        }
    }
}
