using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// used in visualization

public class ClickDetector : MonoBehaviour
{
    public float DetectRange = 1000.0f;
    public float RingRadius = 90.0f;
    public int[] HeadbandState = new int[16];
    public GameObject[] Motors;
    private RawImage[] MotorImages = new RawImage[16];
    private Color maxColor = Color.red;

    private void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            HeadbandState[i] = 0;
            MotorImages[i] = Motors[i].GetComponent<RawImage>();
            Motors[i].GetComponent<RectTransform>().localPosition
                = new Vector2(RingRadius * Mathf.Cos(Mathf.PI / 2 - i * Mathf.PI / 8), RingRadius * Mathf.Sin(Mathf.PI / 2 - i * Mathf.PI / 8));
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, DetectRange))
            {
                if(hit.transform != null)
                {
                    Debug.Log(hit.transform.gameObject.GetComponent<ParameterHolder>()._timeStamp);
                    HeadbandState = hit.transform.gameObject.GetComponent<ParameterHolder>()._SumIntensity;
                    UpdateRingColor();
                }
            }
        }
    }

    private void UpdateRingColor()
    {
        for (int i = 0; i < 16; i++)
        {
            MotorImages[i].color = Color.Lerp(Color.white, maxColor, (HeadbandState[i] / 100.0f));
        }
    }
}
