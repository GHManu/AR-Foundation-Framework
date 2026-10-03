using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class SpawnState : IObjectInteractionState
{
    public void EnterState(ARSpawnObject context, GameObject obj) { }

    public void UpdateState(ARSpawnObject context, GameObject obj)
    {
        Vector2 inputPos = context.GetInputPosition();

        if (context.Raycast_Manager.Raycast(inputPos, context.Hit_List, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = context.Hit_List[0].pose;
            GameObject spawnedObj = context.TrySpawnObjectAtPose(hitPose);

            if (spawnedObj != null)
            {
                // Inizia subito il dragging del nuovo oggetto
                context.ChangeState(new DragState(), spawnedObj);
                return;
            }
        }

        // Se non spetta/fallisce, torna in Idle
        context.ChangeState(new IdleState(), null);
    }

    public void ExitState(ARSpawnObject context, GameObject obj) { }
}