using UnityEngine;

public class CameraManager : MonoBehaviour {
    [Header("References")]
    [SerializeField] public PlayerManager playerManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] public CameraController cameraController;

    [Header("Values")]
    [SerializeField] public bool clampCam, moveCamera;
    [SerializeField] public bool invertYAxis;
    [SerializeField] public bool reduceMotion;
    [SerializeField] public float sensX, sensY;
    
    [Header("Headbobbing")]
	[SerializeField] private float landBobSpeed = 15f;
	[SerializeField] private float landBobMultiplier = 1f;
	[SerializeField] public Vector3 desyncOffset, bodyOffset, desiredLandBob, landBobOffset, moveBobPos, moveBobRot;
    [SerializeField] public bool moveBobX, moveBobY;
    [SerializeField, Range(0, 2f)] private float amplitude;
    [SerializeField, Range(0, 30f)] private float frequency;
    [SerializeField] public float walkFreq, sprintFreq, crouchFreq;
    [SerializeField] private readonly float toggleSpeed = 3.0f;
    private Vector3 startPos;
    private Vector3 startRot;
    [SerializeField] public float moveBobReturnSpeed = 10f;

    /// [Header("Field of View")]
    /// public float walkFov, sprintFov, crouchFov;

    [Header("Tilts")]
    [SerializeField] public bool tilt;
    [SerializeField] public bool Tilt {
        get { return tilt; }
        set
        {
            tilt = value;
            if (value == false) TargetMoveTilt = 0f;
        }
    }
    [SerializeField] public float moveTilt, tiltMultiplier;
    [SerializeField] public float TargetMoveTilt { get; set; }

    private void Start()
    {
        startPos = Vector3.zero;
        startRot = Vector3.zero;
        playerMovement = playerManager.GetPlayerMovement();
        playerMovement.OnStartLand.AddListener(LandBob);
    }

    private void LateUpdate() => transform.position = moveCamera ? playerManager.GetHead().transform.position + desyncOffset + bodyOffset + (!reduceMotion ? moveBobPos + landBobOffset : Vector3.zero) : transform.localPosition;

    private void Update()
    {
        if(tilt && playerMovement.allowMove) TiltCamera();
        MoveBob();
		if (desyncOffset != Vector3.zero) desyncOffset = Vector3.Slerp(desyncOffset, Vector3.zero, Time.deltaTime * 5f);
    }

    #region Camera Tilting
    float _moveTilt;
    private void TiltCamera()
    {
        if (playerMovement.isCrouching)
        {
            TargetMoveTilt = Mathf.Lerp(TargetMoveTilt, 0f, tiltMultiplier * Time.deltaTime);
            return;
        }
        _moveTilt = moveTilt * -playerMovement.moveInput.x;
        TargetMoveTilt = Mathf.Lerp(TargetMoveTilt, playerMovement.moveInput.y != 0 ? _moveTilt / 2 : _moveTilt, tiltMultiplier * Time.deltaTime);
    }
    #endregion

    #region Headbob Logic
    private void MoveBob()
    {
        if (reduceMotion || !moveCamera)
        {
            desiredLandBob = desiredLandBob == Vector3.zero ? desiredLandBob : Vector3.zero;
            landBobOffset = landBobOffset == Vector3.zero ? landBobOffset : Vector3.zero;
            moveBobPos = moveBobPos == Vector3.zero ? moveBobPos : Vector3.zero;
            moveBobRot = moveBobRot == Vector3.zero ? moveBobRot : Vector3.zero;
            return;
        }
        frequency = playerMovement.isCrouching ? crouchFreq :
                    playerMovement.isSprinting ? sprintFreq :
                    walkFreq;
		desiredLandBob = Vector3.Lerp(desiredLandBob, Vector3.zero, Time.deltaTime * landBobSpeed * 0.5f);
		landBobOffset = Vector3.Lerp(landBobOffset, desiredLandBob, Time.deltaTime * landBobSpeed);
        CheckMotion();
    }

    public void LandBob()
	{
        if (reduceMotion) return;
        if (playerMovement.coyoteTime < playerMovement.moveData.fallTimeMax) return;
		Vector3 bob = ClampVector(new Vector3(0f, playerMovement.fallSpeed, 0f) * 0.15f, -3f, 3f);
		desiredLandBob = bob * landBobMultiplier;
	}

	private Vector3 ClampVector(Vector3 vec, float min, float max) => new(Mathf.Clamp(vec.x, min, max), Mathf.Clamp(vec.y, min, max), Mathf.Clamp(vec.z, min, max));

    private void CheckMotion()
    {
        float speed = playerMovement.velocity.magnitude;
        if (!playerMovement.CoyoteGrounded())
        {
            ResetPosition();
            ResetRotation();
            return;
        }
        if (!playerMovement.isSprinting) ResetRotation();
        if (playerMovement.isCrouching)
        {
            ResetPosition();
            return;
        }
        if (speed < toggleSpeed)
        {
            ResetPosition();
            return;
        }
        if (!playerMovement.isMoving) return;
        PlayMotion(FootstepMotion(), tilt ? FootstepRotation() : Vector3.zero);
    }

    private void ResetPosition()
    {
        if (moveBobPos == startPos) return;
        moveBobPos = Vector3.Lerp(moveBobPos, startPos, moveBobReturnSpeed * Time.deltaTime);
    }

    private void ResetRotation()
    {
        if (moveBobRot == startRot) return;
        moveBobRot = Vector3.Lerp(moveBobRot, startRot, moveBobReturnSpeed * Time.deltaTime);
    }

    private void PlayMotion(Vector3 motion, Vector3 rotation)
    {
        moveBobPos += motion;
        if (!playerMovement.isSprinting) return;
        moveBobRot += rotation;
    }

    private Vector3 FootstepMotion()
    {
        Vector3 pos = Vector3.zero;
        if (moveBobY) pos.y += Mathf.Sin(Time.time * frequency) * amplitude * Time.deltaTime;
        if (moveBobX) pos.x += Mathf.Cos(Time.time * frequency * 0.5f) * amplitude / 1.75f * Time.deltaTime * playerManager.transform.right.x;
        if (moveBobX) pos.z += Mathf.Cos(Time.time * frequency * 0.5f) * amplitude / 1.75f * Time.deltaTime * playerManager.transform.right.z;
        return pos;
    }

    private Vector3 FootstepRotation()
    {
        Vector3 rot = Vector3.zero;
        if (moveBobX) rot.z += Mathf.Cos(Time.time * frequency * 0.5f) * amplitude * 5 * Time.deltaTime;
        return rot;
    }
    #endregion
}