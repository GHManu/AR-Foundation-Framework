using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class SpawnState : IObjectInteractionState
{
    public void EnterState(ARMainApplication context, GameObject obj) { }

    public void UpdateState(ARMainApplication context, GameObject obj)
    {
        Vector2 inputPos = context.GetInputPosition();

        if (context.Raycast_Manager.Raycast(inputPos, context.Hit_List, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = context.Hit_List[0].pose;

            SpawnContext spawn_context = new SpawnContext(hitPose, context.Object_To_Spawn,
                context.Objects_Spawned, context.Load_Prefabs.Spawnable_Objects, context.Main_Camera);

            GameObject spawnedObj = context.Object_Factory.SpawnObject(spawn_context);

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

    public void ExitState(ARMainApplication context, GameObject obj) { }
}