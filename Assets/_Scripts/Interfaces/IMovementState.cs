using UnityEngine;

public interface IMovementState
{
    bool IsGrounded { get; }
    bool IsJumping { get; }
    bool IsFalling { get; }
    bool IsWallSliding { get; }
    bool IsDashing { get; }
    float MovementSpeed { get; }
    Vector2 MovementDirection { get; }
}

