using UnityEngine;
using PlotBranching;

public class Door : MonoBehaviour
{
    public Consequence so;

    private void OnEnable()
    {
        so.objectToRemove = gameObject;
    }
}
