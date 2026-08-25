using Godot;

public partial class MageSpell : CharacterBody2D
{
	[ExportCategory("Movement")]
	[Export] public float MinimumSpeed { get; set; } = 120.0f;
	[Export] public float MaximumSpeed { get; set; } = 500.0f;
	[Export] public float Lifetime { get; set; } = 4.0f;
	[Export] public bool RotateTowardsMovement { get; set; } = true;

	[ExportCategory("Power")]
	[Export] public float MinimumDamage { get; set; } = 1.0f;
	[Export] public float MaximumDamage { get; set; } = 10.0f;
	[Export] public float MinimumScaleMultiplier { get; set; } = 0.7f;
	[Export] public float MaximumScaleMultiplier { get; set; } = 1.4f;

	public float Damage { get; protected set; }
	public float Charge { get; protected set; }

	protected bool IsMoving;
	protected float RemainingLifetime;

	private Vector2 startingScale;

	public override void _Ready()
	{
		startingScale = Scale;
		SetPhysicsProcess(false);
	}

	public virtual void Launch(Vector2 direction, float charge)
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

		Velocity = direction * speed;
		Scale = startingScale * scaleMultiplier;
		Rotation = direction.Angle();

		RemainingLifetime = Lifetime;
		IsMoving = true;
		Visible = true;

		CpuParticles2D particles =
			GetNodeOrNull<CpuParticles2D>("CPUParticles2D");

		if (particles != null)
		{
			particles.Restart();
			particles.Emitting = true;
		}

		OnLaunched();
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (IsMoving)
		{
			BeforeMovement(dt);

			if (RotateTowardsMovement &&
				Velocity.LengthSquared() > 0.001f)
			{
				Rotation = Velocity.Angle();
			}

			KinematicCollision2D collision =
				MoveAndCollide(Velocity * dt);

			if (collision != null)
			{
				OnSpellCollision(collision);
			}
		}

		RemainingLifetime -= dt;

		if (RemainingLifetime <= 0.0f)
			QueueFree();
	}

	protected virtual void OnLaunched()
	{
	}

	protected virtual void BeforeMovement(float delta)
	{
	}

	protected virtual void OnSpellCollision(
		KinematicCollision2D collision)
	{
		QueueFree();
	}

	protected void StopMoving()
	{
		Velocity = Vector2.Zero;
		IsMoving = false;
	}
}
