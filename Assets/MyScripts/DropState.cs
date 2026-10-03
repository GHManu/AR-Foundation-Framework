using UnityEngine;

public class DropState : IObjectInteractionState
{
    public void EnterState(ARSpawnObject context, GameObject obj)
    {
        context.Is_Dragging = false;
        context.Selected_Object = null;
    }

    public void UpdateState(ARSpawnObject context, GameObject obj)
    {
        // Torna immediatamente a Idle
        context.ChangeState(new IdleState(), null);
    }

    public void ExitState(ARSpawnObject context, GameObject obj) { }
}