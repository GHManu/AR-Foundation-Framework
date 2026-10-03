using UnityEngine;
using UnityEngine.UI;

public class ARUIManager : MonoBehaviour
{
    [Header("Riferimento Principale")]
    [SerializeField] private ARMainApplication main_app;

    [Header("Pulsanti UI (Opzionali per Inspector)")]
    [SerializeField] private Button undo_button;
    [SerializeField] private Button delete_button;
    [SerializeField] private Button rotate_button;
    [SerializeField] private Button clean_all;

    private void Start()
    {
        if (undo_button != null) undo_button.onClick.AddListener(OnUndoButtonPressed);
        if (delete_button != null) delete_button.onClick.AddListener(OnDeleteButtonPressed);
        if (rotate_button != null) rotate_button.onClick.AddListener(OnRotateButtonPressed);
        if (clean_all != null) clean_all.onClick.AddListener(OnCleanAllButtonPressed);
    }

    public void OnUndoButtonPressed()
    {
        if (main_app != null)
        {
            main_app.UndoLastCommand();
            Debug.Log("[UI] Eseguito Annulla (Undo)");
        }
    }

    public void OnCleanAllButtonPressed()
    {
        if (main_app != null && main_app.Objects_Spawned != null && main_app.Objects_Spawned.Count > 0)
        {
            ICommand clean_all_cmd = new ClearAllObjectsCommand(main_app);
            main_app.ExecuteCommand(clean_all_cmd);
            Debug.Log("[UI] Eseguito Cancella Tutto (Clean All)");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto presente nella scena da cancellare.");
        }
    }

    public void OnDeleteButtonPressed()
    {
        if (main_app == null) return;

        // Recupera l'oggetto selezionato o, se è null, l'ultimo oggetto creato
        GameObject target = GetTargetObject();

        if (target != null)
        {
            ICommand deleteCmd = new DeleteObjectCommand(main_app, target);
            main_app.ExecuteCommand(deleteCmd);

            main_app.ChangeState(new IdleState(), null);
            Debug.Log($"[UI] Oggetto '{target.name}' eliminato via Command");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto selezionato o disponibile da eliminare!");
        }
    }

    public void OnRotateButtonPressed()
    {
        if (main_app == null) return;

        // Recupera l'oggetto selezionato o, se è null, l'ultimo oggetto creato
        GameObject target = GetTargetObject();

        if (target != null)
        {
            ICommand rotateCmd = new RotateObjectCommand(target, new Vector3(0, 45f, 0));
            main_app.ExecuteCommand(rotateCmd);
            Debug.Log($"[UI] Oggetto '{target.name}' ruotato di 45°");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto selezionato o disponibile da ruotare!");
        }
    }

    // Helper per ottenere l'oggetto di target (selezionato o l'ultimo in lista)
    private GameObject GetTargetObject()
    {
        if (main_app.Selected_Object != null)
        {
            return main_app.Selected_Object;
        }

        if (main_app.Objects_Spawned != null && main_app.Objects_Spawned.Count > 0)
        {
            return main_app.Objects_Spawned[main_app.Objects_Spawned.Count - 1];
        }

        return null;
    }
}