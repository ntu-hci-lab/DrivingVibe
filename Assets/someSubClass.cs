using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class someSubClass : Layer
{
    public int b;
    protected override void Start()
    {
        base.Start();
        Debug.Log("This is in someSubClass->Start");
    }

    // Update is called once per frame
    protected override void Update()
    {

        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("This is in someSubClass->Update");
        }
    }

    public override void MyFunc()
    {
        base.MyFunc();
        Debug.Log("This is in someSubClass->MyFunc");
    }
}
