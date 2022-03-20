using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomVibration : MonoBehaviour
{
    private int numOfVibrators;
    private int frequency;
    private int intensity;
    private float duration;
    public float vibratorSwitchInterval = 3.0f;
    public float vibratorSwitchTimer;
    public float parameterSwitchInterval = 2.0f;
    public float parameterSwitchTimer;
    public float noiseSampleInterval = 0.5f;
    public float noiseSampleTimer;
    public float whiteNoise;

    private int index;
    private VirtualHeadband virtualHeadband;
    private bool[] isChosen = new bool[16];

    private void Start()
    {
        vibratorSwitchTimer = 0.0f;
        parameterSwitchTimer = 0.0f;
        noiseSampleTimer = 0.0f;
        virtualHeadband = gameObject.GetComponent<VirtualHeadband>();
        for(int i = 0; i < 16; i++)
        {
            isChosen[i] = false;
        }
        GetComponent<PatternGenerator>().intensityMappingEnable = false;
        GetComponent<additionalIntensityGenerator>().feedbackEnable = false;

    }

    private void Update()
    {
        //whiteNoise = generateNormalRandom(0, 1);
        whiteNoise = Random.Range(-1, 1);

        vibratorSwitchTimer -= Time.deltaTime;
        parameterSwitchTimer -= Time.deltaTime;
        noiseSampleTimer -= Time.deltaTime;

        if (noiseSampleTimer < 0)
        {
            if (whiteNoise < 0)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (isChosen[i])
                    {
                        virtualHeadband.VibratorLifeSpans[i] = noiseSampleInterval;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    virtualHeadband.VibratorLifeSpans[i] = 0.0f;
                }
            }
            noiseSampleTimer = noiseSampleInterval;
        }

        if(parameterSwitchTimer < 0)
        {
            frequency = Random.Range(0, 100);
            intensity = Random.Range(0, 100);
            for (int i = 0; i < 16; i++)
            {
                if (isChosen[i])
                {
                    intensity = Random.Range(0, 100);
                    virtualHeadband.VibratorIntensities[i] = intensity;
                }
            }
            parameterSwitchInterval = Random.Range(2, 4);
            parameterSwitchTimer = parameterSwitchInterval;
        }

        if(vibratorSwitchTimer < 0)
        {
            numOfVibrators = Random.Range(3, 6);
            switchVibrator();
            //vibratorSwitchInterval = Random.Range(2, 4);
            vibratorSwitchTimer = vibratorSwitchInterval;
        }
    }

    private void switchVibrator()
    {
        for (int i = 0; i < 16; i++)
        {
            isChosen[i] = false;
        }
        int leftAmount = numOfVibrators;
        int index;
        while(leftAmount > 0)
        {
            index = Random.Range(0, 16);
            if (!isChosen[index])
            {
                leftAmount--;
                isChosen[index] = true;
            }
        }
    }

    private static float generateNormalRandom(float mu, float sigma)
    {
        float rand1 = Random.Range(0.0f, 1.0f);
        float rand2 = Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        return (mu + sigma * n);
    }
}
