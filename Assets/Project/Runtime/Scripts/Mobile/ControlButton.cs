using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;
using System.Collections.Generic;

namespace NeKoRoSYS.InputHandling.Mobile
{
    public class ControlButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visuals")]
        [SerializeField] private Sprite pressedIcon;
        [SerializeField] private Color releasedColor = Color.white, pressedColor = Color.grey;
        [SerializeField] private bool animateSprite = true, animateColor = true;
        private Image icon;
        private Sprite releasedIcon;

        [Header("Input")]
        [InputControl(layout = "Button")]
        [SerializeField] private string m_ControlPath;
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        [Header("Events")]
        public UnityEvent OnStartPress;
        public UnityEvent OnDoubleTap;
        public UnityEvent OnStopPress;
        private Coroutine visualCoroutine;

        private GraphicRaycaster raycaster;
        private Vector2 startPos;
        public void CheckOverlap<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
        {
            List<RaycastResult> results = new();
            eventData.position = startPos;
			raycaster.Raycast(eventData, results);
			foreach (RaycastResult result in results)
			{
				if (result.gameObject == gameObject) continue;
                if (!result.gameObject.TryGetComponent(out ControlButton button)) return;
                if (Equals(eventFunction, ExecuteEvents.pointerDownHandler) && button.touched) break;
                else if (Equals(eventFunction, ExecuteEvents.pointerUpHandler) && !button.touched) break;
                ExecuteEvents.Execute(result.gameObject, eventData, eventFunction);
			}
        }

        private bool touched = false;
        private int touchId, touchAmount;
        private readonly float maxTapInterval = 15f;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (touched) return;
            touched = true;
            touchId = eventData.pointerId;
            if (eventData.pointerId != touchId) return;
            startPos = eventData.position;
            CheckOverlap(eventData, ExecuteEvents.pointerDownHandler);
            OnStartPress?.Invoke();
        }

        internal void ResetTapAmount() => touchAmount = 0;
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != touchId) return;
            touched = false;
            CheckOverlap(eventData, ExecuteEvents.pointerUpHandler);
            OnStopPress?.Invoke();
        }

        private void Awake()
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
            icon = GetComponent<Image>();
            releasedIcon = icon.sprite;
            OnStartPress.AddListener(PressButton);
            OnStopPress.AddListener(ReleaseButton);
        }

        private void PressButton()
        {
            SendValueToControl(1.0f);
            touchAmount++;
            CancelInvoke(nameof(ResetTapAmount));
            Invoke(nameof(ResetTapAmount), maxTapInterval * Time.deltaTime);
            if (touchAmount == 2)
            {
                touchAmount = 0;
                OnDoubleTap?.Invoke();
            }
            if (visualCoroutine != null) StopCoroutine(visualCoroutine);
            visualCoroutine = StartCoroutine(PlayVisuals(pressedIcon, pressedColor));
        }

        private void ReleaseButton()
        {
            SendValueToControl(0.0f);
            if (visualCoroutine != null) StopCoroutine(visualCoroutine);
            visualCoroutine = StartCoroutine(PlayVisuals(releasedIcon, releasedColor));
        }

        private IEnumerator PlayVisuals(Sprite targetIcon, Color targetColor)
        {
            if (targetIcon != null && animateSprite) icon.sprite = targetIcon;
            if (animateColor)
            {
                while (icon.color != targetColor)
                {
                    icon.color = Color.Lerp(icon.color, targetColor, Time.deltaTime * 15f);
                    yield return null;
                }
            }
        }
    }
}