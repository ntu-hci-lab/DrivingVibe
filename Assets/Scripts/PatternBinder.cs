using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternBinder : MonoBehaviour
{
    public string UserID = "0";
    public ConditionNum WhichCondition = ConditionNum.Inertia_based;


    public WifiEncoder encoder;
    public AbstractHeadband abstractHeadband;

    public enum ConditionNum
    {
        Mirroring = 1,
        Inertia_based = 2,
    }
    void Awake()
    {
        encoder.calibrationFileName = UserID + ".txt";


        switch (WhichCondition)
        {
            case ConditionNum.Mirroring:
                abstractHeadband.isMirroring = true;
                break;
            case ConditionNum.Inertia_based:
                abstractHeadband.isMirroring = false;
                break;
        }
    }
}
