using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace NeKoRoSYS.InputHandling.Mobile
{
    public enum StickMode { Fixed, Free, Floating }
    public enum StickAxis { X, Y, Both }
    public class ControlStick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public StickMode StickMode
        {
            get => stickMode;
            set
            {
                stickMode = value;

                if (stickMode == StickMode.Fixed)
                {
                    stickBounds.anchoredPosition = fixedPosition;
                    stickBounds.gameObject.SetActive(true);
                    inputArea.GetComponent<Image>().raycastTarget = false;
                } else inputArea.GetComponent<Image>().raycastTarget = true;
            }
        }
        [SerializeField] private StickMode stickMode;
        public StickAxis StickAxis { get => stickAxis; set => stickAxis = value; }
        [SerializeField] private StickAxis stickAxis = StickAxis.Both;
        [InputControl(layout = "Vector2")]
        [SerializeField] private string m_ControlPath;
        protected override string controlPathInternal { get => m_ControlPath; set => m_ControlPath = value; }
        public Vector2 RawInput {
            get => rawInput;
            set
            {
                rawInput = value;
                input = value;
                input = StickAxis == StickAxis.X ? new(input.x, 0f) : StickAxis == StickAxis.Y ? new(0f, input.y) : input;
            }
        }
        private Vector2 rawInput = Vector2.zero;
        public Vector2 input = Vector2.zero;
        public bool FullInput { get; set; }
        [SerializeField] private float fullInputThreshold = 0.75f;
        [SerializeField] private float followThreshold = 1;
        [SerializeField] private float stickRange = 1f;
        [SerializeField] private float moveThreshold = 0.17f;
        [SerializeField] private RectTransform inputArea = null;
        [SerializeField] private RectTransform stickBounds = null;
        [SerializeField] private RectTransform stickHandle = null;
        private Vector2 center = new(0.5f, 0.5f);
        private Vector2 fixedPosition = Vector2.zero;
        private Canvas canvas;
        private Camera cam;

        public void SetJoystick(int stickIndex)
        {
            switch (stickIndex) {
                case 0:
                    StickMode = StickMode.Fixed;
                break;
                case 1:
                    StickMode = StickMode.Free;
                break;
                case 2:
                    StickMode = StickMode.Floating;
                break;
            }
        }

        private void Awake() => FormatControlStick();

        private void FormatControlStick()
        {
            canvas = GetComponentInParent<Canvas>();
            stickBounds.pivot = center;
            fixedPosition = stickBounds.anchoredPosition;
            stickHandle.anchorMin = center;
            stickHandle.anchorMax = center;
            stickHandle.pivot = center;
            stickHandle.anchoredPosition = Vector2.zero;
        }

        private bool touched = false;
        private int touchId;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (touched) return;
            touched = true;
            touchId = eventData.pointerId;
            if (eventData.pointerId != touchId) return;
            if (StickMode != StickMode.Fixed)
            {
                bool inRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(inputArea, eventData.position, cam, out Vector2 localPoint);
                stickBounds.anchoredPosition = inRect ? localPoint - (stickBounds.anchorMax * inputArea.sizeDelta) + inputArea.pivot * inputArea.sizeDelta : Vector2.zero;
                stickBounds.gameObject.SetActive(true);
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!touched || eventData.pointerId != touchId) return;
            cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
            Vector2 radius = stickBounds.sizeDelta / 2;
            RawInput = (eventData.position - RectTransformUtility.WorldToScreenPoint(cam, stickBounds.position)) / (radius * canvas.scaleFactor);
            ProcessInput(input.magnitude, input.normalized, radius);
        }

        private void ProcessInput(float magnitude, Vector2 normalized, Vector2 radius)
        {
            if (StickMode == StickMode.Floating && magnitude > followThreshold)
            {
                Vector2 difference = normalized * (magnitude - followThreshold) * radius;
                stickBounds.anchoredPosition += difference;
            }
            FullInput = magnitude > fullInputThreshold;
            input = magnitude > moveThreshold ? (magnitude < fullInputThreshold ? normalized * 0.35f : normalized * 0.7f) : Vector2.zero;
            stickHandle.anchoredPosition = input * stickRange * radius;
            SendValueToControl(input);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != touchId) return;
            if (StickMode != StickMode.Fixed) stickBounds.gameObject.SetActive(false);
            touched = false;
            FullInput = false;
            input = Vector2.zero;
            stickHandle.anchoredPosition = Vector2.zero;
            SendValueToControl(Vector2.zero);
        }
    }
}