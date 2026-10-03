using UnityEngine;

public class IdleState : IObjectInteractionState
{
    public void EnterState(ARSpawnObject context, GameObject obj)
    {
        context.Selected_Object = null;
        context.Is_Dragging = false;
    }

    public void UpdateState(ARSpawnObject context, GameObject obj)
    {
        Vector2 inputPos = context.GetInputPosition();

        if (context.IsPressedThisFrame())
        {
            // Prova a selezionare un oggetto tramite Raycast fisico
            Ray ray = context.Main_Camera.ScreenPointToRay(inputPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (context.Objects_Spawned.Contains(hit.transform.gameObject))
                {
                    context.ChangeState(new DragState(), hit.transform.gameObject);
                    return;
                }
            }

            // Se tocca il piano AR, seleziona l'oggetto più vicino o spawna
            if (context.Raycast_Manager.Raycast(inputPos, context.Hit_List, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = context.Hit_List[0].pose;
                GameObject nearbyObj = context.GetClosestSpawnedObject(hitPose.position, 0.25f);

                if (nearbyObj != null)
                {
                    context.ChangeState(new DragState(), nearbyObj);
                }
                else
                {
                    context.ChangeState(new SpawnState(), null);
                }
            }
        }
    }

    public void ExitState(ARSpawnObject context, GameObject obj) { }
}