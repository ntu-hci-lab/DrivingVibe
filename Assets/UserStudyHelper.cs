using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyHelper : MonoBehaviour
{
    public string UserID = "0";
    public TaskNum WhichTask = TaskNum.Task1_Acc;
    public ConditionNum WhichCondition = ConditionNum.B_ControllerHaptic;

    public WifiEncoder encoder;
    public VirtualHeadband virtualHeadband;

    public enum TaskNum
    {
        Task1_Acc = 1,
        Task2_Turn = 2,
        Task3_RoadShake = 3,
        Task4_FreeDriving = 4,
    }
    public enum ConditionNum
    {
        B_ControllerHaptic = 1,
        C_DrivingVibe = 2,
    }

    void Awake()
    {
        encoder.calibrationFileName = UserID + ".txt";
        switch (WhichTask)
        {
            case TaskNum.Task1_Acc:
                virtualHeadband.isPassive = true;
                virtualHeadband.profileFilePath = @"./Profiles\profile_task1\";
                break;
            case TaskNum.Task2_Turn:
                virtualHeadband.isPassive = true;
                virtualHeadband.profileFilePath = @"./Profiles\profile_task2\";
                break;
            case TaskNum.Task3_RoadShake:
                virtualHeadband.isPassive = true;
                virtualHeadband.profileFilePath = @"./Profiles\profile_task3\";
                break;
            case TaskNum.Task4_FreeDriving:
                virtualHeadband.isPassive = false;
                break;
        }

        switch (WhichCondition)
        {
            case ConditionNum.B_ControllerHaptic:
                virtualHeadband.isControllerHaptic = true;
                break;
            case ConditionNum.C_DrivingVibe:
                virtualHeadband.isControllerHaptic = false;
                break;
        }
    }

}
