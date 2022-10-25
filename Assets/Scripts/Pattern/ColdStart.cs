using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class ColdStart : Pattern
{
    public float ISOI = 0.05f;
    public float DurationOverlapByMotor = 4.0f;
    public float MotionIntervalByMotor = 5.0f;
    public int MotionIntensity = 40;
    public float SpeedThreshold = 5.0f;
    [HideInInspector]
    public bool IsTactileMotionOngoing = false;

    private float duration;
    private float motionInterval;
    private float motionTimer = 0.0f;
    private float speed;
    private float[] tactileMotionLifeSpans = new float[16];
    private int[] rawIntensity = new int[16];

    private void Start()
    {
        motionTimer = 0.0f; for (int i = 0; i < 16; i++)
        {
            tactileMotionLifeSpans[i] = -1.0f;
            rawIntensity[i] = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!IsEnable)
        {
            SetIntensitiesToZero();
            return;
        }

        for (int i = 0; i < 16; i++)
        {
            tactileMotionLifeSpans[i] -= Time.fixedDeltaTime;
        }
        motionTimer -= Time.fixedDeltaTime;
        speed = GetComponent<ACListener>().velocity;
        duration = DurationOverlapByMotor * ISOI;
        motionInterval = duration + MotionIntervalByMotor * ISOI;

        if (GetComponent<ACListener>().gas > 0.5 && speed < SpeedThreshold && motionTimer < 0)
        {
            motionTimer = motionInterval;
            StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f));
            StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f));
        }

        IsTactileMotionOngoing = false;
        for (int i = 0; i < 16; i++)
        {
            if (tactileMotionLifeSpans[i] > 0)
            {
                IsTactileMotionOngoing = true;
                HeadbandIntensities[i] = rawIntensity[i];
            }
            else
            {
                HeadbandIntensities[i] = 0;
            }
        }
    }

    private IEnumerator generateMotion(bool isClockwise, float startAngle, float endAngle, float currentAngle)
    {
        if ((isClockwise && currentAngle < endAngle) || (!isClockwise && currentAngle > endAngle))
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
                if (currentAngle > AngleOfEachVibrator[i] && nextAngle <= AngleOfEachVibrator[i])
                {
                    activateThisIndex = true;
                }
                else if (currentAngle > AngleOfEachVibrator[i] + 360.0f && nextAngle <= AngleOfEachVibrator[i] + 360.0f)
                {
                    activateThisIndex = true;
                }
                else if (currentAngle > AngleOfEachVibrator[i] - 360.0f && nextAngle <= AngleOfEachVibrator[i] - 360.0f)
                {
                    activateThisIndex = true;
                }
            }
            else
            {
                if (currentAngle < AngleOfEachVibrator[i] && nextAngle >= AngleOfEachVibrator[i])
                {
                    activateThisIndex = true;
                }
                else if (currentAngle < AngleOfEachVibrator[i] + 360.0f && nextAngle >= AngleOfEachVibrator[i] + 360.0f)
                {
                    activateThisIndex = true;
                }
                else if (currentAngle < AngleOfEachVibrator[i] - 360.0f && nextAngle >= AngleOfEachVibrator[i] - 360.0f)
                {
                    activateThisIndex = true;
                }
            }
            if (i == 0 && currentAngle == startAngle)
            {
                activateThisIndex = true;
            }

            if (activateThisIndex)
            {
                tactileMotionLifeSpans[i] = duration;
                rawIntensity[i] = MotionIntensity;
            }
        }

        yield return new WaitForFixedUpdate();
        StartCoroutine(generateMotion(isClockwise, startAngle, endAngle, nextAngle));
        yield break;
    }

}
