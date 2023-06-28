using UnityEngine;

public class GroundState : IMovementState
{
    public bool onSlope = false;
    private RaycastHit slopeHit;
    public Vector3 slopeMoveDir;
    public Vector3 moveProject;
    public Vector3 slopeForce;
    private PlayerManager playerManager;
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private CachedMoveData moveData;

    public GroundState(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
        playerMovement = playerManager.GetPlayerMovement();
        controller = playerManager.GetCharacterController();
        moveData = playerMovement.moveData;
    }

    public void OnEnteredState()
    {
        RefreshVar(playerMovement.moveData);
        controller.stepOffset = moveData.stepOffset;
    }

    public void RefreshVar(CachedMoveData moveData) => this.moveData = moveData;

    private bool IsFloor(Vector3 floor)
    {
        float angle = Vector3.Angle(Vector3.up, floor);
        return angle < Mathf.Abs(controller.slopeLimit);
    }

    private void HandleSlope()
    {
        if (playerMovement.colliderHit == null) return;
        if (IsFloor(playerMovement.colliderHit.normal)) onSlope = Vector3.Angle(Vector3.up, slopeHit.normal) > 1f;
    }

    private void SlopeMovement()
    {
        if (!Physics.Raycast(playerMovement.transform.position, Vector3.down, out slopeHit, (controller.height / 2) +
        playerMovement.groundDistance, moveData.groundMask))
        {
            slopeForce = playerMovement.colliderHit.normal;
            return;
        }
        if (IsFloor(slopeHit.normal)) slopeForce = slopeHit.normal;
    }

    public Vector3 MoveVector(Vector3 moveInput)
    {
        if (playerMovement.colliderHit == null) return Vector3.zero;
        HandleSlope();
        SlopeMovement();
        playerMovement.moveDir = (playerManager.GetOrientation().forward * moveInput.y + playerManager.GetOrientation().right * moveInput.x).normalized;
        slopeMoveDir = Vector3.ProjectOnPlane(playerMovement.moveDir, playerMovement.colliderHit.normal).normalized;
        return onSlope ? slopeMoveDir : playerMovement.moveDir;
    }

    public void OnExitedState()
    {
        playerMovement.airState.extraAirVector = playerMovement.moveSmoothen;
        controller.stepOffset = 0f;
        onSlope = false;
        playerMovement.OnStopGrounded?.Invoke();
    }

    public bool IsRelevant(PlayerMovement playerMovement)
    {
        if (controller.isGrounded || playerMovement.CoyoteGrounded()) return true;
        return false;
    }
}
