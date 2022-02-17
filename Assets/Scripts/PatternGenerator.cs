﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class PatternGenerator : MonoBehaviour
{
    public bool lowSusVibration = true;

    [Header("pattern choice")]
    // turn off then no vibration
    public bool intensityMappingEnable;
    // turn off then constant frequency
    public bool frequencyMappingEnable;
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
        tactileMotion_magToISOI = 3
    }
    public PatternType MotionPatternType = PatternType.directionalCue;
    public PatternType DecelPatternType = PatternType.directionalCue;
    public PatternType AccelPatternType = PatternType.directionalCue;
    public PatternType TurnPatternType = PatternType.directionalCue;

    public enum RegionType
    {
        none = 0,
        front = 1,
        right = 2,
        back = 3,
        left = 4,
        backCue = 5
    };
    private RegionType currentRegion = RegionType.none;
    private RegionType lastRegion = RegionType.none;

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

    private VirtualHeadband virtualHeadband;

    private float justBrakeTimer;

    private void Start()
    {
        motionTimer = 0.0f;
        justBrakeTimer = 0.0f;
        checkAngles();
        Gforce = Vector3.zero;
        intensityMappingEnable = true;
        virtualHeadband = gameObject.GetComponent<VirtualHeadband>();
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

        
        if(AccelPatternType == PatternType.tactileMotion_magToISOI)
        {
            // duration fixed at 0.2s
            // ISOI range: 0.05~0.12s

            // map ISOI to speed
            ISOI = maxISOI - (speed - speedMinThreshold) / (speedMaxThreshold - speedMinThreshold) * (maxISOI - minISOI);

            // map ISOI to G 
            // ISOI = 0.12f - (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
            ISOI = Mathf.Max(ISOI, minISOI);

            duration = minDuration + (ISOI - minISOI) / (maxISOI - minISOI) * (maxDuration - minDuration);
            motionInterval = 4.0f * duration;
        }

        /* bad
        if (defaultTactileMotionEnable && speed > 0.01f)
        {
            // duration fixed at 0.2s
            // magnitude is mapped to ISOI
            // ISOI range: 0.05~0.12s
            if (motionTimer < 0)
            {
                motionTimer = motionInterval;
                virtualHeadband.isMotion = true;
                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
            }
           
        }
        */

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
                            currentRegion = RegionType.front;
                            if(currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            virtualHeadband.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualHeadband.setAllToZero();
                        }
                        break;

                    case PatternType.tactileMotion_magToIntensity:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.front;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                            StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                        }
                        break;

                    case PatternType.tactileMotion_magToISOI:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {   
                            currentRegion = RegionType.front;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(true, 180.0f, 0.0f, 180.0f));
                            StartCoroutine(generateMotion(false, -180.0f, 0.0f, -180.0f));
                        }
                        break;
                }
            }
            else if(GforceAngle >= 180.0f - accAngleRange || GforceAngle <= -180.0f + accAngleRange || planarGforceMagnitude < 0.01f)
            {
                // acceleration, back part
                switch (AccelPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            virtualHeadband.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualHeadband.setAllToZero();
                        }
                        break;

                    case PatternType.tactileMotion_magToIntensity:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            if (motionTimer < 0)
                            {
                                motionTimer = motionInterval;
                                virtualHeadband.isMotion = true;
                                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                            }
                        }
                        else
                        {
                            if(lastRegion == RegionType.back)
                            {
                                // from acceleration to none, extend the motions
                                currentRegion = RegionType.back;
                                if (motionTimer < 0)
                                {
                                    motionTimer = motionInterval;
                                    virtualHeadband.isMotion = true;
                                    StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                                    StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                                }
                            }
                        }
                        break;

                    case PatternType.tactileMotion_magToISOI:
                        if(justBrakeTimer > 0.0f)
                        {
                            // just brake, use directional cue
                            if (planarGforceMagnitude > GforceMinThreshold)
                            {
                                currentRegion = RegionType.backCue;
                                if (currentRegion != lastRegion)
                                {
                                    StopAllCoroutines();
                                    virtualHeadband.setAllToZero();
                                }
                                virtualHeadband.isMotion = false;
                                convertGforce(GforceAngle, planarGforceMagnitude * 2);
                            }
                            else
                            {
                                virtualHeadband.setAllToZero();
                            }
                        }
                        else
                        {
                            // use tactile motion
                            if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                            {
                                currentRegion = RegionType.back;
                                if (currentRegion != lastRegion)
                                {
                                    StopAllCoroutines();
                                    virtualHeadband.setAllToZero();
                                }
                                // duration fixed at 0.2s
                                // magnitude is mapped to ISOI
                                // ISOI range: 0.05~0.12s
                                if (motionTimer < 0)
                                {
                                    motionTimer = motionInterval;
                                    virtualHeadband.isMotion = true;
                                    StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
                                    StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
                                }
                            }
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
                            currentRegion = RegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            virtualHeadband.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualHeadband.setAllToZero();
                        }
                        break;

                    case PatternType.tactileMotion_magToIntensity:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(true, 0.0f, -359.0f, 0.0f));
                        }
                        break;

                    case PatternType.tactileMotion_magToISOI:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(true, 0.0f, -359.0f, 0.0f));
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
                            currentRegion = RegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            virtualHeadband.isMotion = false;
                            convertGforce(GforceAngle, planarGforceMagnitude);
                        }
                        else
                        {
                            virtualHeadband.setAllToZero();
                        }
                        break;

                    case PatternType.tactileMotion_magToIntensity:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(false, 0.0f, 359.0f, 0.0f));
                        }
                        break;

                    case PatternType.tactileMotion_magToISOI:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            motionTimer = motionInterval;
                            virtualHeadband.isMotion = true;
                            StartCoroutine(generateMotion(false, 0.0f, 359.0f, 0.0f));
                        }
                        break;
                }
            }
        }

        lastRegion = currentRegion;

        if (frequencyMappingEnable)
        {
            float tmp;  
            switch (frequencyParameter)
            {
                case ParameterType.constant:
                    // set to default freq
                    for (int i = 0; i < 16; i++)
                    {
                        virtualHeadband.VibratorFrequencies[i] = (virtualHeadband.defaultFreq - virtualHeadband.minFreq) * 100 / (virtualHeadband.maxFreq - virtualHeadband.minFreq);
                    }
                    break;

                case ParameterType.Gforce:
                    for (int i = 0; i < 16; i++)
                    {
                        tmp = Mathf.Min(planarGforceMagnitude, GforceMaxThreshold);
                        virtualHeadband.VibratorFrequencies[i] = Mathf.FloorToInt((tmp - GforceMinThreshold) * 100.0f / (GforceMaxThreshold - GforceMinThreshold));
                    }
                    break;

                case ParameterType.velocity:
                    for (int i = 0; i < 16; i++)
                    {
                        tmp = Mathf.Min(speed, speedMaxThreshold);
                        virtualHeadband.VibratorFrequencies[i] = Mathf.FloorToInt((tmp - speedMinThreshold) * 100.0f / (speedMaxThreshold - speedMinThreshold));
                    }
                    break;
            }
        }

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
                virtualHeadband.VibratorIntensities[i] = 0;
            }
        }
    }


    private void setVibratorIntensity(float angleDiff, float G_mag, int VibratorIndex)
    {
        virtualHeadband.VibratorIntensities[VibratorIndex] = Mathf.FloorToInt(calculateIntensity(angleDiff, G_mag));
        // virtualHeadband.VibratorLifeSpans[VibratorIndex] = 1.0f;
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        G_mag = Mathf.Min(GforceMaxThreshold, G_mag);
        G_mag = Mathf.Max(G_mag, GforceMinThreshold);
        float ret = (G_mag - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 100.0f * (angleThreshold - angleDiff) / angleThreshold;
        return ret;
    }


    private IEnumerator generateMotion(bool isClockwise, float startAngle, float endAngle, float currentAngle)
    {
        if((isClockwise && currentAngle < endAngle) || (!isClockwise && currentAngle > endAngle))
        {
            yield break;
        }
        float angleSpeed = 22.5f / ISOI;
        float angleStep = angleSpeed * Time.deltaTime;
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
                virtualHeadband.VibratorLifeSpans[i] = duration;
                if (MotionPatternType == PatternType.tactileMotion_magToIntensity)
                {
                    virtualHeadband.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, planarGforceMagnitude));
                }
                else
                {
                    // From 0 ~ 100
                    virtualHeadband.VibratorMotionIntensities[i] = Mathf.FloorToInt(calculateIntensity(0.0f, speed / (speedMaxThreshold - speedMinThreshold)));
                }
            }
        }

        yield return 0;
        StartCoroutine(generateMotion(isClockwise, startAngle, endAngle, nextAngle));
        yield break;
    }

}