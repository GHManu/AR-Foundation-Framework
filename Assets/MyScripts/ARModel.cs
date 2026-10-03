using System;
using System.Collections.Generic;
using UnityEngine;

public class ARModel
{
    // Eventi Observer
    public event Action<GameObject> OnObjectSpawned;
    public event Action<GameObject> OnObjectSelected;
    public event Action<bool> OnDraggingStateChanged;

    public List<GameObject> Spawned_Objects { get; private set; } = new List<GameObject>();

    private GameObject selected_object;
    public GameObject Selected_Object
    {
        get => selected_object;
        set
        {
            selected_object = value;
            OnObjectSelected?.Invoke(selected_object);
        }
    }

    private bool isDragging;
    public bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
            OnDraggingStateChanged?.Invoke(isDragging);
        }
    }

    public void AddObject(GameObject obj)
    {
        Spawned_Objects.Add(obj);
        OnObjectSpawned?.Invoke(obj);
    }
}