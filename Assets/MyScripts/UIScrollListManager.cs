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

            // Aggancia il Listener di selezione
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                GameObject currentPrefab = prefab;
                btn.onClick.AddListener(() =>
                {
                    if (ar_app != null)
                    {
                        ar_app.Object_To_Spawn = currentPrefab;
                        Debug.Log($"[UI] Selezionato oggetto AR: {currentPrefab.name}");
                    }
                });
            }
        }
    }
}