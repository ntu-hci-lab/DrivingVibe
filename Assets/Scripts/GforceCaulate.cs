using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GforceCaulate : MonoBehaviour
{
    public enum RotationMode
    {
        Orientation,
        Velocity
    }
 
    //public RotationMode rotationMode;

    public enum MonitorMode
        {
            Transform,
            Rigidbody
        }

    //public bool transformModeGravity = false;
    //public bool regularUpdateMode = false;

    //public Transform headTransform;
    public Transform cameraTransform;

        [Range(2, 20)] public int accelBufferSampleCount = 5;
        [Range(2, 20)] public int veloBufferSampleCount = 5;


        private Vector3 lastVelocity = Vector3.zero;
        private Vector3 lastAccel = Vector3.zero;
        private int accelSamples = 0;
        private Vector3[] accelBuffer;

        public Vector3 velo;
        public Vector3 accel;

        public Vector3 Acceleration => accel;
        public float cityScale = 5;

        //public Vector3 G { get; private set; }
        public Vector3 AccelerationToArduino;
        public int timer = 0;

        private void Awake()
        {
            accelBuffer = new Vector3[accelBufferSampleCount];
        }

        private void Update()
        {
            //Calculate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (timer % 3 == 0)
            {
                Calculate(Time.fixedDeltaTime*3); // get one sample every 3 frames
                timer = 0;
            }
            timer++;

            //Calculate(Time.fixedDeltaTime);
        }

        private void Calculate(float deltaTime)
        {
            velo = -GetComponent<CinemachineDollyCart>().m_Speed * transform.forward;

            //var velocity = (monitorMode == MonitorMode.Rigidbody) ? rigidbody.velocity : velo;
            var frameAccel = (velo - lastVelocity) / deltaTime;

            accelBuffer[accelSamples % accelBufferSampleCount] = frameAccel;
            accelSamples++;

            accel = Vector3.zero;
            // SMOOTHING
            var cnt = Mathf.Min(accelSamples, accelBufferSampleCount);
            for (var i = 0; i < cnt; i++)
            {
                accel += accelBuffer[i]; // Taking average of the acceleration of the past _ frames 
            }
        
            accel /= cnt;
            accel = Quaternion.Inverse(cameraTransform.rotation) * accel;
            AccelerationToArduino =  accel;
            //Debug.Log(accel.y);
            //if (transformModeGravity && monitorMode == MonitorMode.Transform) accel += Physics.gravity;
          

            lastVelocity = velo;
            lastAccel = accel;


    }
}
