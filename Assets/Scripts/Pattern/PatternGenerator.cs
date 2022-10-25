using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class PatternGenerator : MonoBehaviour
{
    [Header("pattern choice")]
    // turn off then no vibration
    public bool GforcePatternEnable = true;
    //public bool RoadShakeVer0Enable = true;
    //public bool RoadShakeVer1Enable = false;
    public bool RoadShakeVer2Enable = false;
    public enum ParameterType
    {
        constant = 0,
        Gforce = 1,
        velocity = 2
    };
    public enum PatternType
    {
        none = 0,
        directionalCue = 1,
        tactileMotion_magToIntensity = 2,
        tactileMotion_magToISOI = 3,
        cue_and_tactileMotion = 4
    }
    public PatternType MotionPatternType = PatternType.directionalCue;
    public PatternType DecelPatternType = PatternType.directionalCue;
    public PatternType AccelPatternType = PatternType.directionalCue;
    public PatternType TurnPatternType = PatternType.directionalCue;


    [Header("Directional Cue related")]
    public VibrationRegionType currentRegion = VibrationRegionType.none;
    private VibrationRegionType lastRegion = VibrationRegionType.none;
    // back
    [HideInInspector]
    public float accAngleRange = 45.0f;
    // front
    [HideInInspector]
    public float decAngleRange = 45.0f;
    public float GforceMinThreshold = 0.1f;
    public float GforceMaxThreshold = 1.3f;
    public enum VibrationRegionType
    {
        none = 0,
        front = 1,
        right = 2,
        back = 3,
        left = 4,
        backCue = 5
    };
    private float angleThreshold = 45.0f; // bad method

    [Header("Tactile Motion related")]
    public float duration = 0.2f;
    public float ISOI = 0.1f;
    private float motionInterval = 0.5f;
    private float motionTimer;
    public float minISOI = 0.04f;
    public float maxISOI = 0.06f;
    public float DurationOverlapByMotor = 4.0f;
    public float MotionIntervalByMotor = 5.0f;
    [HideInInspector]
    public bool MotionOnlyWhenHighVelocity;
    [HideInInspector]
    public float VelocityThresholdToMotion = 20.0f;
    public int fixMotionIntensity = 40;
    public bool VariableMotionIntensity;
    [HideInInspector]
    public int maxMotionIntensity = 100;
    [HideInInspector]
    public float speedMinThreshold = 0.0f;
    [HideInInspector]
    public float speedMaxThreshold = 30.0f;

    [Header("Road Shake related")]
    public float maxRoadShakeIntensity = 50.0f;
    public float TyreSpeedMinThreshold = 0.02f;
    public float TyreSpeedMaxThreshold = 0.4f;
    [HideInInspector]
    public float[] suspensionDiff = new float[4];
    // front right: 0 1 2 3 4
    private int[] frontRightIndice = { 1, 2, 3 };
    // front left: 12 13 14 15 0
    private int[] frontLeftIndice = { 13, 14, 15 };
    // back right: 4 5 6 7 8
    private int[] backRightIndice = { 5, 6, 7 };
    // back left: 8 9 10 11 12
    private int[] backLeftIndice = { 9, 10, 11 };


    [Header("Demo Playground")]
    public bool GearShiftPatternEnable = false;
    public int GearShiftIntensity = 40;
    public bool RPMappingToMotion = false;
    public float RPMThreshold  = 7000;
    public bool startMotionEnable = false;
    public float lowSpeedThreshold = 10.0f;

    [Header("Number Inspection")]
    //[HideInInspector]
    public Vector2 planarGforce;
    [HideInInspector]
    public Vector3 Gforce;
    //[HideInInspector]
    public float planarGforceMagnitude;
    [HideInInspector]
    public float GforceAngle;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public float[] angleOfEachVibrator = new float[16]; // 0 degree -> forward, increase clockwisely from -180 ~ 180

    [HideInInspector]
    // From 0~100 intensity
    public int[] DirectionalCueIntensities = new int[16];
    [HideInInspector]
    // From 0~100 intensity
    public int[] TactileMotionIntensities = new int[16];
    [HideInInspector]
    // From 0~100 intensity
    public int[] RoadShakeIntensities = new int[16];
    [HideInInspector]
    // For tactile motion
    public float[] TactileMotionLifeSpans = new float[16];
    [HideInInspector]
    public bool isTactileMotionOngoing = false;
    private int FramesPerUpdate = 1;


    private void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            DirectionalCueIntensities[i] = 0;
            TactileMotionIntensities[i] = 0;
            RoadShakeIntensities[i] = 0;
            TactileMotionLifeSpans[i] = -1.0f;
        }
        motionTimer = 0.0f;
        checkAngles();
        Gforce = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for(int i = 0; i < 16; i++)
        {
            TactileMotionLifeSpans[i] -= FramesPerUpdate * Time.fixedDeltaTime;
        }
        motionTimer -= Time.fixedDeltaTime;
        speed = GetComponent<ACListener>().velocity;

        // old version of taking inertia as parameter
        // get computed acceleration
        // convert them into planar value (ignore y component)
        planarGforce = -1 * GetComponent<ACListener>().Gforce;
        /*
        if(planarGforce.magnitude < GforceMinThreshold)
        {
            virtualLayer.setAllToZero();
            return;
        }
        */
        //Gforce = new Vector3(planarGforce.x, 0, planarGforce.y);
        //Gforce = Quaternion.Inverse(GetComponent<BackgroundVRListener>().HeadRotation) * Gforce;
        //planarGforce = new Vector2(Gforce.x, Gforce.z);

        planarGforceMagnitude = planarGforce.magnitude;
        GforceAngle = Vector2.SignedAngle(planarGforce, new Vector2(0.0f, 1.0f));
        //invGforceAngle = Vector2.SignedAngle(planarGforce, new Vector2(0.0f, 1.0f));
        /*
        if (GearShiftPatternEnable && GetComponent<ACListener>().gear == 1 && speed > 0.1f)
        {
            // make all vibrate
            for (int i = 0; i < 16; i++)
            {
                DirectionalCueIntensities[i] = GearShiftIntensity;
                //turn off other intensities
                TactileMotionIntensities[i] = 0;
                RoadShakeIntensities[i] = 0;
            }
            return;
        }
        */

        if (AccelPatternType == PatternType.cue_and_tactileMotion || RPMappingToMotion || startMotionEnable)
        {
            // duration range: 0.1~0.2s
            // ISOI range: 0.06~0.12s

            // map ISOI to speed
            ISOI = maxISOI - (speed - speedMinThreshold) / (speedMaxThreshold - speedMinThreshold) * (maxISOI - minISOI);

            // map ISOI to G 
            // ISOI = 0.12f - (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
            ISOI = Mathf.Max(ISOI, minISOI);
            //ISOI = 0.12f;

            //duration = minDuration + (ISOI - minISOI) / (maxISOI - minISOI) * (maxDuration - minDuration);
            duration = DurationOverlapByMotor * ISOI;

            //motionInterval = duration + 4.0f * ISOI;
            motionInterval = duration + MotionIntervalByMotor * ISOI;
        }

        if (GforcePatternEnable)
        {
            if(GforceAngle <= decAngleRange && GforceAngle >= -decAngleRange && planarGforceMagnitude > 0.01f)
            {
                currentRegion = VibrationRegionType.front;
                // deceleration, front part
                switch (DecelPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.front;
                            if(currentRegion != lastRegion)
                            {
                                //motionTimer = -1.0f;
                                //StopAllCoroutines();
                                //virtualLayer.setAllToZero();
                            }
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            SetCueIntensitiesToZero();
                        }
                        break;
                }
            }
            else if(GforceAngle >= 180.0f - accAngleRange || GforceAngle <= -180.0f + accAngleRange)
            {
                currentRegion = VibrationRegionType.back;
                // acceleration, back part
                switch (AccelPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                //motionTimer = -1.0f;
                                //StopAllCoroutines();
                                //virtualLayer.setAllToZero();
                            }
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            SetCueIntensitiesToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        currentRegion = VibrationRegionType.back;
                        if (currentRegion != lastRegion)
                        {
                            //motionTimer = -1.0f;
                            //StopAllCoroutines();
                            //virtualLayer.setAllToZero();
                        }
                        // use directional cue
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        // use tactile motion magnitude is mapped to ISOI
                        if (motionTimer < 0)
                        {
                            if(!MotionOnlyWhenHighVelocity || speed > VelocityThresholdToMotion)
                            {
                                motionTimer = motionInterval;
                                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                            }
                        }
                        break;
                }
            }
            else if(GforceAngle < -decAngleRange && GforceAngle > -180.0f + accAngleRange)
            {
                currentRegion = VibrationRegionType.right;
                // left turn, right part
                switch (TurnPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                //motionTimer = -1.0f;
                                //StopAllCoroutines();
                                //virtualLayer.setAllToZero();
                            }
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            SetCueIntensitiesToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        // Vibration at right, tactile motion at left
                        currentRegion = VibrationRegionType.right;
                        if (currentRegion != lastRegion)
                        {
                            //motionTimer = -1.0f;
                            //StopAllCoroutines();
                            //virtualLayer.setAllToZero();
                        }
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        if (motionTimer < 0)
                        {
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            motionTimer = motionInterval;
                            StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                        }
                        break;
                }
            }
            else if(GforceAngle > decAngleRange && GforceAngle < 180.0f - accAngleRange)
            {
                currentRegion = VibrationRegionType.left;
                // right turn, left part
                switch (TurnPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                //motionTimer = -1.0f;
                                //StopAllCoroutines();
                                //virtualLayer.setAllToZero();
                            }
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            SetCueIntensitiesToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        // Vibration at left, tactile motion at right
                        currentRegion = VibrationRegionType.left;
                        if (currentRegion != lastRegion)
                        {
                            motionTimer = -1.0f;
                            StopAllCoroutines();
                            SetCueIntensitiesToZero();
                        }
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        if (motionTimer < 0)
                        {
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            motionTimer = motionInterval;
                            StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                        }
                        break;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    DirectionalCueIntensities[i] = 0;
                }
            }
        }
        else
        {
            /*
            // not exceed threshold, probably in straight going
            if (motionTimer < 0)
            {
                if (!MotionOnlyWhenHighVelocity || speed > VelocityThresholdToMotion)
                {
                    currentRegion = VibrationRegionType.none;
                    motionTimer = motionInterval;
                    StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                    StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                }
            }
            */
            // not exceed threshold, probably in straight going
            // make all stop
            for (int i = 0; i < 16; i++)
            {
                DirectionalCueIntensities[i] = 0;
            }
        }

        /*
        if (RPMappingToMotion && (currentRegion == VibrationRegionType.front || currentRegion == VibrationRegionType.back))
        {
            if (GetComponent<ACListener>().RPM > RPMThreshold && motionTimer < 0)
            {
                motionTimer = motionInterval;
                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
            }
        }
        */

        if (startMotionEnable)
        {
            if (GetComponent<ACListener>().gas > 0.5 && speed < lowSpeedThreshold && motionTimer < 0)
            {
                motionTimer = motionInterval;
                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
            }
        }

        // Road shake pattern
        /*
        if (RoadShakeVer0Enable)
        {
            suspensionDiff = gameObject.GetComponent<ACListener>().unBufferedSuspensionDiff;
            for (int i = 0; i < 3; i++)
            {
                // front left: 12 13 14 15 0
                if (suspensionDiff[0] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[0], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = 0;
                }

                // front right: 0 1 2 3 4
                if (suspensionDiff[1] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[1], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontRightIndice[i]] = 0;
                }

                // back left: 8 9 10 11 12
                if (suspensionDiff[2] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[2], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backLeftIndice[i]] = 0;
                }

                // back right: 4 5 6 7 8
                if (suspensionDiff[3] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[3], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backRightIndice[i]] = 0;
                }
            }
        }
        if (RoadShakeVer1Enable)
        {
            suspensionDiff = gameObject.GetComponent<ACListener>().suspensionDiff;
            for (int i = 0; i < 4; i++)
            {
                suspensionDiff[i] = Mathf.Abs(suspensionDiff[i]);
            }
            for (int i = 0; i < 3; i++)
            {
                // front left: 12 13 14 15 0
                if (suspensionDiff[0] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[0], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = 0;
                }

                // front right: 0 1 2 3 4
                if (suspensionDiff[1] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[1], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontRightIndice[i]] = 0;
                }

                // back left: 8 9 10 11 12
                if (suspensionDiff[2] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[2], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backLeftIndice[i]] = 0;
                }

                // back right: 4 5 6 7 8
                if (suspensionDiff[3] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[3], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backRightIndice[i]] = 0;
                }
            }
        }
        */
        if (RoadShakeVer2Enable)
        {
            suspensionDiff = gameObject.GetComponent<ACListener>().suspensionDiff;
            //TyreSpeedMinThreshold = 0.005f + Mathf.Clamp(speed / 15.0f ,0.0f,15.0f) * 0.025f;
            //TyreSpeedMaxThreshold = 0.05f + Mathf.Clamp(speed / 15.0f, 0.0f, 15.0f) * 0.25f;
            for (int i = 0; i < 3; i++)
            {
                // front left: 12 13 14 15 0
                if (suspensionDiff[0] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[0], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontLeftIndice[i]] = 0;
                }

                // front right: 0 1 2 3 4
                if (suspensionDiff[1] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[frontRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[1], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[frontRightIndice[i]] = 0;
                }

                // back left: 8 9 10 11 12
                if (suspensionDiff[2] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backLeftIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[2], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backLeftIndice[i]] = 0;
                }

                // back right: 4 5 6 7 8
                if (suspensionDiff[3] > TyreSpeedMinThreshold)
                {
                    RoadShakeIntensities[backRightIndice[i]] = Mathf.FloorToInt(Mathf.Min(suspensionDiff[3], TyreSpeedMaxThreshold) * maxRoadShakeIntensity / TyreSpeedMaxThreshold);
                }
                else
                {
                    RoadShakeIntensities[backRightIndice[i]] = 0;
                }
            }
        }

        // check if there is any tactile motion at the moment
        isTactileMotionOngoing = false;
        for(int i = 0; i < 16; i++)
        {
            if(TactileMotionLifeSpans[i] > 0)
            {
                isTactileMotionOngoing = true;
            }
        }

        lastRegion = currentRegion;
    }

    void checkAngles()
    {
        // check if all angle values are zero
        float tmp = 0;
        for (int i = 0; i < 16; i++)
        {
            tmp += angleOfEachVibrator[i] * angleOfEachVibrator[i];

            // set all angles into -180 ~ 180
            while (angleOfEachVibrator[i] < -180.0f)
            {
                angleOfEachVibrator[i] += 360.0f;
            }
            while (angleOfEachVibrator[i] >= 180.0f)
            {
                angleOfEachVibrator[i] -= 360.0f;
            }
        }
        if (tmp == 0)
        {
            // if so, set to default values
            setDefaultAngle();
        }
    }
    void setDefaultAngle()
    {
        // 0 -> front
        // counter clockwisely increase

        angleOfEachVibrator[0] = 0.0f;
        angleOfEachVibrator[1] = -22.5f;
        angleOfEachVibrator[2] = -45.0f;
        angleOfEachVibrator[3] = -67.5f;
        angleOfEachVibrator[4] = -90.0f;
        angleOfEachVibrator[5] = -112.5f;
        angleOfEachVibrator[6] = -135.0f;
        angleOfEachVibrator[7] = -157.5f;
        angleOfEachVibrator[8] = 180.0f;
        angleOfEachVibrator[9] = 157.5f;
        angleOfEachVibrator[10] = 135.0f;
        angleOfEachVibrator[11] = 112.5f;
        angleOfEachVibrator[12] = 90.0f;
        angleOfEachVibrator[13] = 67.5f;
        angleOfEachVibrator[14] = 45.0f;
        angleOfEachVibrator[15] = 22.5f;
    }

    private void convertGforce(float angle, float magnitude)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(angle - angleOfEachVibrator[i]) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i]), magnitude, i);
            }
            else if (Mathf.Abs(angle - angleOfEachVibrator[i] + 360.0f) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i] + 360.0f), magnitude, i);
            }
            else if (Mathf.Abs(angle - angleOfEachVibrator[i] - 360.0f) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i] - 360.0f), magnitude, i);
            }
            else
            {
                DirectionalCueIntensities[i] = 0;
            }
        }
    }


    private void setVibratorIntensity(float angleDiff, float G_mag, int VibratorIndex)
    {
        // from 0~100
        DirectionalCueIntensities[VibratorIndex] = Mathf.FloorToInt(calculateIntensity(angleDiff, G_mag));
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        float ret = 0;
        float angleMult = (angleThreshold - angleDiff) / angleThreshold;
        // Cropping
        G_mag = Mathf.Min(GforceMaxThreshold, G_mag);
        G_mag = Mathf.Max(G_mag, GforceMinThreshold);
        ret = (G_mag / GforceMaxThreshold) * angleMult * 100.0f;
        return ret;
    }


    private IEnumerator generateMotion(bool isClockwise, float startAngle, float endAngle, float currentAngle)
    {
        if((isClockwise && currentAngle < endAngle) || (!isClockwise && currentAngle > endAngle))
        {
            yield break;
        }
        float angleSpeed = 22.5f / ISOI;
        float angleStep = angleSpeed * Time.fixedDeltaTime;
        float nextAngle = currentAngle;
        bool activateThisIndex = false;
        if (isClockwise)
        {
            nextAngle = currentAngle - angleStep;
        }
        else
        {
            nextAngle = currentAngle + angleStep;
        }

        for (int i = 0; i < 16; i++)
        {
            activateThisIndex = false;
            if (isClockwise)
            {
                if (currentAngle > angleOfEachVibrator[i] && nextAngle <= angleOfEachVibrator[i])
                {
                    activateThisIndex = true;
                }
                else if (currentAngle > angleOfEachVibrator[i] + 360.0f && nextAngle <= angleOfEachVibrator[i] + 360.0f)
                {
                    activateThisIndex = true;
                }
                else if (currentAngle > angleOfEachVibrator[i] - 360.0f && nextAngle <= angleOfEachVibrator[i] - 360.0f)
                {
                    activateThisIndex = true;
                }
            }
            else
            {
                if (currentAngle < angleOfEachVibrator[i] && nextAngle >= angleOfEachVibrator[i])
                {
                    activateThisIndex = true;
                }
                else if (currentAngle < angleOfEachVibrator[i] + 360.0f && nextAngle >= angleOfEachVibrator[i] + 360.0f)
                {
                    activateThisIndex = true;
                }
                else if (currentAngle < angleOfEachVibrator[i] - 360.0f && nextAngle >= angleOfEachVibrator[i] - 360.0f)
                {
                    activateThisIndex = true;
                }
            }
            if(i == 0 && currentAngle == startAngle)
            {
                activateThisIndex = true;
            }

            if (activateThisIndex)
            {
                TactileMotionLifeSpans[i] = duration;
                if (!VariableMotionIntensity)
                {
                    //virtualLayer.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, planarGforceMagnitude));
                    TactileMotionIntensities[i] = fixMotionIntensity;
                }
                else
                {
                    // From 0 ~ 100
                    TactileMotionIntensities[i] = Mathf.FloorToInt((maxMotionIntensity / 100.0f) * Mathf.Min(speedMaxThreshold, speed) / (speedMaxThreshold - speedMinThreshold));
                    // virtualLayer.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, planarGforceMagnitude));
                }
            }
        }

        yield return new WaitForFixedUpdate();
        StartCoroutine(generateMotion(isClockwise, startAngle, endAngle, nextAngle));
        yield break;
    }

    private void SetCueIntensitiesToZero()
    {
        for (int i = 0; i < 16; i++)
        {
            DirectionalCueIntensities[i] = 0;
            //TactileMotionIntensities[i] = 0;
            //RoadShakeIntensities[i] = 0;
            //TactileMotionLifeSpans[i] = -1.0f;
        }
    }
}


