using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ARMainApplication : MonoBehaviour
{
    // EVENTI OBSERVER
    public event Action<GameObject> OnSelectedObjectChanged;
    public event Action<bool> OnDraggingStateChanged;
    public event Action<GameObject> OnObjectToSpawnChanged;
    public event Action<GameObject> OnObjectSpawned;

    [Header("Configurazione Prefab")]
    private LoadPrefabs load_prefabs;
    public LoadPrefabs Load_Prefabs
    {
        get => load_prefabs;
        set => load_prefabs = value;
    }

    private GameObject object_to_spawn;
    public GameObject Object_To_Spawn
    {
        get => object_to_spawn;
        set
        {
            object_to_spawn = value;
            OnObjectToSpawnChanged?.Invoke(object_to_spawn); // Notifica Observer
        }
    }

    private bool is_dragging;
    public bool Is_Dragging
    {
        get => is_dragging;
        set
        {
            if (is_dragging != value)
            {
                is_dragging = value;
                OnDraggingStateChanged?.Invoke(is_dragging); // Notifica Observer
            }
        }
    }

    private GameObject selected_object;
    public GameObject Selected_Object
    {
        get => selected_object;
        set
        {
            if (selected_object != value)
            {
                selected_object = value;
                OnSelectedObjectChanged?.Invoke(selected_object); // Notifica Observer
            }
        }
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

    public List<ARRaycastHit> Hit_List { get; set; }
    public List<GameObject> Objects_Spawned { get; set; }
    public int Current_Index { get; set; }

    public ARRaycastManager Raycast_Manager { get; set; }
    public ARPlaneManager Plane_Manager { get; set; }
    public Camera Main_Camera { get; set; }

    private ASpawnObjectFactory object_factory;
    public ASpawnObjectFactory Object_Factory
    {
        get => object_factory;
        set => object_factory = value;
    }

    private IObjectInteractionState current_state;

    // GESTIONE COMMAND PATTERN (per Undo/Redo)
    private readonly Stack<ICommand> command_history = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        command_history.Push(command);
    }

    public void UndoLastCommand()
    {
        if (command_history.Count > 0)
        {
            ICommand cmd = command_history.Pop();
            cmd.Undo();
        }
    }

    private void Awake()
    {
        Load_Prefabs = GetComponent<LoadPrefabs>();
        Raycast_Manager = GetComponent<ARRaycastManager>();
        Plane_Manager = GetComponent<ARPlaneManager>();

        Main_Camera = Camera.main;

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
        Plane_Manager.enabled = true;
        Current_Index = 0;

        if (Load_Prefabs != null && Load_Prefabs.Spawnable_Objects != null && Load_Prefabs.Spawnable_Objects.Count > 0)
        {
            Object_To_Spawn = Load_Prefabs.Spawnable_Objects[Current_Index];
        }
        else
        {
            Debug.LogError("Spawnable_Objects è vuoto o non assegnato in LoadPrefabs!");
        }

        Is_Dragging = false;
        Objects_Spawned = new List<GameObject>();
        Hit_List = new List<ARRaycastHit>();

        ChangeState(new IdleState(), null);

        Object_Factory = new GeometricShapesFactory();
    }

    private void Update()
    {
        if (!IsPointerOverUI() && current_state != null)
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

    public void NotifyObjectSpawned(GameObject spawnedObj)
    {
        OnObjectSpawned?.Invoke(spawnedObj);
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

    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        return EventSystem.current.IsPointerOverGameObject();
    }
}