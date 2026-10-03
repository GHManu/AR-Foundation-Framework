using UnityEngine;

public class DeleteObjectCommand : ICommand
{
    private readonly ARMainApplication context;
    private readonly GameObject target_object;
    private readonly Vector3 position;
    private readonly Quaternion rotation;

    public DeleteObjectCommand(ARMainApplication context, GameObject targetObject)
    {
        this.context = context;
        this.target_object = targetObject;
        if (targetObject != null)
        {
            this.position = targetObject.transform.position;
            this.rotation = targetObject.transform.rotation;
        }
    }

    public void Execute()
    {
        if (target_object == null) return;

        if (context.Objects_Spawned.Contains(target_object))
        {
            context.Objects_Spawned.Remove(target_object);
        }

        target_object.SetActive(false); // Disattiviamo invece di distruggere per rendere immediato l'Undo
        Object.Destroy(target_object);
    }

    public void Undo()
    {
        if (target_object == null) return;

        target_object.SetActive(true);
        if (!context.Objects_Spawned.Contains(target_object))
        {
            context.Objects_Spawned.Add(target_object);
        }
    }
}