using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using NeKoRoSYS.InputManagement;

public class CameraController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] public ControlPad controlPad;
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
                lookAction.performed += OnMouseLookInput;
                lookAction.canceled += OnMouseLookInput;
                controlPad.OnTouchDrag.AddListener(OnTouchLookInput);
            break;
            case false:
                lookAction.performed -= OnMouseLookInput;
                lookAction.canceled -= OnMouseLookInput;
                controlPad.OnTouchDrag.RemoveListener(OnTouchLookInput);
            break;
        }
    }

    private void Update() => Look();
    
    private void OnMouseLookInput(InputAction.CallbackContext ctx)
    {
        yaw += ctx.ReadValue<Vector2>().x * cameraManager.sensX * multiplier;
        pitch -= (cameraManager.invertYAxis ? -ctx.ReadValue<Vector2>().y : ctx.ReadValue<Vector2>().y) * cameraManager.sensY * multiplier;
        pitch = cameraManager.clampCam ? Mathf.Clamp(pitch, UpClamp, DownClamp) : pitch;
    }

    private void OnTouchLookInput(Vector2 touchDelta)
    {
        yaw += touchDelta.x * cameraManager.sensX * multiplier;
        pitch -= (cameraManager.invertYAxis ? -touchDelta.y : touchDelta.y) * cameraManager.sensY * multiplier;
        pitch = cameraManager.clampCam ? Mathf.Clamp(pitch, UpClamp, DownClamp) : pitch;
    }

    private void Look()
    {
        Clamping = ((pitch == UpClamp) || (pitch == DownClamp));
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