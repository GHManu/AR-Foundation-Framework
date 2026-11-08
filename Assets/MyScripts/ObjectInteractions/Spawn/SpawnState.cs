using UnityEngine;

public class SpawnState : IObjectInteractionState
{
    public void EnterState(ARMainApplication context, GameObject obj)
    {
        if (context.Object_Factory != null && context.Hit_List.Count > 0)
        {
            Pose hit_pose = context.Hit_List[0].pose;
            SpawnContext spawn_context = new SpawnContext(
                hit_pose,
                context.Object_To_Spawn,
                context.Objects_Spawned,
                context.Load_Prefabs.Spawnable_Objects,
                context.Main_Camera
            );

            // Invece di chiamare direttamente lo Spawn, incapsuliamo nel Command
            ICommand spawn_cmd = new SpawnObjectCommand(context.Object_Factory, spawn_context, context);
            context.ExecuteCommand(spawn_cmd);
        }

        context.ChangeState(new IdleState(), null);
    }

    public void UpdateState(ARMainApplication context, GameObject obj) { }
    public void ExitState(ARMainApplication context, GameObject obj) { }
}