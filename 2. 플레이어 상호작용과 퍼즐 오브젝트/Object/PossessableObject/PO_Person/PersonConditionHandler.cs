using UnityEngine;

[System.Serializable]
public class QTESettings
{
    public float rotationSpeed;
    public float timeLimit;
    public int successZoneCount;
    public float minZoneSize;
    public float maxZoneSize;
}

public abstract class PersonConditionHandler
{
    public abstract QTESettings GetQTESettings();
}

// 활력
public class VitalConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 270,
        timeLimit = 4f,
        successZoneCount = 4,
        minZoneSize = 20f,
        maxZoneSize = 30f
    };
}

// 보통
public class NormalConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 180f,
        timeLimit = 4f,
        successZoneCount = 3,
        minZoneSize = 30f,
        maxZoneSize = 40f
    };
}

// 피곤
public class TiredConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 180f,
        timeLimit = 4f,
        successZoneCount = 2,
        minZoneSize = 30f,
        maxZoneSize = 50f
    };
}

// public class PersonConditionRouter
// {
//     public static void Apply(PersonCondition condition, QTEUI3 qteUI)
//     {
//         PersonConditionHandler handler = condition switch
//         {
//             PersonCondition.Vital => new VitalConditionHandler(),
//             PersonCondition.Normal => new NormalConditionHandler(),
//             PersonCondition.Tired => new TiredConditionHandler(),
//             _ => null
//         };

//         if (handler != null)
//         {
//             var settings = handler.GetQTESettings();
//             qteUI.ApplySettings(settings);
//         }
//     }
// }
