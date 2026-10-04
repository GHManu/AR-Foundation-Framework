using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class DragState : IObjectInteractionState
{
    private Vector3 initial_position;
    private Quaternion initial_rotation;

    public void EnterState(ARMainApplication context, GameObject obj)
    {
        context.Selected_Object = obj;
        context.Is_Dragging = true;

        if (obj != null)
        {
            initial_position = obj.transform.position;
            initial_rotation = obj.transform.rotation;
        }
        // Attiva lo stato "Drag" visivo per i punti di tracciamento
        context.Custom_Point_Cloud_Manager?.SetPointCloudActiveState(true);
    }

    public void UpdateState(ARMainApplication context, GameObject obj)
    {
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

            //Vector3 cameraView = context.Main_Camera.transform.position - context.Selected_Object.transform.position;
            //cameraView.y = 0f;
            //if (cameraView != Vector3.zero)
            //{
            //    context.Selected_Object.transform.rotation = Quaternion.LookRotation(cameraView);
            //}

            // Evidenzia il piano su cui si sta trascinando l'oggetto
            ARRaycastHit hit = context.Hit_List[0];
            ARPlane plane = context.Plane_Manager.GetPlane(hit.trackableId);
            if (plane != null)
            {
                context.Custom_Plane_Manager?.HighlightPlane(plane);
            }
        }
    }

    public void ExitState(ARMainApplication context, GameObject obj)
    {
        // Pulisce gli evidenziatori visivi
        context.Custom_Plane_Manager?.ClearHighlight();
        context.Custom_Point_Cloud_Manager?.SetPointCloudActiveState(false);

        // Se l'oggetto si è effettivamente mosso, registra il comando per l'Undo
        if (obj != null && (initial_position != obj.transform.position || initial_rotation != obj.transform.rotation))
        {
            ICommand move_cmd = new MoveObjectCommand(
                obj,
                initial_position,
                initial_rotation,
                obj.transform.position,
                obj.transform.rotation
            );

            context.ExecuteCommand(move_cmd);
        }
    }
}