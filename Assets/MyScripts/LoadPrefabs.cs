using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class LoadPrefabs : MonoBehaviour
{
    [Header("Lista dei Prefab Caricati")]
    private List<GameObject> spawnable_objects = new List<GameObject>(); //visibile anche dopo la build
    public List<GameObject> Spawnable_Objects
    {
        get { return spawnable_objects; }
        set { spawnable_objects = value; }
    }
    void Start()
    {
        LoadAllPrefabs();
    }

    // Questo metodo è visibile e usabile SOLO nell'Editor prima della build
    [ContextMenu("Popola Lista Prefab")]
    private void LoadAllPrefabs()
    {
#if UNITY_EDITOR
        Spawnable_Objects.Clear();

            // Cerca tutti i prefab nella cartella Assets/Prefabs
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/ObjectSpawned" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    Spawnable_Objects.Add(prefab);
                }
            }

            // Segnala a Unity che l'oggetto è cambiato così da salvare la scena/componente
            EditorUtility.SetDirty(this);
            Debug.Log($"[Editor] Popolata la lista con {Spawnable_Objects.Count} prefab.");
    #endif
        }
}
