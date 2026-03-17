using UnityEditor;
using PlotBranching;

[System.Serializable]
public class KarmaChange : Consequence
{
    public int amount;

    public override void Apply(PlotManager context)
    {
        context.ChangeKarma(amount);
    }
}

public class ChangeWorldState : Consequence
{
    public WorldStateType worldState;

    public override void Apply(PlotManager context)
    {
        context.ChangeWorldState(worldState);
    }
}

public class OpenPath : Consequence
{
    public string pathID;
    public override void Apply(PlotManager context)
    {
        if (!string.IsNullOrEmpty(pathID))
        {
            if (!context.plotState.openedPathIDs.Contains(pathID))
            {
                context.plotState.openedPathIDs.Add(pathID);
            }
            context.onPathOpened?.Invoke(pathID); 
        }
    }
    
}