using NUnit.Framework;
using PlotBranching;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData", order = 1)]
 public class PlayerData : ScriptableObject
{
    public ConditionType conditionType;
}
