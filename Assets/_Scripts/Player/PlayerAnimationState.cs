using UnityEngine;

public class PlayerAnimationState : IAnimationState
{
    public bool IsGrounded { get; set; }
    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }
    public bool IsWallSliding { get; set; }
    public bool IsDashing { get; set; }
    public float MovementSpeed { get; set; }
    public Vector2 MovementDirection { get; set; }
}
