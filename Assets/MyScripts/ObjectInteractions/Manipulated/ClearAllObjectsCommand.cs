using System.Collections.Generic;
using UnityEngine;

public class ClearAllObjectsCommand : ICommand
{
    private readonly ARMainApplication context;
    private readonly List<GameObject> cleared_objects;

    public ClearAllObjectsCommand(ARMainApplication context)
    {
        this.context = context;
        // Salva una copia della lista attuale per consentire l'Undo
        this.cleared_objects = new List<GameObject>(context.Objects_Spawned);
    }

    public void Execute()
    {
        foreach (GameObject obj in cleared_objects)
        {
            if (obj != null) obj.SetActive(false);
        }
        context.Objects_Spawned.Clear();
    }

    public void Undo()
    {
        foreach (GameObject obj in cleared_objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                context.Objects_Spawned.Add(obj);
            }
        }
    }
}