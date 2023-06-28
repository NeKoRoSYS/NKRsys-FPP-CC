using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace NeKoRoSYS.InputHandling.Mobile
{
    public class ControlPad : MonoBehaviour
    {
        [SerializeField] private int touchLimit = 10;
        [HideInInspector] public Vector2 delta = Vector2.zero;
        public UnityEvent<Vector2> OnTouchDrag;
        private Vector2 currentPos, lastPos;
        private HashSet<int> availableTouchIds = new();

        private void ResetTouchpad() => delta = Vector2.zero;
        private bool IsTouchingPad(Touch touch) => RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, touch.screenPosition);
        private void Update()
        {
            if (EventSystem.current == null) return;
            if (availableTouchIds.Count <= touchLimit) GetTouch();
            ResetTouchpad();
            if (availableTouchIds.Count == 0) return;
            foreach (var touch in Touch.activeTouches)
                { if (availableTouchIds.Contains(touch.touchId)) ApplyTouch(touch); }
            OnTouchDrag?.Invoke(delta);
        }

        private void GetTouch()
        {
            foreach (var touch in Touch.activeTouches)
            {
                var touching = touch.phase is TouchPhase.Began;
                var notTouching = touch.phase is TouchPhase.Ended or TouchPhase.Canceled or TouchPhase.None;
                if (IsTouchingPad(touch) && touching) availableTouchIds.Add(touch.touchId);
                else if (notTouching) availableTouchIds.Remove(touch.touchId);
            }
        }

        private void ApplyTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Moved)
            {
                currentPos += touch.delta;
                delta += currentPos - lastPos;
                lastPos = currentPos;
            } else if (touch.phase == TouchPhase.Stationary) ResetTouchpad();
        }
    }
}