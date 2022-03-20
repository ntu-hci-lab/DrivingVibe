using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class PatternGenerator : MonoBehaviour
{
    public bool lowSusVibration = true;

    [Header("pattern choice")]
    // turn off then no vibration
    public bool intensityMappingEnable;
    // turn on then always map velocity to tactile motion, remember to turn acc pattern to cue
    public bool defaultTactileMotionEnable;
    public enum ParameterType
    {
        constant = 0,
        Gforce = 1,
        velocity = 2
    };
    public ParameterType frequencyParameter = ParameterType.velocity;

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

    public enum VibrationRegionType
    {
        none = 0,
        front = 1,
        right = 2,
        back = 3,
        left = 4,
        backCue = 5
    };
    private VibrationRegionType currentRegion = VibrationRegionType.none;
    private VibrationRegionType lastRegion = VibrationRegionType.none;

    [Header("general variable")]
    // back
    public float accAngleRange = 45.0f;
    // front
    public float decAngleRange = 45.0f;
    public float GforceMinThreshold = 0.15f;
    public float GforceMaxThreshold = 1.0f;
    public float speedMinThreshold = 0.0f;
    public float speedMaxThreshold = 30.0f;

    [Header("motion variable")]
    public float motionInterval = 0.5f;
    private float motionTimer;
    public float motionStartAngle = 0.0f;
    public float duration = 0.2f;
    public float ISOI = 0.1f;
    private float minISOI = 0.06f;
    private float maxISOI = 0.12f;
    private float minDuration = 0.1f;
    private float maxDuration = 0.2f;

    [Header("directional cue variable")]
    public float angleThreshold = 30.0f; // bad method

    [HideInInspector]
    public Vector3 Gforce;
    //[HideInInspector]
    public Vector2 planarGforce;
    //[HideInInspector]
    public float planarGforceMagnitude;
    [HideInInspector]
    public float GforceAngle;
    private float invGforceAngle;

    [HideInInspector]
    public float speed;

    [HideInInspector]
    public float[] angleOfEachVibrator = new float[16]; // 0 degree -> forward, increase clockwisely from -180 ~ 180

    private VirtualLayer virtualLayer;

    private float justBrakeTimer;

    private void Start()
    {
        motionTimer = 0.0f;
        justBrakeTimer = 0.0f;
        checkAngles();
        Gforce = Vector3.zero;
        intensityMappingEnable = true;
        virtualLayer = gameObject.GetComponent<VirtualLayer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        motionTimer -= Time.fixedDeltaTime;
        justBrakeTimer -= Time.fixedDeltaTime;

        speed = GetComponent<ACListener>().velocity;

        // old version of taking inertia as parameter
        // get computed acceleration
        // convert them into planar value (ignore y component)
        planarGforce = -1 * GetComponent<ACListener>().Gforce;
        if(planarGforce.magnitude < 0.01f)
        {
            planarGforce = new Vector2(0.001f, 0.0f);
        }
        Gforce = new Vector3(planarGforce.x, 0, planarGforce.y);
        Gforce = Quaternion.Inverse(GetComponent<BackgroundVRListener>().HeadRotation) * Gforce;
        planarGforce = new Vector2(Gforce.x, Gforce.z);

        // Gforce interval: 0.15G ~ 1G
        planarGforceMagnitude = planarGforce.magnitude;
        GforceAngle = Vector2.SignedAngle(planarGforce, new Vector2(0.0f, 1.0f));
        invGforceAngle = Vector2.SignedAngle(planarGforce, new Vector2(0.0f, 1.0f));

        
        if(AccelPatternType == PatternType.cue_and_tactileMotion)
        {
            // duration range: 0.1~0.2s
            // ISOI range: 0.06~0.12s

            // map ISOI to speed
            ISOI = maxISOI - (speed - speedMinThreshold) / (speedMaxThreshold - speedMinThreshold) * (maxISOI - minISOI);

            // map ISOI to G 
            // ISOI = 0.12f - (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
            ISOI = Mathf.Max(ISOI, minISOI);
            //ISOI = 0.12f;

            duration = minDuration + (ISOI - minISOI) / (maxISOI - minISOI) * (maxDuration - minDuration);
            //duration = 0.2f;
            motionInterval = 6.0f * duration;
        }

        if (intensityMappingEnable)
        {
            if(GforceAngle <= decAngleRange && GforceAngle >= -decAngleRange && planarGforceMagnitude > 0.01f)
            {
                // deceleration, front part
                justBrakeTimer = 1.0f;
                switch (DecelPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.front;
                            if(currentRegion != lastRegion)
                            {
                                motionTimer = -1.0f;
                                StopAllCoroutines();
                                virtualLayer.setAllToZero();
                            }
                            virtualLayer.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualLayer.setAllToZero();
                        }
                        break;
                }
            }
            else if(GforceAngle >= 180.0f - accAngleRange || GforceAngle <= -180.0f + accAngleRange)
            {
                // acceleration, back part
                switch (AccelPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                motionTimer = -1.0f;
                                StopAllCoroutines();
                                virtualLayer.setAllToZero();
                            }
                            virtualLayer.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualLayer.setAllToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        currentRegion = VibrationRegionType.back;
                        if (currentRegion != lastRegion)
                        {
                            motionTimer = -1.0f;
                            StopAllCoroutines();
                            virtualLayer.setAllToZero();
                        }
                        // use directional cue
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        // use tactile motion magnitude is mapped to ISOI
                        if (motionTimer < 0)
                        {
                            motionTimer = motionInterval;
                            virtualLayer.isMotion = true;
                            StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                            StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                        }
                        break;
                }
            }
            else if(GforceAngle < -decAngleRange && GforceAngle > -180.0f + accAngleRange)
            {
                // left turn, right part
                switch (TurnPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                motionTimer = -1.0f;
                                StopAllCoroutines();
                                virtualLayer.setAllToZero();
                            }
                            virtualLayer.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualLayer.setAllToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        // Vibration at right, tactile motion at left
                        currentRegion = VibrationRegionType.right;
                        if (currentRegion != lastRegion)
                        {
                            motionTimer = -1.0f;
                            StopAllCoroutines();
                            virtualLayer.setAllToZero();
                        }
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        if (motionTimer < 0)
                        {
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            motionTimer = motionInterval;
                            virtualLayer.isMotion = true;
                            StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                        }
                        break;
                }
            }
            else if(GforceAngle > decAngleRange && GforceAngle < 180.0f - accAngleRange)
            {
                // right turn, left part
                switch (TurnPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = VibrationRegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                motionTimer = -1.0f;
                                StopAllCoroutines();
                                virtualLayer.setAllToZero();
                            }
                            virtualLayer.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualLayer.setAllToZero();
                        }
                        break;

                    case PatternType.cue_and_tactileMotion:
                        // Vibration at left, tactile motion at right
                        currentRegion = VibrationRegionType.left;
                        if (currentRegion != lastRegion)
                        {
                            motionTimer = -1.0f;
                            StopAllCoroutines();
                            virtualLayer.setAllToZero();
                        }
                        convertGforce(GforceAngle, planarGforceMagnitude);
                        if (motionTimer < 0)
                        {
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            motionTimer = motionInterval;
                            virtualLayer.isMotion = true;
                            StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                        }
                        break;
                }
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
                virtualLayer.VibratorIntensities[i] = 0;
            }
        }
    }


    private void setVibratorIntensity(float angleDiff, float G_mag, int VibratorIndex)
    {
        // from 0~100
        virtualLayer.VibratorIntensities[VibratorIndex] = Mathf.FloorToInt(calculateIntensity(angleDiff, G_mag));
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        G_mag = Mathf.Min(GforceMaxThreshold, G_mag);
        G_mag = Mathf.Max(G_mag, GforceMinThreshold);
        float ret = (G_mag / GforceMaxThreshold) * ((angleThreshold - angleDiff) / angleThreshold) * 100.0f;
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

            if (activateThisIndex)
            {
                virtualLayer.VibratorLifeSpans[i] = duration;
                if (MotionPatternType == PatternType.tactileMotion_magToIntensity)
                {
                    virtualLayer.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, planarGforceMagnitude));
                }
                else
                {
                    // From 0 ~ 100
                    virtualLayer.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, speed / (speedMaxThreshold - speedMinThreshold)));
                }
            }
        }

        yield return new WaitForFixedUpdate();
        StartCoroutine(generateMotion(isClockwise, startAngle, endAngle, nextAngle));
        yield break;
    }

}


