using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyHelper : MonoBehaviour
{
    public string UserID = "0";
    public bool isStaticCondition = false;

    public WifiEncoder virtualHeadband;
    void Awake()
    {
        virtualHeadband.isStaticCondition = isStaticCondition;
        virtualHeadband.fileName = UserID + ".txt";
    }

}
