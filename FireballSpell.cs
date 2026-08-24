using Godot;

public partial class FireballSpell : Area2D
{
    [Export]
    public float Speed { get; set; } = 200.0f;

    [Export]
    public float Lifetime { get; set; } = 3.0f;

    private Vector2 direction;


    public void Initialize(Vector2 newDirection)
    {
        direction = newDirection.Normalized();
        Rotation = direction.Angle();
    }


    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        GlobalPosition += direction * Speed * dt;

        Lifetime -= dt;

        if (Lifetime <= 0.0f)
        {
            QueueFree();
        }
    }
}