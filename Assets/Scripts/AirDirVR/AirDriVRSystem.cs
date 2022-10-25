using System;
using UnityEngine;
using System.IO.Ports;

namespace AirDriVR
{
    public class AirDriVRSystem : SingletonMonoBehaviour<AirDriVRSystem>
    {
        public Transform headTransform;
        
        private SerialPort serialPort;

        private int currentPriority = -1;
        private Vector2 currentForce;
        private byte[] buffer = new byte[4];  // Signal for Arduino board 

        private float updateInterval = 0.1f; // Set update frequency (0.1f = 10Hz)
        private float updateTimer = 0;

        public static bool HasInitialized => Instance.serialPort != null && Instance.serialPort.IsOpen;

        [Flags]
        public enum OverrideMode : short
        {
            None = 0, XOnly = 1, YOnly = 2, Both = 3
        }

        public enum ADTMode // for ADT study
        {
            None = '0', Seated = '3', Standing = '4'
        }

        private ADTMode adtMode = ADTMode.None;
        
        public static bool Init(string port)
        {
            if (HasInitialized)
            {
                Debug.LogError("AirDriVRSystem Init failed: AirDriVRSystem has already connected to a port.");
                return false;
            }
            
            try
            {
                Instance.serialPort = new SerialPort(port, 9600);
                Instance.serialPort.Open();
            }
            catch (Exception e)
            {
                Debug.LogError($"AirDriVRSystem Init failed: AirDriVRSystem failed to open the assigned port ({port}). Exception: {e}");
                return false;
            }
            
            Debug.Log($"AirDriVRSystem Init successfully. Now running on {port}.");
            return true;
        }

        public static void SetHeadTransform(Transform t)
        {
            Instance.headTransform = t;
        }

        public static void SetADTMode(ADTMode mode)
        {
            if (!HasInitialized) return;
            Instance.adtMode = mode;
        }

        public static void SetForce(Vector2 force, int priority, OverrideMode overrideMode = OverrideMode.Both)
        {
            if (!HasInitialized)
            {
                Debug.LogError($"AirDriVRSystem SetForce failed: AirDriVRSystem has not yet been initialized.");
                return;
            }

            if (priority < 0)
            {
                Debug.LogError($"AirDriVRSystem SetForce failed: Priority must be greater than 0.");
                return;
            }

            if (priority < Instance.currentPriority) return;

            Instance.currentPriority = priority;
            if ((overrideMode & OverrideMode.XOnly) != 0) Instance.currentForce.x = force.x;
            if ((overrideMode & OverrideMode.YOnly) != 0) Instance.currentForce.y = force.y;
            Instance.buffer[0] = (byte)(int)Instance.adtMode; // Header
        }

        private void LateUpdate()
        {
            //Send data to Arduino here.
            if (!HasInitialized) return;

            if (currentPriority < 0) return; // encounter special event

            updateTimer += Time.deltaTime;

            if (updateTimer < updateInterval) return;

            var cnt = (buffer[0] == '1') ? 1 : 4;
            if (cnt > 1)
            {
                // map virtual force -> reality force
                var force = currentForce;
                if (headTransform)  // change direction to supply force.
                {
                    var f = headTransform.forward;
                    f.y = 0;
                    var tmpForce = new Vector3(force.x, 0, force.y);
                    tmpForce = Quaternion.Inverse(Quaternion.LookRotation(f)) * tmpForce;
                    force.x = tmpForce.x;
                    force.y = tmpForce.z;
                }

                var absY = Mathf.Abs(force.y);
                var absX = Mathf.Abs(force.x);
                // set force
                Instance.buffer[1] = (byte) (Mathf.Clamp01(absY) * 255);  // Longitudinal
                Instance.buffer[2] = (byte) (Mathf.Clamp01(absX) * 255);  // Horizontal
                  
                var gateByte = (byte) 0;
                // decide which valves should be opened.
                if (absY > 0.01f)
                {
                    gateByte |= (byte)(force.y < 0 ? 0b00001000 : 0b00000100);
                }
                if (absX > 0.01f)
                {
                    gateByte |= (byte)(force.x < 0 ? 0b00000010 : 0b00000001);
                }

                Instance.buffer[3] = gateByte;
            }
            serialPort.Write(buffer, 0, cnt);

            //PrintDebug(cnt);
            
            currentPriority = -1;
            currentForce = Vector2.zero;
            updateTimer -= updateInterval;
        }

        private void PrintDebug(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Debug.Log(Convert.ToString(buffer[i], 2).PadLeft(8, '0'));
            }
        }

        public static void SendReset()
        {
            if (!HasInitialized)
            {
                Debug.LogError($"AirDriVRSystem SetForce failed: AirDriVRSystem has not yet been initialized.");
                return;
            }

            Instance.currentPriority = 0;
            Instance.buffer[0] = (byte) '1'; // Header
        }

        private void OnDisable()
        {
            if (!HasInitialized) return;
            
            SendReset();
            updateTimer += updateInterval;
            LateUpdate();
            serialPort.Close();
        }
    }
}