using Godot;

public partial class MageSpell : Area2D
{
    [ExportCategory("Movement")]
    [Export] public float MinimumSpeed { get; set; } = 120.0f;
    [Export] public float MaximumSpeed { get; set; } = 500.0f;
    [Export] public float Lifetime { get; set; } = 4.0f;

    [ExportCategory("Power")]
    [Export] public float MinimumDamage { get; set; } = 1.0f;
    [Export] public float MaximumDamage { get; set; } = 10.0f;
    [Export] public float MinimumScaleMultiplier { get; set; } = 0.7f;
    [Export] public float MaximumScaleMultiplier { get; set; } = 1.4f;

    public float Damage { get; private set; }
    public float Charge { get; private set; }

    private Vector2 velocity;
    private Vector2 startingScale;
    private float remainingLifetime;
    private bool launched;

    public override void _Ready()
    {
        startingScale = Scale;
        SetPhysicsProcess(false);
    }

    public void Launch(Vector2 direction, float charge)
    {
        direction = direction.Normalized();

        if (direction.LengthSquared() < 0.001f)
            direction = Vector2.Right;

        Charge = Mathf.Clamp(charge, 0.0f, 1.0f);

        float speed = Mathf.Lerp(
            MinimumSpeed,
            MaximumSpeed,
            Charge
        );

        Damage = Mathf.Lerp(
            MinimumDamage,
            MaximumDamage,
            Charge
        );

        float scaleMultiplier = Mathf.Lerp(
            MinimumScaleMultiplier,
            MaximumScaleMultiplier,
            Charge
        );

        velocity = direction * speed;
        Scale = startingScale * scaleMultiplier;
        Rotation = direction.Angle();

        remainingLifetime = Lifetime;
        launched = true;

        Visible = true;
        Monitoring = true;
        Monitorable = true;

        CpuParticles2D particles =
            GetNodeOrNull<CpuParticles2D>("CPUParticles2D");

        if (particles != null)
        {
            particles.Restart();
            particles.Emitting = true;
        }

        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!launched)
            return;

        float dt = (float)delta;

        GlobalPosition += velocity * dt;

        remainingLifetime -= dt;

        if (remainingLifetime <= 0.0f)
            QueueFree();
    }
}