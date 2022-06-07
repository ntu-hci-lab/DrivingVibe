using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirDriVR;

public class ShowVibration : MonoBehaviour
{
    // from 0~100%
    public int[] HeadbandState = new int[16];
    public GameObject[] Motors;
    private RawImage[] MotorImages = new RawImage[16];

    public float[] newSuspension = new float[4];
    public GameObject frontRight;
    public GameObject frontLeft;
    public GameObject backRight;
    public GameObject backLeft;
    private Color maxColor;
    private VirtualHeadband virtualHeadband;

    void Start()
    {
        /*
        VibratorIntensities = gameObject.GetComponent<VirtualLayer>().VibratorIntensities;
        VibratorLifeSpans = gameObject.GetComponent<VirtualLayer>().VibratorLifeSpans;
        VibratorMotionIntensities = gameObject.GetComponent<VirtualLayer>().VibratorMotionIntensities;
        VibratorAddiIntensities = gameObject.GetComponent<VirtualLayer>().VibratorAdditionalIntensities;
        VibratorAddiLifeSpans = gameObject.GetComponent<VirtualLayer>().VibratorAddiLifeSpans;
        */
        virtualHeadband = GetComponent<VirtualHeadband>();
        HeadbandState = virtualHeadband.HeadbandIntensity;
        maxColor = Color.red;
        for (int i = 0; i < 16; i++)
        {
            // HeadbandState[i] = (char)0;
            MotorImages[i] = Motors[i].GetComponent<RawImage>();
            Motors[i].GetComponent<RectTransform>().localPosition
                = new Vector2(100.0f * Mathf.Cos(Mathf.PI / 2 - i * Mathf.PI / 8), 100.0f * Mathf.Sin(Mathf.PI / 2 - i * Mathf.PI / 8));
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 16; i++)
        {
            //maximum intensity is 40
            /*
            if ((VibratorIntensities[i] > 0 || VibratorLifeSpans[i] > 0 || VibratorAddiLifeSpans[i] > 0) && (VibratorIntensities[i] + VibratorMotionIntensities[i] + VibratorAddiIntensities[i])> 0 && VibratorIntensities[i] <= 100)
            {
                MotorImages[i].color = Color.Lerp(Color.white, maxColor, ((float)VibratorIntensities[i] + VibratorMotionIntensities[i] / 2 + VibratorAddiIntensities[i] / 2) / 120.0f);
            }
            else
            {
                MotorImages[i].color = Color.white;
            }
            */

            MotorImages[i].color = Color.Lerp(Color.white, maxColor, (HeadbandState[i]/100.0f));
        }
    }
}
