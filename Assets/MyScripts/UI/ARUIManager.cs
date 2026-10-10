using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARUIManager : MonoBehaviour
{
    [Header("Riferimento Principale")]
    [SerializeField] private ARMainApplication ar_app;

    [Header("Pulsanti UI (Opzionali per Inspector)")]
    [SerializeField] private Button undo_button;
    [SerializeField] private Button delete_button;
    [SerializeField] private Button rotate_y_button;
    [SerializeField] private Button rotate_x_button;
    [SerializeField] private Button clean_all;
    [SerializeField] private Slider slider;

    // Variabili private per tracciare lo stato della scala
    private Vector3 initial_scale_before_drag;
    private GameObject current_slider_target;

    private void Start()
    {
        if (undo_button != null) undo_button.onClick.AddListener(OnUndoButtonPressed);
        if (delete_button != null) delete_button.onClick.AddListener(OnDeleteButtonPressed);
        if (rotate_y_button != null) rotate_y_button.onClick.AddListener(OnRotateYButtonPressed);
        if (rotate_x_button != null) rotate_x_button.onClick.AddListener(OnRotateXButtonPressed);
        if (clean_all != null) clean_all.onClick.AddListener(OnCleanAllButtonPressed);

        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnScaleChange);
            SetupSliderEventTriggers();
        }
    }

    // Configura automaticamente l'intercettazione dell'inizio e fine trascinamento dello Slider
    private void SetupSliderEventTriggers()
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        // Pointer Down (Inizio Drag)
        EventTrigger.Entry entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((data) => { OnSliderPointerDown(); });
        trigger.triggers.Add(entryDown);

        // Pointer Up (Fine Drag)
        EventTrigger.Entry entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((data) => { OnSliderPointerUp(); });
        trigger.triggers.Add(entryUp);
    }

    // Aggiorna la posizione dello Slider grafico leggendo la scala dell'oggetto selezionato
    private void SyncSliderWithTargetScale()
    {
        GameObject target = GetTargetObject();
        if (target != null && slider != null)
        {
            // Usiamo SetValueWithoutNotify per spostare la manopola dello slider 
            // SENZA scatenare nuovamente il callback OnScaleChange
            slider.SetValueWithoutNotify(target.transform.localScale.x);
        }
    }


    private void OnSliderPointerDown()
    {
        current_slider_target = GetTargetObject();
        if (current_slider_target != null)
        {
            // Salva la scala prima dell'inizio delle modifiche
            initial_scale_before_drag = current_slider_target.transform.localScale;
            Debug.Log($"[UI] Start Drag Slider. Scala Iniziale: {initial_scale_before_drag}");
        }
    }

    public void OnScaleChange(float scale)
    {
        GameObject target = GetTargetObject();
        if (target != null)
        {
            // Applica la scala visivamente durante il trascinamento
            target.transform.localScale = Vector3.one * scale;
        }
    }

    private void OnSliderPointerUp()
    {
        if (current_slider_target != null)
        {
            Vector3 final_scale = current_slider_target.transform.localScale;

            // Registra il comando solo se la scala è effettivamente cambiata
            if (initial_scale_before_drag != final_scale)
            {
                ICommand scale_cmd = new ScaleObjectCommand(current_slider_target, initial_scale_before_drag, final_scale);
                ar_app.ExecuteCommand(scale_cmd);
                Debug.Log($"[UI] Registrato comando Scala. Da {initial_scale_before_drag} a {final_scale}");
            }

            current_slider_target = null;
        }
    }

    public void OnUndoButtonPressed()
    {
        if (ar_app != null)
        {
            ar_app.UndoLastCommand();
            Debug.Log("[UI] Eseguito Annulla (Undo)");

            // Sincronizza lo slider grafico con la scala attuale dell'oggetto dopo l'Undo
            SyncSliderWithTargetScale();
        }
    }

    public void OnCleanAllButtonPressed()
    {
        if (ar_app != null && ar_app.Objects_Spawned != null && ar_app.Objects_Spawned.Count > 0)
        {
            ICommand clean_all_cmd = new ClearAllObjectsCommand(ar_app);
            ar_app.ExecuteCommand(clean_all_cmd);
            Debug.Log("[UI] Eseguito Cancella Tutto (Clean All)");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto presente nella scena da cancellare.");
        }
    }

    public void OnDeleteButtonPressed()
    {
        if (ar_app == null) return;

        GameObject target = GetTargetObject();

        if (target != null)
        {
            ICommand deleteCmd = new DeleteObjectCommand(ar_app, target);
            ar_app.ExecuteCommand(deleteCmd);

            ar_app.Selected_Object = null;
            ar_app.ChangeState(new IdleState(), null);
            Debug.Log($"[UI] Oggetto '{target.name}' eliminato via Command");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto selezionato o disponibile da eliminare!");
        }
    }

    public void OnRotateYButtonPressed()
    {
        if (ar_app == null) return;

        GameObject target = GetTargetObject();

        if (target != null)
        {
            ICommand rotateCmd = new RotateObjectCommand(target, new Vector3(0, 45f, 0));
            ar_app.ExecuteCommand(rotateCmd);
            Debug.Log($"[UI] Oggetto '{target.name}' ruotato di 45°");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto selezionato o disponibile da ruotare!");
        }
    }

    public void OnRotateXButtonPressed()
    {
        if (ar_app == null) return;

        GameObject target = GetTargetObject();

        if (target != null)
        {
            ICommand rotateCmd = new RotateObjectCommand(target, new Vector3(45f, 0, 0));
            ar_app.ExecuteCommand(rotateCmd);
            Debug.Log($"[UI] Oggetto '{target.name}' ruotato di 45°");
        }
        else
        {
            Debug.LogWarning("[UI] Nessun oggetto selezionato o disponibile da ruotare!");
        }
    }

    private GameObject GetTargetObject()
    {
        if (ar_app.Selected_Object != null)
        {
            return ar_app.Selected_Object;
        }

        if (ar_app.Objects_Spawned != null && ar_app.Objects_Spawned.Count > 0)
        {
            return ar_app.Objects_Spawned[ar_app.Objects_Spawned.Count - 1];
        }

        return null;
    }
}