using System.Collections.Generic;
using UnityEngine;

public class LoadPrefabs : MonoBehaviour
{
    private List<GameObject> spawnable_objects = new List<GameObject>();

    public List<GameObject> Spawnable_Objects
    {
        get { return spawnable_objects; }
        set { spawnable_objects = value; }
    }

    private void Awake() // Uso Awake invece di Start per garantire che sia pronto prima degli altri script
    {
        LoadAllPrefabs();
    }

    private void LoadAllPrefabs()
    {
        spawnable_objects.Clear();

        // Carica tutti i GameObject presenti in Assets/Resources/Prefabs/ObjectSpawned
        GameObject[] loadedObjects = Resources.LoadAll<GameObject>("Prefabs/ObjectSpawned");

        spawnable_objects.AddRange(loadedObjects);

        Debug.Log($"[Runtime] Caricati {spawnable_objects.Count} prefab dalla cartella Resources.");
    }
}