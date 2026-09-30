using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;  
using UnityEngine.UI;   
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARSpawnObject : MonoBehaviour
{
    [Header("Configurazione Prefab")]   //lo scrive nell'inspector e SerializeField lo rende inizializzabile dall'inspector
    [SerializeField] private List<GameObject> spawnable_objects;    //tipi di oggetti totali da poter spawnare
    [SerializeField] private GameObject object_to_spawn;    //tipologia di oggetto selezionato da spawnare
    [SerializeField] private Button switch_button;  //per cambiare tipologia di oggetto


    private EventTrigger event_trigger; //per eventi di sistema
    private int current_index;
    private bool mouse_on_btn;

    [Header("Input System Settings")]
    [SerializeField] private InputActionReference input_action; // Es. Primary Touch / Press; InputActionReference è l'azione singola
    [SerializeField] private InputAction screen_pos;            // Posizione del Touch/mouse, InputAction

    private bool isDragging;
    private List<ARRaycastHit> hit_list;    //mi serve per contenere i punti in cui il mio raycast tocca le superfici
    private List<GameObject> objects_spawned;
    private GameObject selected_object;

    //Manager
    private ARRaycastManager raycast_manager;
    private ARPlaneManager plane_manager;

    //Awake viene eseguito dopo la creazione dell'istanza dello script (cioè dopo la chiamata del costruttore C# e l'allocazione della memoria)
    //quindi anche dopo l'istanza dei prefab e prima di start.
    //Se un GameObject è inattivo durante l'avvio, Awake non viene chiamata finché non viene attivato.
    private void Awake()
    {
        raycast_manager = GetComponent<ARRaycastManager>();
        plane_manager = GetComponent<ARPlaneManager>();

        event_trigger = switch_button.gameObject.GetComponent<EventTrigger>();


        if (event_trigger == null){
            event_trigger = switch_button.gameObject.AddComponent<EventTrigger>();
        }

        // Se la posizione dello schermo non è configurata nell'Inspector, usiamo la posizione di default del touch/pointer
        if (screen_pos.bindings.Count == 0)
        {
            screen_pos = new InputAction("TouchPos", binding: "<Pointer>/position");
        }
        screen_pos.Enable();
    }

    //Chiamato solo quando un GameObject è o diventa attivo. Questo accade quando viene creata un'istanza di MonoBehavior,
    //ad esempio quando viene caricato un livello o viene istanziato un GameObject con il componente script.
    private void OnEnable()
    {
        if (input_action != null && input_action.action != null)
        {
            input_action.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (input_action != null && input_action.action != null)
        {
            input_action.action.Disable();
        }
        screen_pos.Disable();
    }

    //Eseguito dopo l'Awake, prima del primo frame
    private void Start()
    {
        plane_manager.enabled = true;
        mouse_on_btn = false;
        current_index = 0;
        isDragging = false;
        objects_spawned = new List<GameObject>();
        hit_list = new List<ARRaycastHit>();

        switch_button.onClick.AddListener(SwitchObjByClick);

        // Configura l'evento di ENTRATA nel pulsante (PointerEnter)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnMouseEnter(); });
        event_trigger.triggers.Add(entryEnter);

        // Configura l'evento di USCITA nel pulsante (PointerExit)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnMouseExit(); });
        event_trigger.triggers.Add(entryExit);



    }

    private void OnMouseEnter()
    {
        this.mouse_on_btn = true;
    }

    private void OnMouseExit()
    {
        this.mouse_on_btn = false;
    }

    //Eseguito ad ogni frame
    private void Update()
    {
        if (!mouse_on_btn)
        {
            // Lettura sicura della posizione corrente sullo schermo
            Vector2 current_input_pos = screen_pos.ReadValue<Vector2>();

            // Fallback per dispositivi mobili se screen_pos non legge correttamente
            if (current_input_pos == Vector2.zero && Touchscreen.current != null)
            {
                current_input_pos = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            // Se non abbiamo un'azione di tocco associata, usiamo un fallback sul Touch/Click generico
            bool isPressedThisFrame = (input_action != null && input_action.action != null)
                ? input_action.action.WasPressedThisFrame()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

            bool isPressed = (input_action != null && input_action.action != null)
                ? input_action.action.IsPressed()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);

            bool wasReleased = (input_action != null && input_action.action != null)
                ? input_action.action.WasReleasedThisFrame()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);

            // INIZIO TOCCO
            if (isPressedThisFrame)
            {
                selected_object = null;
                isDragging = false;

                // Physics Raycast per selezionare oggetti già generati
                Ray ray = Camera.main.ScreenPointToRay(current_input_pos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (objects_spawned.Contains(hit.transform.gameObject))
                    {
                        selected_object = hit.transform.gameObject;
                        isDragging = true;
                        return;
                    }
                }

                // AR Raycast per selezionare o generare oggetti sul piano AR
                if (raycast_manager.Raycast(current_input_pos, hit_list, TrackableType.PlaneWithinPolygon))
                {
                    Pose hit_pose = hit_list[0].pose;
                    GameObject nearby_object = GetClosestSpawnedObject(hit_pose.position, 0.25f);

                    if (nearby_object != null)
                    {
                        selected_object = nearby_object;
                        isDragging = true;
                        return;
                    }

                    TrySpawnObjectAtPose(hit_pose);
                }
            }

            // TRASCINAMENTO (DRAG)
            if (isDragging && selected_object != null && isPressed)
            {
                if (raycast_manager.Raycast(current_input_pos, hit_list, TrackableType.PlaneWithinPolygon))
                {
                    Pose hit_pose = hit_list[0].pose;

                    selected_object.transform.position = hit_pose.position;

                    Vector3 camera_view = Camera.main.transform.position - selected_object.transform.position;
                    //camera_view.y = 0f;
                    if (camera_view != Vector3.zero)
                    {
                        selected_object.transform.rotation = Quaternion.LookRotation(camera_view);
                    }
                }
            }

            // RILASCIO (DROP)
            if (wasReleased || !isPressed)
            {
                isDragging = false;
                selected_object = null;
            }
        }
       
    }

    // Serve per prendere l'elemento più vicino, se c'è, invece di spawnarne un altro, impedisce anche di spawnarli attaccati o uno dentro l'altro o tanti
    //nella stessa posizione che non avrebbe senso
    private GameObject GetClosestSpawnedObject(Vector3 hit_position, float max_distance)
    {
        GameObject closest = null;
        float min_dist = max_distance;

        foreach (GameObject obj in objects_spawned)
        {
            if (obj == null) continue;
            float dist = Vector3.Distance(obj.transform.position, hit_position);
            if (dist < min_dist)
            {
                min_dist = dist;
                closest = obj;
            }
        }

        return closest;
    }

    // Serve per lo spawn del prefab
    private void TrySpawnObjectAtPose(Pose hit_pose)
    {
        GameObject prefab_to_spawn = ChoosePrefabToSpawn(spawnable_objects, object_to_spawn);
        if (prefab_to_spawn != null)
        {
            GameObject prefab_spawned = Instantiate(prefab_to_spawn, hit_pose.position, hit_pose.rotation);

            Vector3 camera_view = Camera.main.transform.position - prefab_spawned.transform.position;
            camera_view.y = 0f;
            prefab_spawned.transform.rotation = Quaternion.LookRotation(camera_view);

            // Aggiungiamo un Collider se l'oggetto non ne possiede uno (per permettere il Physics.Raycast)
            if (prefab_spawned.GetComponent<Collider>() == null)
            {
                prefab_spawned.AddComponent<BoxCollider>();
            }

            objects_spawned.Add(prefab_spawned);
        }
    }

    //Scelgo il prefab da spawnare
    private GameObject ChoosePrefabToSpawn(List<GameObject> list, GameObject to_find)
    {
        if (to_find == null) return null;

        if (list != null)
        {
            foreach (GameObject obj in list)
            {
                if (obj != null && obj.name.Contains(to_find.name))
                {
                    return obj;
                }
            }
        }

        return to_find;
    }

    // Serve a cambiare oggetto a runtime
    private void SwitchObjByClick(){
        if (current_index < spawnable_objects.Count - 1)
        {
            object_to_spawn = spawnable_objects[current_index];
            current_index++;
        }
        else
        {
            current_index = 0;
        }
        
    }


}