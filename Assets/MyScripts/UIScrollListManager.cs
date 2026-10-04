using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScrollListManager : MonoBehaviour
{
    [Header("Riferimenti Contenitore UI")]
    [SerializeField] private Transform content_parent;
    [SerializeField] private GameObject button_prefab;

    [Header("Riferimenti Script AR")]
    [SerializeField] private LoadPrefabs load_prefabs;
    [SerializeField] private ARMainApplication ar_app;

    private void OnEnable()
    {
        if (ar_app != null)
        {
            ar_app.On_object_to_spawn_changed += HandleObjectToSpawnChanged;
        }
    }

    private void OnDisable()
    {
        if (ar_app != null)
        {
            ar_app.On_object_to_spawn_changed -= HandleObjectToSpawnChanged;
        }
    }

    private void Start()
    {
        GenerateList();
    }

    private void GenerateList()
    {
        if (load_prefabs == null || load_prefabs.Spawnable_Objects == null)
        {
            Debug.LogError("[UIScrollListManager] LoadPrefabs o lista oggetti mancante!");
            return;
        }

        foreach (GameObject prefab in load_prefabs.Spawnable_Objects)
        {
            GameObject newButton = Instantiate(button_prefab, content_parent);

            TMP_Text tmpText = newButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null) tmpText.text = prefab.name;

            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                GameObject currentPrefab = prefab;
                btn.onClick.AddListener(() =>
                {
                    if (ar_app != null)
                    {
                        ar_app.Object_To_Spawn = currentPrefab;
                    }
                });
            }
        }
    }

    private void HandleObjectToSpawnChanged(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            Debug.Log($"[UI Observer] Oggetto da spawnare aggiornato: {newPrefab.name}");
        }
    }
}