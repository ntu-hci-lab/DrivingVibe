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

    private Color maxColor;

    void Start()
    {
        if(GetComponent<VirtualHeadband>() != null)
        {
            HeadbandState = GetComponent<VirtualHeadband>().HeadbandIntensityAfterOffset;
        }
        else if(GetComponent<AbstractHeadband>() != null)
        {
            HeadbandState = GetComponent<AbstractHeadband>().HeadbandIntensityAfterOffset;
        }
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
            MotorImages[i].color = Color.Lerp(Color.white, maxColor, (HeadbandState[i]/100.0f));
        }
    }
}
