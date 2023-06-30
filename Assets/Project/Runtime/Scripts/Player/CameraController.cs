using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using NeKoRoSYS.InputHandling.Mobile;

public class CameraController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private Transform camHolder;
    
    [Header("Camera")]
    [SerializeField] public bool Clamping;
    [SerializeField] private float UpClamp, DownClamp;
    [SerializeField] private float pitch, yaw;
    [SerializeField] public float pitchLerp, yawLerp, damp;
    private readonly float multiplier = 0.01f;

    private bool playerInputInit = false;
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        ToggleInputs(true);
        playerInputInit = true;
    }

    private void OnEnable()
    {
        if (!playerInputInit) return;
        ToggleInputs(true);
    }

    private void OnDisable() => ToggleInputs(false);
    
    private void ToggleInputs(bool enable)
    {
        var lookAction = PlayerInput.Instance.LookAction;
        switch (enable)
        {
            case true:
                lookAction.performed += OnLookInput;
                lookAction.canceled += OnLookInput;
                PlayerInput.Instance.controlPad.OnTouchDrag.AddListener(OnTouchLookInput);
            break;
            case false:
                lookAction.performed -= OnLookInput;
                lookAction.canceled -= OnLookInput;
                PlayerInput.Instance.controlPad.OnTouchDrag.RemoveListener(OnTouchLookInput);
            break;
        }
    }

    private void Update() => Look();
    
    private void OnLookInput(InputAction.CallbackContext ctx) => ProcessDelta(ctx.ReadValue<Vector2>());
    private void OnTouchLookInput(Vector2 touchDelta) => ProcessDelta(touchDelta);
    private void ProcessDelta(Vector2 lookDelta)
    {
        yaw += lookDelta.x * cameraManager.sensX * multiplier;
        pitch -= (cameraManager.invertYAxis ? -lookDelta.y : lookDelta.y) * cameraManager.sensY * multiplier;
        pitch = cameraManager.clampCam ? Mathf.Clamp(pitch, UpClamp, DownClamp) : pitch;
    }

    private void Look()
    {
        Clamping = (pitch == UpClamp) || (pitch == DownClamp);
        if (yaw >= 360f) 
        {
            yaw -= 360f;
            yawLerp -= 360f;
        } else if (yaw <= -360f)
        {
            yaw += 360f;
            yawLerp += 360f;
        }
        pitchLerp = Mathf.Lerp(pitchLerp, pitch, damp * 10f * Time.deltaTime);
        yawLerp = Mathf.Lerp(yawLerp, yaw, damp * 10f * Time.deltaTime);
        cameraManager.playerManager.transform.rotation = Quaternion.Euler(0, yawLerp, 0);
        camHolder.transform.rotation = Quaternion.Euler(pitchLerp, yawLerp, !cameraManager.reduceMotion ? cameraManager.TargetMoveTilt + cameraManager.moveBobRot.z : 0f);
    }
}