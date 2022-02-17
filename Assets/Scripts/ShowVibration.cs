using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirDriVR;

public class ShowVibration : MonoBehaviour
{
    protected int[] VibratorIntensities = new int[16];
    protected int[] VibratorMotionIntensities = new int[16];
    protected int[] VibratorAddiIntensities = new int[16];
    protected int[] VibratorFrequencies = new int[16];
    protected float[] VibratorAddiLifeSpans = new float[16];
    public float[] VibratorLifeSpans = new float[16];
    public GameObject[] Motors;
    private RawImage[] MotorImages = new RawImage[16];

    public float[] newSuspension = new float[4];
    public GameObject frontRight;
    public GameObject frontLeft;
    public GameObject backRight;
    public GameObject backLeft;
    private Color maxColor;

    void Start()
    {
        VibratorIntensities = gameObject.GetComponent<VirtualHeadband>().VibratorIntensities;
        VibratorLifeSpans = gameObject.GetComponent<VirtualHeadband>().VibratorLifeSpans;
        VibratorMotionIntensities = gameObject.GetComponent<VirtualHeadband>().VibratorMotionIntensities;
        VibratorAddiIntensities = gameObject.GetComponent<VirtualHeadband>().VibratorAdditionalIntensities;
        VibratorFrequencies = gameObject.GetComponent<VirtualHeadband>().VibratorFrequencies;
        VibratorAddiLifeSpans = gameObject.GetComponent<VirtualHeadband>().VibratorAddiLifeSpans;

        for (int i = 0; i < 16; i++)
        {
            MotorImages[i] = Motors[i].GetComponent<RawImage>();
            Motors[i].GetComponent<RectTransform>().localPosition
                = new Vector2(100.0f * Mathf.Cos(Mathf.PI / 2 - i * Mathf.PI / 8), 100.0f * Mathf.Sin(Mathf.PI / 2 - i * Mathf.PI / 8));
        }
    }

    // Update is called once per frame
    void Update()
    {
        maxColor = Color.Lerp(Color.red, Color.red, ((float)VibratorFrequencies[0]) / 100.0f);
        for (int i = 0; i < 16; i++)
        {
            if ((VibratorIntensities[i] > 0 || VibratorLifeSpans[i] > 0 || VibratorAddiLifeSpans[i] > 0) && (VibratorIntensities[i] + VibratorMotionIntensities[i] + VibratorAddiIntensities[i])> 0 && VibratorIntensities[i] <= 100)
            {
                MotorImages[i].color = Color.Lerp(Color.white, maxColor, ((float)VibratorIntensities[i] + VibratorMotionIntensities[i] / 2 + VibratorAddiIntensities[i] / 2) / 120.0f);
            }
            else
            {
                MotorImages[i].color = Color.white;
            }
        }

        for(int i = 0; i < 4; i++)
        {
            newSuspension[i] = gameObject.GetComponent<ACListener>().suspension[i];
        }

        frontLeft.GetComponent<Image>().fillAmount = newSuspension[3] * 5;
        frontRight.GetComponent<Image>().fillAmount = newSuspension[2] * 5;
        backLeft.GetComponent<Image>().fillAmount = newSuspension[1] * 5;
        backRight.GetComponent<Image>().fillAmount = newSuspension[0] * 5;
    }
}
