using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AirDriVR
{
    public class ACListener : GameListener
    {
        public float accReduceFactor = 0.25f;
        public string hostIP = "127.0.0.1";
        public int hostPort = 9996;

        public Text infoText;

        // [FormerlySerializedAs("airDriVrController")] public AirDriVRTestController airDriVrTestController;

        private IPEndPoint acEndPoint;
        private UdpClient client;

        private ACCarInfo info;
        private Vector2 forceVector;
        private float forceMag;
    
        private readonly object infoLock = new object();

        public bool hasConnected = false;

        public bool logGForceToFile = false;
        public string logFolder;

        private StreamWriter logStreamWriter;

        private bool isConnecting = false;

        
        [HideInInspector]
        public float[] pos = new float[4];
        [HideInInspector]
        public float velocity;
        [HideInInspector]
        public Vector2 Gforce;
        [HideInInspector]
        public float G_vertical;
        //[HideInInspector]
        public float gas;
        //[HideInInspector]
        //public float brake;
        //private float lastRPM;
        //[HideInInspector]
        //public float RPM;
        //[HideInInspector]
        //public bool IsRpmRising;
        //[HideInInspector]
        //public int gear;
        private float myTimeStamp;
        [HideInInspector]
        public int lastRecordTime = 0;
        //[HideInInspector]
        //public float[] load = new float[4];
        private float[][] suspensionBuffer;
        private int[] lapTimeBuffer;
        public int bufferSize = 8;
        private int bufferIndex = 0;
        private float[] lastSuspension = new float[4];
        private float[] unBufferedLastSuspension = new float[4];
        //[HideInInspector]
        public float[] newSuspension = new float[4];
        public float[] unBufferedNewSuspension = new float[4];
        //[HideInInspector]
        public float[] suspensionDiff = new float[4];
        //[HideInInspector]
        public float[] unBufferedSuspensionDiff = new float[4];
        //[HideInInspector]
        //public float carSlope;
        //[HideInInspector]
        //public float[] Dy = new float[4];
        //[HideInInspector]
        //public float[] lastDirty = new float[4];
        //[HideInInspector]
        //public float[] newDirty = new float[4];

        //public bool[] isOutOfTrack = new bool[4];

        private void Start()
        {
            suspensionBuffer = new float[4][];
            lapTimeBuffer = new int[bufferSize];
            for (int i = 0; i < 4; i++)
            {
                lastSuspension[i] = 0;
                newSuspension[i] = 0;
                unBufferedLastSuspension[i] = 0;
                unBufferedNewSuspension[i] = 0;
                suspensionDiff[i] = 0;
                unBufferedSuspensionDiff[i] = 0;
                suspensionBuffer[i] = new float[bufferSize];
                for(int j = 0; j < bufferSize; j++)
                {
                    suspensionBuffer[i][j] = 0;
                    lapTimeBuffer[j] = 0;
                }
            }
            Connect();
        }
        public void Connect()
        {
        
            if (hasConnected)
            {
                Debug.LogError("Already connected. Now attempt to disconnect and reconnect.");
                OnDisable();
            }
        
            if (isConnecting)
            {
                Debug.LogError("Can't start handshake; an attempt to handshake is undergoing.");
                return;
            }
        
            acEndPoint = new IPEndPoint(IPAddress.Parse(hostIP), hostPort);
            client = new UdpClient();

            StartCoroutine(HandshakeStart());
        }

        IEnumerator HandshakeStart()
        {
            Debug.Log("Initiating handshake");
            isConnecting = true;
            var hs = new ACHandshaker(0, 1, 0);
            var hsPacketSize = Marshal.SizeOf<ACHandshaker>();
            var packet = new byte[hsPacketSize];

            var ptr = Marshal.AllocHGlobal(hsPacketSize);
            Marshal.StructureToPtr(hs, ptr, true);
            Marshal.Copy(ptr, packet, 0, hsPacketSize);

            try
            {
                client.Send(packet, packet.Length, acEndPoint);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                isConnecting = false;
                yield break;
            }

            var listeningTask = client.ReceiveAsync();

            while (!listeningTask.IsCompleted)
            {
                yield return null;
            }

            var hsResponse = FromBytes<ACHandshackerResponse>(listeningTask.Result.Buffer);
            Debug.Log(hsResponse.ToString());

            // Subscribe
            hs.operationId = 1;
            Marshal.StructureToPtr(hs, ptr, true);
            Marshal.Copy(ptr, packet, 0, hsPacketSize);

            client.Send(packet, packet.Length, acEndPoint);
        
            Marshal.FreeHGlobal(ptr);

            isConnecting = false;
            hasConnected = true;

            if (logGForceToFile)
            {
                var logFilePath = Path.Combine(logFolder, $"{DateTime.Now:yyyyMMdd-hh-mm-ss-ff}.csv");
                logStreamWriter = File.AppendText(logFilePath);
                logStreamWriter.WriteLine("lap_time,horizontal,longitudinal");
            }

            Task.Run(ListenToUpdate);
            while (hasConnected)
            {
                /*
                lock (infoLock)
                {
                    airDriVrTestController.SetGForce(
                        info.accG_horizontal * horizontalMultipler, 
                        -info.accG_frontal * longitudinalMultiplier);
                }
                */
                ProcessParameters();
                yield return new WaitForFixedUpdate();
                // Debug.Log("In while loop! ");
            }
        }

        private void ListenToUpdate()
        {
            while (hasConnected)
            {
                var tmp = FromBytes<ACCarInfo>(client.Receive(ref acEndPoint));
                info = tmp;
                logStreamWriter?.WriteLine($"{tmp.lapTime},{tmp.accG_horizontal},{-tmp.accG_frontal}");
            }
        }

        private void OnDisable()
        {
            if (hasConnected)
            {
                hasConnected = false;
                isConnecting = false;
                // 3: Dismiss connection
                var hs = new ACHandshaker(0, 1, 3);
                var hsPacketSize = Marshal.SizeOf<ACHandshaker>();
                var packet = new byte[hsPacketSize];

                var ptr = Marshal.AllocHGlobal(hsPacketSize);
                Marshal.StructureToPtr(hs, ptr, true);
                Marshal.Copy(ptr, packet, 0, hsPacketSize);

                client.Send(packet, packet.Length, acEndPoint);
        
                Marshal.FreeHGlobal(ptr);
        
                Debug.Log("Successfully unsubscribed");
                
                logStreamWriter?.Dispose();
            }
        }
        private void ProcessParameters()
        {
            infoText.text = info.ToString();
            pos = info.carCoordinates; // new Vector3(info.carCoordinates[0], info.carCoordinates[1], info.carCoordinates[2]); 
            velocity = info.speed_Ms;
            Gforce = new Vector2(info.accG_horizontal, info.accG_frontal);
            /*
            if (info.accG_frontal > 0)
            {
                Gforce = new Vector2(info.accG_horizontal, info.accG_frontal * accReduceFactor);
            }
            else
            {
                Gforce = new Vector2(info.accG_horizontal, info.accG_frontal);
            }
            */
            G_vertical = info.accG_vertical;

            gas = info.gas;

            if (lastRecordTime < info.lapTime)
            {

                for (int i = 0; i < 4; i++)
                {
                    unBufferedNewSuspension = info.suspensionHeight;
                    suspensionBuffer[i][bufferIndex] = unBufferedNewSuspension[i];
                    
                    //newSuspension[i] = BufferAverage(suspensionBuffer[i], bufferSize);
                }
                lapTimeBuffer[bufferIndex] = info.lapTime;
                
                int nextBufferIndex = (bufferIndex + 1) % bufferSize;
                for (int i = 0; i < 4; i++)
                {
                    unBufferedSuspensionDiff[i] = Mathf.Abs(unBufferedNewSuspension[i] - unBufferedLastSuspension[i]) / (info.lapTime - lastRecordTime) * 1000;
                    //suspensionDiff[i] = Mathf.Abs(newSuspension[i] - lastSuspension[i]) / (info.lapTime - lastRecordTime) * 1000;
                    suspensionDiff[i] = (suspensionBuffer[i][bufferIndex] - suspensionBuffer[i][nextBufferIndex]) / (lapTimeBuffer[bufferIndex] - lapTimeBuffer[nextBufferIndex]) * 1000;
                    unBufferedLastSuspension[i] = unBufferedNewSuspension[i];
                    //lastSuspension[i] = newSuspension[i];
                }
                bufferIndex = nextBufferIndex;
                lastRecordTime = info.lapTime;
            }
            else
            {
                lastRecordTime = info.lapTime;
            }

        }

        private float BufferAverage(float[] array, int size)
        {
            float tmp = 0;
            for(int i = 0; i < size; i++)
            {
                tmp += array[i];
            }
            return (tmp / size);
        }
    }
}
