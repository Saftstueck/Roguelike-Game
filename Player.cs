using Godot;

public partial class Player : CharacterBody2D
{
	[ExportCategory("Movement")]
	[Export] public float Speed { get; set; } = 200.0f;
	[Export] public float GroundAcceleration { get; set; } = 1400.0f;
	[Export] public float AirAcceleration { get; set; } = 500.0f;
	[Export] public float JumpVelocity { get; set; } = -280.0f;
	[Export] public float MaximumWeaponVelocity { get; set; } = 650.0f;

	private AnimatedSprite2D animatedSprite;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
			velocity += GetGravity() * dt;

		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		float direction = Input.GetAxis("move_left", "move_right");
		float targetSpeed = direction * Speed;
		float acceleration = IsOnFloor()
			? GroundAcceleration
			: AirAcceleration;

		velocity.X = Mathf.MoveToward(
			velocity.X,
			targetSpeed,
			acceleration * dt
		);

		Velocity = velocity;
		MoveAndSlide();

		if (direction != 0.0f)
			animatedSprite.FlipH = direction < 0.0f;

		animatedSprite.Play(direction != 0.0f ? "walk" : "idle");
	}

	public void ApplyWeaponVelocity(Vector2 velocityChange)
	{
		Velocity = (Velocity + velocityChange).LimitLength(
			MaximumWeaponVelocity
		);
	}
}
