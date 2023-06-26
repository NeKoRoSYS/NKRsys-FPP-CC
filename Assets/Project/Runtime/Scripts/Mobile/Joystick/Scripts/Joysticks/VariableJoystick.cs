using UnityEngine;
using UnityEngine.EventSystems;

public class VariableJoystick : Joystick
{
    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1;
    [SerializeField] private JoystickType joystickType;
    public JoystickType JoystickType
    {
        get { return joystickType; }
        set
        {
            joystickType = value;
            
            if(joystickType == JoystickType.Fixed)
            {
                background.anchoredPosition = fixedPosition;
                background.gameObject.SetActive(true);
                joystickInput.raycastTarget = false;
            } else joystickInput.raycastTarget = true;
        }
    }

    private Vector2 fixedPosition = Vector2.zero;

    public void SetMode(int joystickIndex)
    {
        switch (joystickIndex) {
            case 0:
                JoystickType = JoystickType.Fixed;
            break;
            case 1:
                JoystickType = JoystickType.Floating;
            break;
            case 2:
                JoystickType = JoystickType.Dynamic;
            break;
        }
    }

    protected override void Start()
    {
        base.Start();
        fixedPosition = background.anchoredPosition;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!touched)
        {
            touched = true;
            touchId = eventData.pointerId;
        }

        if(eventData.pointerId == touchId && joystickType != JoystickType.Fixed)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.gameObject.SetActive(true);
        }
        
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if(eventData.pointerId == touchId && joystickType != JoystickType.Fixed) background.gameObject.SetActive(false);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (joystickType == JoystickType.Dynamic && magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}

public enum JoystickType { Fixed, Floating, Dynamic }