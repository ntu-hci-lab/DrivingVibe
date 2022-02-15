using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class partiallyRandomVibration : MonoBehaviour
{
    [Header("pattern choice")]
    // turn off then no vibration
    public bool intensityMappingEnable;
    // turn off then constant frequency
    public bool frequencyMappingEnable;
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
    public PatternType patternType = PatternType.directionalCue;
    public PatternType frontPatternType = PatternType.directionalCue;
    public PatternType backPatternType = PatternType.directionalCue;
    public PatternType lateralPatternType = PatternType.directionalCue;

    public enum RegionType
    {
        none = 0,
        front = 1,
        right = 2,
        back = 3,
        left = 4
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
    private float GforceMidThreshold = 0.5f;
    public float speedMinThreshold = 0.0f;
    public float speedMaxThreshold = 10.0f;

    [Header("motion variable")]
    public float motionInterval = 0.5f;
    public float motionTimer;
    public float motionStartAngle = 0.0f;

    [Header("directional cue variable")]
    public float angleThreshold = 30.0f; // bad method

    [HideInInspector]
    public Vector3 Gforce;
    [HideInInspector]
    public Vector2 planarGforce;
    [HideInInspector]
    public float planarGforceMagnitude;
    [HideInInspector]
    public float GforceAngle;
    private float invGforceAngle;

    [HideInInspector]
    public float speed;

    [HideInInspector]
    public float[] angleOfEachVibrator = new float[16]; // 0 degree -> forward, increase clockwisely from -180 ~ 180

    private VirtualHeadband virtualHeadband;
    public float accTimer = 0.0f;
    public float accTimerThreshold = 3.0f;

    private void Start()
    {
        GforceMidThreshold = (GforceMaxThreshold + GforceMinThreshold) / 2;
        motionTimer = 0.0f;
        checkAngles();
        Gforce = Vector3.zero;
        intensityMappingEnable = true;
        virtualHeadband = gameObject.GetComponent<VirtualHeadband>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        accTimer += Time.fixedDeltaTime;
        motionTimer -= Time.fixedDeltaTime;

        speed = GetComponent<ACListener>().velocity;

        // get computed acceleration
        //Gforce = GetComponent<ACListener>().Gforce; // in a unit of G

        // convert them into planar value (ignore y component)
        planarGforce = -1 * GetComponent<ACListener>().Gforce;
        if (planarGforce.magnitude < 0.01f)
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


        if (intensityMappingEnable)
        {
            if (GforceAngle <= decAngleRange && GforceAngle >= -decAngleRange)
            {
                // deceleration, front part
                switch (frontPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.front;
                            if (currentRegion != lastRegion)
                            {
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
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
                            StartCoroutine(generateMotion(true, 180.0f, 0.0f, 180.0f, 0.2f, 0.1f));
                            StartCoroutine(generateMotion(false, -180.0f, 0.0f, -180.0f, 0.2f, 0.1f));
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
                            float ISOI = 0.05f + (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
                            ISOI = Mathf.Min(ISOI, 0.12f);
                            StartCoroutine(generateMotion(true, 180.0f, 0.0f, 180.0f, 0.2f, ISOI));
                            StartCoroutine(generateMotion(false, -180.0f, 0.0f, -180.0f, 0.2f, ISOI));
                        }
                        break;
                }
            }
            else if (GforceAngle >= 180.0f - accAngleRange || GforceAngle <= -180.0f + accAngleRange)
            {
                // acceleration, back part
                switch (backPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
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
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // ISOI fixed at 0.1s
                            if (accTimer > accTimerThreshold)
                            {
                                // directional cue at back
                                convertGforce(180.0f, planarGforceMagnitude);
                            }
                            else if (motionTimer < 0)
                            {
                                motionTimer = motionInterval;
                                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f, 0.2f, 0.1f));
                                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f, 0.2f, 0.1f));
                            }
                        }
                        break;

                    case PatternType.tactileMotion_magToISOI:
                        if (planarGforceMagnitude > GforceMinThreshold && motionTimer < 0)
                        {
                            currentRegion = RegionType.back;
                            if (currentRegion != lastRegion)
                            {
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
                            // duration fixed at 0.2s
                            // magnitude is mapped to ISOI
                            // ISOI range: 0.05~0.12s
                            if (accTimer > accTimerThreshold)
                            {
                                // directional cue at back
                                convertGforce(180.0f, planarGforceMagnitude);
                            }
                            else if (motionTimer < 0)
                            {
                                motionTimer = motionInterval;
                                float ISOI = 0.05f + (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
                                ISOI = Mathf.Min(ISOI, 0.12f);
                                StartCoroutine(generateMotion(true, 0.0f, -180.0f, 0.0f, 0.2f, ISOI));
                                StartCoroutine(generateMotion(false, 0.0f, 180.0f, 0.0f, 0.2f, ISOI));
                            }
                        }
                        break;
                }
            }
            else if (GforceAngle < -decAngleRange && GforceAngle > -180.0f + accAngleRange)
            {
                // left turn, right part
                switch (lateralPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.right;
                            if (currentRegion != lastRegion)
                            {
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
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
                            StartCoroutine(generateMotion(true, 0.0f, -359.0f, 0.0f, 0.2f, 0.1f));
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
                            float ISOI = 0.05f + (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
                            ISOI = Mathf.Min(ISOI, 0.12f);
                            StartCoroutine(generateMotion(true, 0.0f, -359.0f, 0.0f, 0.2f, ISOI));
                        }
                        break;
                }
            }
            else if (GforceAngle > decAngleRange && GforceAngle < 180.0f - accAngleRange)
            {
                // right turn, left part
                switch (lateralPatternType)
                {
                    case PatternType.directionalCue:
                        if (planarGforceMagnitude > GforceMinThreshold)
                        {
                            currentRegion = RegionType.left;
                            if (currentRegion != lastRegion)
                            {
                                accTimer = 0.0f;
                                StopAllCoroutines();
                                virtualHeadband.setAllToZero();
                            }
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
                            StartCoroutine(generateMotion(false, 0.0f, 359.0f, 0.0f, 0.2f, 0.1f));
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
                            float ISOI = 0.05f + (planarGforceMagnitude - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 0.07f;
                            ISOI = Mathf.Min(ISOI, 0.12f);
                            StartCoroutine(generateMotion(false, 0.0f, 359.0f, 0.0f, 0.2f, ISOI));
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
        else
        {
            for (int i = 0; i < 16; i++)
            {
                virtualHeadband.VibratorFrequencies[i] = (virtualHeadband.defaultFreq - virtualHeadband.minFreq) * 100 / (virtualHeadband.maxFreq - virtualHeadband.minFreq);
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
        for(int i = 0; i < 16; i++)
        {
            virtualHeadband.VibratorIntensities[i] = 0;
        }

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
                // virtualHeadband.VibratorIntensities[i] = 0;
            }
        }
    }


    private void setVibratorIntensity(float angleDiff, float G_mag, int VibratorIndex)
    {
        int index = Random.Range(0, 16);
        virtualHeadband.VibratorIntensities[index] = Mathf.FloorToInt(calculateIntensity(angleDiff, G_mag));
        virtualHeadband.VibratorLifeSpans[index] = 1.0f;
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        G_mag = Mathf.Min(GforceMaxThreshold, G_mag);
        float ret = (G_mag - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * 100.0f * (angleThreshold - angleDiff) / angleThreshold;
        return ret;
    }


    private IEnumerator generateMotion(bool isClockwise, float startAngle, float endAngle, float currentAngle, float duration, float ISOI)
    {
        if ((isClockwise && currentAngle < endAngle) || (!isClockwise && currentAngle > endAngle))
        {
            yield break;
        }
        float angleSpeed = 22.5f / ISOI;
        float angleStep = angleSpeed * Time.deltaTime;
        float nextAngle = currentAngle;
        bool activateThisIndex = false;
        int randomIndex;
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
                randomIndex = Random.Range(0, 16);
                virtualHeadband.VibratorLifeSpans[randomIndex] = duration;
                if (patternType == PatternType.tactileMotion_magToIntensity)
                {
                    virtualHeadband.VibratorIntensities[randomIndex] = Mathf.FloorToInt(calculateIntensity(0.0f, planarGforceMagnitude));
                }
                else
                {
                    virtualHeadband.VibratorIntensities[randomIndex] = 50;
                }
            }
        }

        yield return 0;
        StartCoroutine(generateMotion(isClockwise, startAngle, endAngle, nextAngle, duration, ISOI));
        yield break;
    }
}
