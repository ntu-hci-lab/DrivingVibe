using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternBinder : MonoBehaviour
{
    public string UserID = "0";
    public ConditionNum WhichCondition = ConditionNum.B_ControllerHaptic;


    public WifiEncoder encoder;
    public AbstractHeadband abstractHeadband;

    public enum ConditionNum
    {
        B_ControllerHaptic = 1,
        C_DrivingVibe = 2,
    }
    void Awake()
    {
        encoder.calibrationFileName = UserID + ".txt";


        switch (WhichCondition)
        {
            case ConditionNum.B_ControllerHaptic:
                abstractHeadband.isControllerHaptic = true;
                break;
            case ConditionNum.C_DrivingVibe:
                abstractHeadband.isControllerHaptic = false;
                break;
        }
    }
}
