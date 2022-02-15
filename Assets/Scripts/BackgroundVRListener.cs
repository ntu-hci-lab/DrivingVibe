using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace AirDriVR
{
    public class BackgroundVRListener : MonoBehaviour
    {
        [HideInInspector]
        public TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        [HideInInspector]
        public TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

        public bool useOnlyYAxisRotation = true;
        private Vector3 _offsetForward = Vector3.forward;

        public Text posText;

        public Quaternion HeadRotation
        {
            get
            {
                var f = transform.forward;
                if (useOnlyYAxisRotation)
                {
                    _offsetForward.y = 0;
                    f.y = 0;
                }
                return Quaternion.FromToRotation(_offsetForward, f);
            }
        }
    
        private void Start()
        {
            InitOpenVR();
        }

        public void InitOpenVR()
        {
            //OpenVR.Shutdown();
            var err = EVRInitError.None;
            OpenVR.Init(ref err, EVRApplicationType.VRApplication_Background);
            if (err != EVRInitError.None)
            {
                Debug.LogError(err);
                enabled = false;
                return;
            }
            Debug.Log("Success; Start listening hmd rotation in background");
        }

        public void CalibrateOffset()
        {
            _offsetForward = transform.forward;
        }

        public void Update()
        {
            OpenVR.Compositor.GetLastPoses(poses, gamePoses);

            var i = 0;

            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            transform.localPosition = pose.pos;
            transform.localRotation = pose.rot;
            posText.text = transform.localRotation.ToString();
        }

        private void OnDisable()
        {
            OpenVR.Shutdown();
        }
    }
}
