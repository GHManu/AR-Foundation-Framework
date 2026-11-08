using UnityEngine;

public class RotateObjectCommand : ICommand
{
    private readonly GameObject target_object;
    private readonly Quaternion previous_rotation;
    private readonly Quaternion new_rotation;

    public RotateObjectCommand(GameObject targetObject, Vector3 rotationAngles)
    {
        this.target_object = targetObject;
        if (targetObject != null)
        {
            this.previous_rotation = targetObject.transform.rotation;
            this.new_rotation = previous_rotation * Quaternion.Euler(rotationAngles);
        }
    }

    public void Execute()
    {
        if (target_object != null)
        {
            target_object.transform.rotation = new_rotation;
        }
    }

    public void Undo()
    {
        if (target_object != null)
        {
            target_object.transform.rotation = previous_rotation;
        }
    }
}