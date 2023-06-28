using UnityEngine;

public interface IMovementState
{
    public abstract void OnEnteredState();
    public abstract void RefreshVar(CachedMoveData moveData);
    public abstract Vector3 MoveVector(Vector3 moveInput);
    public abstract void OnExitedState();
    public abstract bool IsRelevant(PlayerMovement playerMovement);
}