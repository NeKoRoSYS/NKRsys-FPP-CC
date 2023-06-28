using UnityEngine;
[System.Serializable]
public struct CachedMoveData
{   
    [Header("Speed")]
    public bool tagMovement;
    public float acceleration;
    public float tagSpeed;
    public float tagDelay;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float walkDamp;
    public float sprintDamp;
    public float crouchDamp;
    public float airSmoothen;

    [Header("Crouching")]
    [Range(0, 20.0f)] public float heightLerp;
    public float standHeight;
    public float crouchHeight;

    [Header("Jumping")]
    public float jumpForce;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float antiBump;
    public float fallTimeMax;
    public float coyoteTimeMax;
    public float stepOffset;
}
