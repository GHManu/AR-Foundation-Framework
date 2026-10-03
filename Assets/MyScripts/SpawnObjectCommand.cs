using UnityEngine;

public class SpawnObjectCommand : ICommand
{
    private readonly ASpawnObjectFactory factory;
    private readonly SpawnContext spawn_context;
    private readonly ARMainApplication main_app;
    private GameObject spawned_object;

    public SpawnObjectCommand(ASpawnObjectFactory factory, SpawnContext spawn_context, ARMainApplication main_app)
    {
        this.factory = factory;
        this.spawn_context = spawn_context;
        this.main_app = main_app;
    }

    public void Execute()
    {
        spawned_object = factory.SpawnObject(spawn_context);
        if (spawned_object != null && main_app != null)
        {
            main_app.NotifyObjectSpawned(spawned_object);
        }
    }

    public void Undo()
    {
        if (spawned_object != null)
        {
            if (main_app != null && main_app.Objects_Spawned.Contains(spawned_object))
            {
                main_app.Objects_Spawned.Remove(spawned_object);
            }
            Object.Destroy(spawned_object);
        }
    }
}