using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AirDriVR
{
    public class GForceMonitor : MonoBehaviour
    {
        [HideInInspector] public new Rigidbody rigidbody;

        public enum RotationMode
        {
            Orientation,
            Velocity
        }

        public RotationMode rotationMode;

        public enum MonitorMode
        {
            Transform,
            Rigidbody
        }

        public MonitorMode monitorMode;

        public AirDriVRSystem.OverrideMode overrideMode = AirDriVRSystem.OverrideMode.Both;

        public bool transformModeGravity = false;
        public bool regularUpdateMode = false;

        public Transform headTransform;

        [Range(2, 20)] public int accelBufferSampleCount = 6;
        [Range(2, 20)] public int veloBufferSampleCount = 4;
        [Range(1, 20)] public int gBufferSampleCount = 6;

        public bool updateAirDriVR = false;
        public float airDriVRMaxG = 3f;
        public string airDriVRPort = "COM3";

        private Vector3 lastVelocity = Vector3.zero;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private int accelSamples = 0;
        private int veloSamples = 0;
        private int gSamples = 0;
        private Vector3[] accelBuffer;
        private Vector3[] veloBuffer;
        private Vector3[] gBuffer;

        private Vector3 velo;
        private Vector3 accel;

        public Vector3 Acceleration => accel;

        public Vector3 G { get; private set; }

        private void Awake()
        {
            if(monitorMode == MonitorMode.Rigidbody && !rigidbody) rigidbody = GetComponent<Rigidbody>();
            accelBuffer = new Vector3[accelBufferSampleCount];
            veloBuffer = new Vector3[veloBufferSampleCount];
            gBuffer = new Vector3[gBufferSampleCount];
            lastPosition = transform.position;

            if (updateAirDriVR)
            {
                if(!AirDriVRSystem.HasInitialized) updateAirDriVR = AirDriVRSystem.Init(airDriVRPort);
                if(updateAirDriVR) AirDriVRSystem.SetHeadTransform(headTransform);
            }
        }

        private void Update()
        {
            if (!regularUpdateMode) return;
            
            Calculate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (regularUpdateMode) return;
            
            Calculate(Time.fixedDeltaTime);
        }

        private void Calculate(float deltaTime)
        {
            // velo(smooth) -> acc(smooth) -> G(smooth) -> setForce by G 
            if (monitorMode == MonitorMode.Transform)
            {
                var frameVelo = (transform.position - lastPosition) / deltaTime;

                veloBuffer[accelSamples % veloBufferSampleCount] = frameVelo;
                veloSamples++;

                velo = Vector3.zero;
                // SMOOTHING
                var veloCnt = Mathf.Min(veloSamples, veloBufferSampleCount);
                for (var i = 0; i < veloCnt; i++)
                {
                    velo += veloBuffer[i];
                }

                velo /= veloCnt; 
                lastPosition = transform.position;
            }
            
            var velocity = (monitorMode == MonitorMode.Rigidbody) ? rigidbody.velocity : velo;
            var frameAccel = (velocity - lastVelocity) / deltaTime;
            
            accelBuffer[accelSamples % accelBufferSampleCount] = frameAccel;
            accelSamples++;

            accel = Vector3.zero;
            // SMOOTHING
            var cnt = Mathf.Min(accelSamples, accelBufferSampleCount);
            for (var i = 0; i < cnt; i++)
            {
                accel += accelBuffer[i];
            }

            accel /= cnt;
            if (transformModeGravity && monitorMode == MonitorMode.Transform) accel += Physics.gravity;
            
            gBuffer[gSamples % gBufferSampleCount] = Quaternion.Inverse(transform.rotation) * frameAccel / Physics.gravity.y;
            gSamples++;

            G = Vector3.zero;
            var gCnt = Mathf.Min(gSamples, gBufferSampleCount);
            for (var i = 0; i < gCnt; i++)
            {
                G += gBuffer[i];
            }

            G /= gCnt;

            if (updateAirDriVR)
            {
                AirDriVRSystem.SetForce(new Vector2(G.x / airDriVRMaxG, G.z / airDriVRMaxG), 0, overrideMode);
            }

            lastVelocity = velocity;
            lastRotation = transform.rotation;
        }
    }
}