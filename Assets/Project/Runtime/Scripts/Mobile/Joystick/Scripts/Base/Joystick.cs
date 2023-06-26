using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class Joystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal { get { return (snapX) ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x; } }
    public float Vertical { get { return (snapY) ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; } }
    public Vector2 Direction { get { return new Vector2(Horizontal, Vertical); } }

    public float HandleRange
    {
        get { return handleRange; }
        set { handleRange = Mathf.Abs(value); }
    }

    public float DeadZone
    {
        get { return deadZone; }
        set { deadZone = Mathf.Abs(value); }
    }

    public AxisOptions AxisOptions { get { return AxisOptions; } set { axisOptions = value; } }
    public bool SnapX { get { return snapX; } set { snapX = value; } }
    public bool SnapY { get { return snapY; } set { snapY = value; } }

    [InputControl(layout = "Vector2")]
    [SerializeField] private string m_ControlPath;
    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    [SerializeField] public float handleRange = 1;
    [SerializeField] private float fullInputThreshold = 0.75f;
    [SerializeField] private float deadZone = 0;
    [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
    [SerializeField] private bool snapX = false;
    [SerializeField] private bool snapY = false;
    [SerializeField] public Image joystickInput = null;
    [SerializeField] public RectTransform movementState;
    [SerializeField] public Vector2 normalSize = Vector2.zero;
    [SerializeField] public Vector2 walkSize = Vector2.zero;
    [SerializeField] public float joystickDamp = 10;
    [SerializeField] protected RectTransform background = null;
    [SerializeField] private RectTransform handle = null;
    private RectTransform baseRect = null;

    private Canvas canvas;
    private Camera cam;

    public Vector2 RawInput {
        get { return rawInput; }

        set { 
            rawInput = value;
            input = value;
        }
    }
    private Vector2 rawInput = Vector2.zero;
    private Vector2 input = Vector2.zero;

    protected virtual void Start()
    {
        HandleRange = handleRange;
        DeadZone = deadZone;
        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");

        Vector2 center = new(0.5f, 0.5f);
        background.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
    }

    public bool touched = false;
    public int touchId;
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId == touchId)
        {
            normalSize = movementState.sizeDelta;
            walkSize = normalSize / 1.5f;
            OnDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!touched) return;
        if (eventData.pointerId == touchId)
        {
            cam = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) cam = canvas.worldCamera;
            Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
            Vector2 radius = background.sizeDelta / 2;

            RawInput = (eventData.position - position) / (radius * canvas.scaleFactor);
            FormatInput();
            HandleInput(input.magnitude, input.normalized, radius, cam);
            handle.anchoredPosition = input * radius * handleRange;
            SendValueToControl(input);
        }
    }

    public bool FullInput { get; set; }
    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        FullInput = magnitude > fullInputThreshold;

        if (magnitude > deadZone)
        {
            if (magnitude < fullInputThreshold) input = normalised * 0.35f;
            else input = normalised * 0.7f;
        } else input = Vector2.zero;
    }

    private void FormatInput()
    {
        if (axisOptions == AxisOptions.Horizontal) input = new Vector2(input.x, 0f);
        else if (axisOptions == AxisOptions.Vertical) input = new Vector2(0f, input.y);
    }

    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0) return value;

        if (axisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(input, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f) return 0;
                else return (value > 0) ? 1 : -1;
            } else if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f) return 0;
                else return (value > 0) ? 1 : -1;
            }
            return value;
        } else
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
        }
        
        return 0;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == touchId)
        {
            touched = false;
            input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            SendValueToControl(Vector2.zero);
        }
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        _ = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out Vector2 localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }
}

public enum AxisOptions { Both, Horizontal, Vertical }