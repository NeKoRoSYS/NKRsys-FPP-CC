using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveData", menuName = "Advanced FPP Controller/PlayerMoveData", order = 0)]
public class PlayerMoveData : ScriptableObject 
{   
    [Header("Speed")]
    public bool tagMovement = true;
    public float acceleration = 10f;
    public float tagSpeed = 3.25f;
    public float tagDelay = 1f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float walkDamp = 4.5f;
    public float sprintDamp = 3.95f;
    public float crouchDamp = 20f;
    public float airSmoothen = 2.25f;

    [Header("Crouching")]
    [Range(0, 20.0f)] public float heightLerp = 10f;
    public float standHeight = 2f;
    public float crouchHeight = 1f;

    [Header("Jumping")]
    public float jumpForce = 2.5f;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float antiBump = -5;
    public float fallTimeMax = 0.25f;
    public float coyoteTimeMax = 0.1f;
    public float stepOffset = 0.5f;
}
