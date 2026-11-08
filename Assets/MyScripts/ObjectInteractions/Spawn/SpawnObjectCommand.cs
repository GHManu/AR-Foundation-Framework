using UnityEngine;

public class SpawnObjectCommand : ICommand
{
    private readonly ASpawnObjectFactory factory;
    private readonly SpawnContext spawn_context;
    private readonly ARMainApplication ar_app;
    private GameObject spawned_object;

    public SpawnObjectCommand(ASpawnObjectFactory factory, SpawnContext spawn_context, ARMainApplication ar_app)
    {
        this.factory = factory;
        this.spawn_context = spawn_context;
        this.ar_app = ar_app;
    }

    public void Execute()
    {
        spawned_object = factory.SpawnObject(spawn_context);
        if (spawned_object != null && ar_app != null)
        {
            ar_app.NotifyObjectSpawned(spawned_object);
        }
    }

    public void Undo()
    {
        if (spawned_object != null)
        {
            if (ar_app != null && ar_app.Objects_Spawned.Contains(spawned_object))
            {
                ar_app.Objects_Spawned.Remove(spawned_object);
            }
            Object.Destroy(spawned_object);
        }
    }
}