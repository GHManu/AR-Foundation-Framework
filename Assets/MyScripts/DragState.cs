using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class DragState : IObjectInteractionState
{
    public void EnterState(ARSpawnObject context, GameObject obj)
    {
        context.Selected_Object = obj;
        context.Is_Dragging = true;
    }

    public void UpdateState(ARSpawnObject context, GameObject obj)
    {
        // Se il giocatore rilascia il tocco, rilascia l'oggetto
        if (context.WasReleasedThisFrame() || !context.IsPressed())
        {
            context.ChangeState(new DropState(), context.Selected_Object);
            return;
        }

        Vector2 inputPos = context.GetInputPosition();

        if (context.Raycast_Manager.Raycast(inputPos, context.Hit_List, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = context.Hit_List[0].pose;
            context.Selected_Object.transform.position = hitPose.position;

            // Orientamento verso la fotocamera
            Vector3 cameraView = context.Main_Camera.transform.position - context.Selected_Object.transform.position;
            cameraView.y = 0f;
            if (cameraView != Vector3.zero)
            {
                context.Selected_Object.transform.rotation = Quaternion.LookRotation(cameraView);
            }
        }
    }

    public void ExitState(ARSpawnObject context, GameObject obj) { }
}