using Godot;

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 200.0f;

    [Export]
    public float JumpVelocity = -275.0f;

    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Gravity
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Left / right movement
        float direction = Input.GetAxis("move_left", "move_right");

        if (direction != 0)
        {
            velocity.X = direction * Speed;

            // Flip sprite depending on direction
            animatedSprite.FlipH = direction < 0;
        }
        else
        {
            velocity.X = Mathf.MoveToward(
                velocity.X,
                0,
                Speed
            );
        }

        Velocity = velocity;

        MoveAndSlide();

        UpdateAnimation(direction);
    }

    private void UpdateAnimation(float direction)
    {
        if (direction != 0)
        {
            animatedSprite.Play("walk");
        }
        else
        {
            animatedSprite.Play("idle");
        }
    }
}