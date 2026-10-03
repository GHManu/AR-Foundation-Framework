using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARSpawnObject : MonoBehaviour
{
    [Header("Configurazione Prefab")]
    private LoadPrefabs load_prefabs;
    public LoadPrefabs Load_Prefabs
    {
        get => load_prefabs;    //è uguale a fare get{ return load_prefabs;}
        set => load_prefabs = value;
    }

    private GameObject object_to_spawn;
    public GameObject Object_To_Spawn
    {
        get => object_to_spawn;
        set => object_to_spawn = value;
    }

    [SerializeField] private Button switch_button;
    public Button Switch_Button
    {
        get => switch_button;
        set => switch_button = value;
    }

    [Header("Configurazione Input")]
    [SerializeField] private InputActionReference input_action;
    public InputActionReference Input_Action
    {
        get => input_action;
        set => input_action = value;
    }

    [SerializeField] private InputAction screen_pos;
    public InputAction Screen_Pos
    {
        get => screen_pos;
        set => screen_pos = value;
    }

    public bool Is_Dragging { get; set; }
    public List<ARRaycastHit> Hit_List { get; set; }
    public List<GameObject> Objects_Spawned { get; set; }
    public GameObject Selected_Object { get; set; }
    public EventTrigger Event_Trigger { get; set; }
    public int Current_Index { get; set; }
    public bool Mouse_On_Btn { get; set; }

    public ARRaycastManager Raycast_Manager { get; set; }
    public ARPlaneManager Plane_Manager { get; set; }
    public Camera Main_Camera { get; set; }

    private IObjectInteractionState current_state;

    private void Awake()
    {
        Load_Prefabs = GetComponent<LoadPrefabs>();
        Raycast_Manager = GetComponent<ARRaycastManager>();
        Plane_Manager = GetComponent<ARPlaneManager>();
        Event_Trigger = Switch_Button.GetComponent<EventTrigger>();
        Main_Camera = Camera.main;

        if (Event_Trigger == null)
        {
            Event_Trigger = Switch_Button.gameObject.AddComponent<EventTrigger>();
        }

        if (Screen_Pos.bindings.Count == 0)
        {
            Screen_Pos = new InputAction("TouchPos", binding: "<Pointer>/position");
        }
        Screen_Pos.Enable();
    }

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

        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => { Mouse_On_Btn = true; });
        Event_Trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => { Mouse_On_Btn = false; });
        Event_Trigger.triggers.Add(entryExit);

        // Imposta lo stato iniziale
        ChangeState(new IdleState(), null);
    }

    private void Update()
    {
        if (!Mouse_On_Btn && current_state != null)
        {
            current_state.UpdateState(this, Selected_Object);
        }
    }
    public void ChangeState(IObjectInteractionState newState, GameObject targetObj)
    {
        current_state?.ExitState(this, targetObj);
        current_state = newState;
        current_state?.EnterState(this, targetObj);
    }

    // HELPER INPUT
    public Vector2 GetInputPosition()
    {
        Vector2 pos = Screen_Pos.ReadValue<Vector2>();
        if (pos == Vector2.zero && Touchscreen.current != null)
        {
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return pos;
    }

    public bool IsPressedThisFrame()
    {
        return (Input_Action != null && Input_Action.action != null)
            ? Input_Action.action.WasPressedThisFrame()
            : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
    }

    public bool IsPressed()
    {
        return (Input_Action != null && Input_Action.action != null)
            ? Input_Action.action.IsPressed()
            : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed);
    }

    public bool WasReleasedThisFrame()
    {
        return (Input_Action != null && Input_Action.action != null)
            ? Input_Action.action.WasReleasedThisFrame()
            : (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);
    }

    public GameObject GetClosestSpawnedObject(Vector3 hit_position, float max_distance)
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

    public GameObject TrySpawnObjectAtPose(Pose hit_pose)
    {
        GameObject prefab_to_spawn = ChoosePrefabToSpawn(Load_Prefabs.Spawnable_Objects, Object_To_Spawn);
        if (prefab_to_spawn != null)
        {
            GameObject prefab_spawned = Instantiate(prefab_to_spawn, hit_pose.position, hit_pose.rotation);

            Vector3 camera_view = Main_Camera.transform.position - prefab_spawned.transform.position;
            camera_view.y = 0f;
            if (camera_view != Vector3.zero)
            {
                prefab_spawned.transform.rotation = Quaternion.LookRotation(camera_view);
            }

            if (prefab_spawned.GetComponent<Collider>() == null)
            {
                prefab_spawned.AddComponent<BoxCollider>();
            }

            Objects_Spawned.Add(prefab_spawned);
            return prefab_spawned;
        }
        return null;
    }

    public GameObject ChoosePrefabToSpawn(List<GameObject> list, GameObject to_find)
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

    private void SwitchObjByClick()
    {
        if (Current_Index < Load_Prefabs.Spawnable_Objects.Count - 1)
        {
            Current_Index++;
        }
        else
        {
            Current_Index = 0;
        }
        Object_To_Spawn = Load_Prefabs.Spawnable_Objects[Current_Index];
    }
}