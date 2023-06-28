using UnityEngine;

public class AirState : IMovementState
{
    public Vector3 extraAirVector;
    private PlayerManager playerManager;
    private PlayerMovement playerMovement;
    private CachedMoveData moveData;

    public AirState(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
        playerMovement = playerManager.GetPlayerMovement();
        moveData = playerMovement.moveData;
    }

    public void OnEnteredState()
    {
        RefreshVar(playerMovement.moveData);
    }

    public void RefreshVar(CachedMoveData moveData) => this.moveData = moveData;

    public Vector3 MoveVector(Vector3 moveInput)
    {
        playerMovement.moveDir = (playerManager.GetOrientation().forward * moveInput.y + playerManager.GetOrientation().right * moveInput.x).normalized;
        Vector3 moveVector = (playerMovement.moveDir * 1.5f) + extraAirVector;
        if (playerMovement.moveDir != Vector3.zero && playerMovement.moveDir != extraAirVector) extraAirVector = playerMovement.moveSmoothen;
        if (moveVector.sqrMagnitude > 1) moveVector.Normalize();
        return moveVector;
    }

    public void OnExitedState()
    {
        extraAirVector = Vector3.zero;
    }

    public bool IsRelevant(PlayerMovement playerMovement)
    {
        if (!playerManager.GetCharacterController().isGrounded && !playerMovement.CoyoteGrounded()) return true;
        return false;
    }
}