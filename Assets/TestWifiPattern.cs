using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWifiPattern : MonoBehaviour
{
    private WifiVirtualHeadband virtualHeadband;
    public int[] VibratorIntensities;
    void Start()
    {
        virtualHeadband = gameObject.GetComponent<WifiVirtualHeadband>();
        VibratorIntensities = virtualHeadband.VibratorIntensities;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StartCoroutine(testPattern());
            anotherPattern();
        }
    }

    private void anotherPattern()
    {
        for (int i = 0; i < 16; i++)
        {
            VibratorIntensities[i] = 40 - VibratorIntensities[i];
        }
    }

    private IEnumerator testPattern()
    {
        for(int i = 0; i < 16; i++)
        {
            VibratorIntensities[i] = 40;
            yield return new WaitForSeconds(0.2f);
        }
        for (int i = 0; i < 16; i++)
        {
            VibratorIntensities[i] = 0;
            yield return new WaitForSeconds(0.2f);
        }
        yield break;
    }
}
