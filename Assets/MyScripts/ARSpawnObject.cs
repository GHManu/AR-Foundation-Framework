using System;
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
    private LoadPrefabs load_preafabs;
    public LoadPrefabs Load_Prefabs
    {
        get {  return load_preafabs; }
        set {  load_preafabs = value; }
    }
    private GameObject object_to_spawn;    //tipologia di oggetto selezionato da spawnare
    public GameObject Object_To_Spawn
    {
        get { return object_to_spawn; }
        set {  object_to_spawn = value; }
    }
    [SerializeField] private Button switch_button;  //per cambiare tipologia di oggetto
    public Button Switch_Button
    {
        get { return switch_button; }
        set { switch_button = value; }
    }


    [Header("Configurazione Input")]
    [SerializeField] private InputActionReference input_action; // Es. Primary Touch / Press; InputActionReference è l'azione singola
    public InputActionReference Input_Action
    {
        get { return input_action; }
        set { input_action = value; }
    }
    [SerializeField] private InputAction screen_pos;            // Posizione del Touch/mouse, InputAction
    public InputAction Screen_Pos
    {
        get { return screen_pos; }
        set { screen_pos = value; }
    }
    private bool is_dragging;
    public bool Is_Dragging
    {
        get { return is_dragging; }
        set { is_dragging = value; }
    }
    private List<ARRaycastHit> hit_list;    //mi serve per contenere i punti in cui il mio raycast tocca le superfici
    public List<ARRaycastHit> Hit_List
    {
        get { return hit_list; }
        set { hit_list = value; }
    }
    private List<GameObject> objects_spawned;
    public List<GameObject> Objects_Spawned
    {
        get { return objects_spawned; }
        set { objects_spawned = value; }
    }
    private GameObject selected_object;
    public GameObject Selected_Object
    {
        get { return selected_object; }
        set { selected_object = value; }
    }

    private EventTrigger event_trigger; //per eventi di sistema
    public EventTrigger Event_Trigger
    {
        get { return event_trigger; }
        set { event_trigger = value; }
    }
    private int current_index;
    public int Current_Index
    {
        get { return current_index; }
        set { current_index = value; }
    }
    private bool mouse_on_btn;
    public bool Mouse_On_Btn
    {
        get { return mouse_on_btn; }
        set { mouse_on_btn = value; }
    }

    //Manager
    private ARRaycastManager raycast_manager;
    public ARRaycastManager Raycast_Manager
    {
        get { return raycast_manager; }
        set { raycast_manager = value; }
    }

    private ARPlaneManager plane_manager;
    public ARPlaneManager Plane_Manager
    {
        get { return plane_manager;}
        set { plane_manager = value; }
    }

    //Awake viene eseguito dopo la creazione dell'istanza dello script (cioè dopo la chiamata del costruttore C# e l'allocazione della memoria)
    //quindi anche dopo l'istanza dei prefab e prima di start.
    //Se un GameObject è inattivo durante l'avvio, Awake non viene chiamata finché non viene attivato.
    private void Awake()
    {
        Load_Prefabs = GetComponent<LoadPrefabs>();
        Raycast_Manager = GetComponent<ARRaycastManager>();
        Plane_Manager = GetComponent<ARPlaneManager>();
        Event_Trigger = Switch_Button.GetComponent<EventTrigger>();


        if (Event_Trigger == null){
            Event_Trigger = Switch_Button.gameObject.AddComponent<EventTrigger>();
        }

        // Se la posizione dello schermo non è configurata nell'Inspector, usiamo la posizione di default del touch/pointer
        if (Screen_Pos.bindings.Count == 0)
        {
            Screen_Pos = new InputAction("TouchPos", binding: "<Pointer>/position");
        }
        Screen_Pos.Enable();
    }

    //Chiamato solo quando un GameObject è o diventa attivo. Questo accade quando viene creata un'istanza di MonoBehavior,
    //ad esempio quando viene caricato un livello o viene istanziato un GameObject con il componente script.
    private void OnEnable()
    {
        if (Input_Action != null && Input_Action.action != null)
        {
            Input_Action.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (Input_Action != null && Input_Action.action != null)
        {
            Input_Action.action.Disable();
        }
        Screen_Pos.Disable();
    }

    //Eseguito dopo l'Awake, prima del primo frame
    private void Start()
    {
        Object_To_Spawn = Load_Prefabs.Spawnable_Objects[0];
        Plane_Manager.enabled = true;
        Mouse_On_Btn = false;
        Current_Index = 0;
        Is_Dragging = false;
        Objects_Spawned = new List<GameObject>();
        Hit_List = new List<ARRaycastHit>();

        Switch_Button.onClick.AddListener(SwitchObjByClick);

        // Configura l'evento di ENTRATA nel pulsante (PointerEnter)
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnMouseEnter(); });
        Event_Trigger.triggers.Add(entryEnter);

        // Configura l'evento di USCITA nel pulsante (PointerExit)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnMouseExit(); });
        Event_Trigger.triggers.Add(entryExit);



    }

    private void OnMouseEnter()
    {
        this.Mouse_On_Btn = true;
    }

    private void OnMouseExit()
    {
        this.Mouse_On_Btn = false;
    }

    //Eseguito ad ogni frame
    private void Update()
    {
        if (!Mouse_On_Btn)
        {
            // Lettura sicura della posizione corrente sullo schermo
            Vector2 current_input_pos = Screen_Pos.ReadValue<Vector2>();

            // Fallback per dispositivi mobili se screen_pos non legge correttamente
            if (current_input_pos == Vector2.zero && Touchscreen.current != null)
            {
                current_input_pos = Touchscreen.current.primaryTouch.position.ReadValue();
            }

            // Se non abbiamo un'azione di tocco associata, usiamo un fallback sul Touch/Click generico
            bool is_pressed_this_frame = (Input_Action != null && Input_Action.action != null)
                ? Input_Action.action.WasPressedThisFrame()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

            bool is_pressed = (Input_Action != null && Input_Action.action != null)
                ? Input_Action.action.IsPressed()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);

            bool was_released = (Input_Action != null && Input_Action.action != null)
                ? Input_Action.action.WasReleasedThisFrame()
                : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);

            // INIZIO TOCCO
            if (is_pressed_this_frame)
            {
                Selected_Object = null;
                Is_Dragging = false;

                // Physics Raycast per selezionare oggetti già generati
                Ray ray = Camera.main.ScreenPointToRay(current_input_pos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (Objects_Spawned.Contains(hit.transform.gameObject))
                    {
                        Selected_Object = hit.transform.gameObject;
                        Is_Dragging = true;
                        return;
                    }
                }

                // AR Raycast per selezionare o generare oggetti sul piano AR
                if (Raycast_Manager.Raycast(current_input_pos, Hit_List, TrackableType.PlaneWithinPolygon))
                {
                    Pose hit_pose = Hit_List[0].pose;
                    GameObject nearby_object = GetClosestSpawnedObject(hit_pose.position, 0.25f);

                    if (nearby_object != null)
                    {
                        Selected_Object = nearby_object;
                        Is_Dragging = true;
                        return;
                    }

                    TrySpawnObjectAtPose(hit_pose);
                }
            }

            // TRASCINAMENTO (DRAG)
            if (Is_Dragging && Selected_Object != null && is_pressed)
            {
                if (Raycast_Manager.Raycast(current_input_pos, Hit_List, TrackableType.PlaneWithinPolygon))
                {
                    Pose hit_pose = Hit_List[0].pose;

                    Selected_Object.transform.position = hit_pose.position;

                    //Per fare si che l'oggetto guardi verso di noi
                    Vector3 camera_view = Camera.main.transform.position - Selected_Object.transform.position;
                    camera_view.y = 0f;
                    if (camera_view != Vector3.zero)
                    {
                        Selected_Object.transform.rotation = Quaternion.LookRotation(camera_view);
                    }
                }
            }

            // RILASCIO (DROP)
            if (was_released || !is_pressed)
            {
                Is_Dragging = false;
                Selected_Object = null;
            }
        }
       
    }

    // Serve per prendere l'elemento più vicino, se c'è, invece di spawnarne un altro, impedisce anche di spawnarli attaccati o uno dentro l'altro o tanti
    //nella stessa posizione che non avrebbe senso
    private GameObject GetClosestSpawnedObject(Vector3 hit_position, float max_distance)
    {
        GameObject closest = null;
        float min_dist = max_distance;

        foreach (GameObject obj in Objects_Spawned)
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
        GameObject prefab_to_spawn = ChoosePrefabToSpawn(Load_Prefabs.Spawnable_Objects, Object_To_Spawn);
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

            Objects_Spawned.Add(prefab_spawned);
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
        if (Current_Index < Load_Prefabs.Spawnable_Objects.Count-1)
        {
            Current_Index++;
        }
        else
        {
            Current_Index = 0;
        }
        Object_To_Spawn = Load_Prefabs.Spawnable_Objects[current_index];

    }


}