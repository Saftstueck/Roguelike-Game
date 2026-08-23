using Godot;

public partial class Weapon_AI : Sprite2D
{
    private float swingDuration = 0.5f;
    private float swingCooldown = 0.2f;
    private float maximumAngle = Mathf.Pi / 4.0f;
	private float size = 2.0f;
	private float startingrotation = 0.0f;

    private float swingTimer;
    private float cooldownTimer;
    private bool isSwinging;

    public override void _Ready()
    {
        cooldownTimer = swingCooldown;
		Scale = Vector2.One * size;
		Rotation = startingrotation;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        cooldownTimer += dt;

        if (Input.IsActionJustPressed("wepoin_swing") &&
            !isSwinging &&
            cooldownTimer >= swingCooldown)
        {
            isSwinging = true;
            swingTimer = 0.0f;
            cooldownTimer = 0.0f;
        }

        if (!isSwinging)
        {
            return;
        }

        swingTimer += dt;

        float progress = Mathf.Clamp(
            swingTimer / swingDuration,
            0.0f,
            1.0f
        );

        Rotation = Mathf.Sin(progress * Mathf.Pi) * maximumAngle;

        if (progress >= 1.0f)
        {
            isSwinging = false;
            Rotation = 0.0f;
        }
    }
}