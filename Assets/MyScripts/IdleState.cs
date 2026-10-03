using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IObjectInteractionState
{
    public void EnterState(ARMainApplication context, GameObject obj)
    {
        context.Selected_Object = null;
        context.Is_Dragging = false;
    }

    public void UpdateState(ARMainApplication context, GameObject obj)
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
                Pose hit_pose = context.Hit_List[0].pose;
                GameObject nearby_obj = GetClosestSpawnedObject(hit_pose.position, 0.25f, context.Objects_Spawned);

                if (nearby_obj != null)
                {
                    context.ChangeState(new DragState(), nearby_obj);
                }
                else
                {
                    context.ChangeState(new SpawnState(), null);
                }
            }
        }
    }

    public void ExitState(ARMainApplication context, GameObject obj) { }

    private GameObject GetClosestSpawnedObject(Vector3 hit_position, float max_distance, List<GameObject> objects_spawned)
    {
        GameObject closest = null;
        float min_dist = max_distance;

        foreach (GameObject obj in objects_spawned)
        {
            if (obj == null) continue;
            float dist = Vector3.Distance(obj.transform.position, hit_position);
            if (dist < min_dist)
            {
                min_dist = dist;
                closest = obj;
            }
        }
        return closest;
    }

}