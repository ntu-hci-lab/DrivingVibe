using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCalibration : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            transform.Translate(0, 0.002f, 0);
        }
        if (Input.GetKey(KeyCode.T))
        {
            transform.Translate(0, -0.002f, 0);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            transform.Translate(0, 0, 0.002f);
        }
        if (Input.GetKey(KeyCode.U))
        {
            transform.Translate(0, 0, -0.002f);
        }
        if (Input.GetKey(KeyCode.I))
        {
            transform.Translate(0.002f, 0, 0);
        }
        if (Input.GetKey(KeyCode.O))
        {
            transform.Translate(-0.002f, 0, 0);
        }

        //Debug.Log(this.transform.position);
    }
}
