using Godot;

public partial class WeaponAttacks : Node
{
    [ExportCategory("Spells")]
    [Export]
    public PackedScene MageSpellsScene { get; set; }

    [Export]
    public string SelectedSpell { get; set; } =
        "Fireball";

    [ExportCategory("Input")]
    [Export]
    public string AttackAction { get; set; } =
        "weapon_swing";

    [ExportCategory("Bow")]
    [Export]
    public float BowFullDrawTime { get; set; } =
        0.35f;

    [Export]
    public string BowProjectileName { get; set; } =
        "Arrow";

    [Export]
    public float BowSpawnDistance { get; set; } =
        16.0f;

    [ExportCategory("Magic")]
    [Export]
    public float MagicChargeTime { get; set; } =
        0.5f;

    [Export]
    public float MagicCooldown { get; set; } =
        0.25f;

    [Export]
    public float SpellSpawnDistance { get; set; } =
        16.0f;

    private WeaponMovement movement;
    private WeaponCTR weaponTexture;
    private Node spellLibrary;

    private bool bowDrawing;
    private float bowTimer;

    private bool magicCharging;
    private float magicCharge;
    private float magicCooldown = 999.0f;

    public override void _Ready()
    {
        movement =
            GetParent() as WeaponMovement;

        weaponTexture =
            GetParent()
                .GetNodeOrNull<WeaponCTR>(
                    "WeaponCTR"
                );

        if (MageSpellsScene != null)
        {
            spellLibrary =
                MageSpellsScene.Instantiate();
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        if (movement == null ||
            weaponTexture == null ||
            !InputMap.HasAction(AttackAction))
        {
            return;
        }

        float dt =
            (float)delta;

        WeaponCTR.WeaponKind kind =
            weaponTexture.CurrentKind;

        Vector2 direction =
            movement.AimDirection;

        if (kind == WeaponCTR.WeaponKind.Bow)
        {
            CancelMagic();
            UpdateBow(dt, direction);
        }
        else if (
            kind == WeaponCTR.WeaponKind.Wand ||
            kind == WeaponCTR.WeaponKind.Staff)
        {
            CancelBow();
            UpdateMagic(dt, direction);
        }
        else
        {
            CancelBow();
            CancelMagic();

            if (Input.IsActionJustPressed(
                    AttackAction))
            {
                movement.StartSweep();
            }
        }
    }

    private void UpdateBow(
        float delta,
        Vector2 direction)
    {
        if (Input.IsActionJustPressed(
                AttackAction))
        {
            bowDrawing = true;
            bowTimer = 0.0f;

            weaponTexture.SetBowFrame(2);
        }

        if (bowDrawing &&
            Input.IsActionPressed(AttackAction))
        {
            bowTimer += delta;

            weaponTexture.SetBowFrame(
                bowTimer >= BowFullDrawTime
                    ? 3
                    : 2
            );
        }

        if (bowDrawing &&
            Input.IsActionJustReleased(
                AttackAction))
        {
            float charge =
                Mathf.Clamp(
                    bowTimer /
                    Mathf.Max(
                        BowFullDrawTime,
                        0.001f
                    ),
                    0.0f,
                    1.0f
                );

            SpawnProjectile(
                BowProjectileName,
                direction,
                charge,
                BowSpawnDistance
            );

            bowDrawing = false;
            bowTimer = 0.0f;

            weaponTexture.SetBowFrame(1);
        }
    }

    private void UpdateMagic(
        float delta,
        Vector2 direction)
    {
        magicCooldown +=
            delta;

        if (Input.IsActionJustPressed(
                AttackAction) &&
            !magicCharging &&
            magicCooldown >= MagicCooldown)
        {
            magicCharging = true;
            magicCharge = 0.0f;
        }

        if (!magicCharging)
            return;

        if (Input.IsActionPressed(
                AttackAction))
        {
            magicCharge =
                Mathf.Clamp(
                    magicCharge +
                    delta /
                    Mathf.Max(
                        MagicChargeTime,
                        0.001f
                    ),
                    0.0f,
                    1.0f
                );

            return;
        }

        float charge =
            magicCharge;

        magicCharging = false;
        magicCharge = 0.0f;
        magicCooldown = 0.0f;

        SpawnProjectile(
            SelectedSpell,
            direction,
            charge,
            SpellSpawnDistance
        );
    }

    private void SpawnProjectile(
        string projectileName,
        Vector2 direction,
        float charge,
        float spawnDistance)
    {
        if (spellLibrary == null)
            return;

        MageSpell template =
            spellLibrary.GetNodeOrNull<MageSpell>(
                new NodePath(projectileName)
            );

        MageSpell projectile =
            template.Duplicate() as MageSpell;

        if (projectile == null)
            return;

        GetTree().CurrentScene.AddChild(
            projectile
        );

        projectile.GlobalPosition =
            weaponTexture.GlobalPosition +
            direction * spawnDistance;

        projectile.Launch(
            direction,
            charge
        );
    }

    private void CancelBow()
    {
        bowDrawing = false;
        bowTimer = 0.0f;
    }

    private void CancelMagic()
    {
        magicCharging = false;
        magicCharge = 0.0f;
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(
                spellLibrary))
        {
            spellLibrary.Free();
        }

        spellLibrary = null;
    }
}