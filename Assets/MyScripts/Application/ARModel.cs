using System;
using System.Collections.Generic;
using UnityEngine;

public class ARModel
{
    // Eventi Observer, obbligatoriamente i nomi devono iniziare con la lettera maiuscola
    public event Action<GameObject> On_object_spawned;
    public event Action<GameObject> On_object_selected;
    public event Action<bool> On_dragging_state_changed;

    public List<GameObject> Spawned_Objects { get; private set; } = new List<GameObject>();

    private GameObject selected_object;
    public GameObject Selected_Object
    {
        get => selected_object;
        set
        {
            selected_object = value;
            On_object_selected?.Invoke(selected_object);
        }
    }

    private bool isDragging;
    public bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
            On_dragging_state_changed?.Invoke(isDragging);
        }
    }

    public void AddObject(GameObject obj)
    {
        Spawned_Objects.Add(obj);
        On_object_spawned?.Invoke(obj);
    }
}