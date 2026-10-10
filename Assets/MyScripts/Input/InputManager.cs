using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{

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


    public InputManager(InputActionReference input_action, InputAction screen_pos)
    {
        this.Input_Action = input_action;
        this.Screen_Pos = screen_pos;
        OnEnable();
    }


    public void OnEnable()
    {
        if (Input_Action != null && Input_Action.action != null)
        {
            Input_Action.action.Enable();
        }
        if(Screen_Pos != null)
            Screen_Pos.Enable();

    }

    public void OnDisable()
    {
        if (Input_Action != null && Input_Action.action != null)
        {
            Input_Action.action.Disable();
        }
        if(Screen_Pos != null)
            Screen_Pos.Disable();
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
