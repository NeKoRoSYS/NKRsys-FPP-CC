using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;

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
        public UnityEvent OnStopPress;
        private Coroutine visualCoroutine;

        public void OnPointerDown(PointerEventData eventData)
        {
            SendValueToControl(1.0f);
            OnStartPress?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SendValueToControl(0.0f);
            OnStopPress?.Invoke();
        }

        private void Awake()
        {
            icon = GetComponent<Image>();
            releasedIcon = icon.sprite;
            OnStartPress.AddListener(PressButton);
            OnStopPress.AddListener(ReleaseButton);
        }

        private void PressButton()
        {
            if (visualCoroutine != null) StopCoroutine(visualCoroutine);
            visualCoroutine = StartCoroutine(PlayVisuals(pressedIcon, pressedColor));
        }

        private void ReleaseButton()
        {
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