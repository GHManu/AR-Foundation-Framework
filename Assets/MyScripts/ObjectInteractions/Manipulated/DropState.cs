using UnityEngine;

public class DropState : IObjectInteractionState
{
    public void EnterState(ARMainApplication context, GameObject obj)
    {
        context.Is_Dragging = false;
        //context.Selected_Object = null;
    }

    public void UpdateState(ARMainApplication context, GameObject obj)
    {
        // Torna immediatamente a Idle
        context.ChangeState(new IdleState(), obj);
    }

    public void ExitState(ARMainApplication context, GameObject obj) { }
}