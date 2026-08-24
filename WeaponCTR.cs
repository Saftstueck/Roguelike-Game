using Godot;
using System;
using System.IO;

public partial class WeaponCTR : Sprite2D
{
    [ExportCategory("Placement")]
    [Export] public float DistanceFromPlayer { get; set; } = 16.0f;
    [Export] public Vector2 OrbitCenterOffset { get; set; } = Vector2.Zero;
    [Export] public float BaseScale { get; set; } = 0.6f;
    [Export] public float PositionSpeed { get; set; } = 25.0f;
    [Export] public float RotationSpeed { get; set; } = 25.0f;

    [ExportCategory("Input")]
    [Export] public string AttackAction { get; set; } = "weapon_swing";

    [ExportCategory("Melee")]
    [Export] public float MeleeDuration { get; set; } = 0.25f;
    [Export] public float MeleeCooldown { get; set; } = 0.2f;
    [Export] public float MeleeForwardDistance { get; set; } = 20.0f;
    [Export] public float MeleeRotationDegrees { get; set; } = -25.0f;
    [Export] public float MeleeScaleBoost { get; set; } = 0.08f;

    [ExportCategory("Bow")]
    [Export] public float BowFullDrawTime { get; set; } = 0.35f;

    [ExportCategory("Wand And Staff")]
    [Export] public float MagicChargeTime { get; set; } = 0.4f;
    [Export] public float MagicCooldown { get; set; } = 0.3f;
    [Export] public float MagicPullbackDistance { get; set; } = 10.0f;
    [Export] public float SpellSpawnDistance { get; set; } = 8.0f;
    [Export] public PackedScene MageSpellScene { get; set; }

    [ExportCategory("Movement Feeling")]
    [Export] public float MovementTiltDegrees { get; set; } = 10.0f;
    [Export] public float SpeedForMaximumTilt { get; set; } = 250.0f;
    [Export] public float MovementTiltSpeed { get; set; } = 12.0f;

    [ExportCategory("Rotation Offsets")]
    [Export] public float GlobalOffset { get; set; } = 0.0f;
    [Export] public bool ReverseOffsetWhenFlipped { get; set; } = true;
    [Export] public float DefaultOffset { get; set; } = 0.0f;
    [Export] public float SwordOffset { get; set; } = 135.0f;
    [Export] public float GreatSwordOffset { get; set; } = 135.0f;
    [Export] public float SpearOffset { get; set; } = 135.0f;
    [Export] public float BowOffset { get; set; } = 135.0f;
    [Export] public float WandOffset { get; set; } = 45.0f;
    [Export] public float StaffOffset { get; set; } = 45.0f;
    [Export] public float AxeOffset { get; set; } = 135.0f;
    [Export] public float SphereOffset { get; set; } = 230.0f;
    [Export] public float HammerOffset { get; set; } = 135.0f;
    [Export] public float ShieldOffset { get; set; } = 150.0f;
    [Export] public float DaggerOffset { get; set; } = 45.0f;
    [Export] public float CrossbowOffset { get; set; } = 0.0f;
    [Export] public float GunOffset { get; set; } = 0.0f;

    private Node2D player;

    private Texture2D lastTexture;
    private string weaponName = "";
    private float weaponOffset;

    private bool isBow;
    private bool isMagic;

    private float meleeTimer = -1.0f;
    private float meleeCooldownTimer = 999.0f;

    private bool isMagicCharging;
    private float magicCharge;
    private float magicCooldownTimer = 999.0f;

    private float bowTimer;
    private int bowFrame = 1;

    private float movementTilt;
    private Vector2 previousPlayerPosition;

    private string bowFolder = "";
    private readonly Texture2D[] bowTextures = new Texture2D[4];
    private readonly Texture2D[] bowNormals = new Texture2D[4];


    public override void _Ready()
    {
        player = GetParent() as Node2D;

        if (player == null)
        {
            GD.PushError(
                "Error idk."
            );

            SetProcess(false);
            return;
        }

        previousPlayerPosition = player.GlobalPosition;

        RefreshWeapon(true);

        Vector2 direction = GetAimDirection();

        Position =
            OrbitCenterOffset +
            direction * DistanceFromPlayer;

        Rotation = direction.Angle();
    }


    public override void _Process(double delta)
    {
        float dt = (float)delta;

        RefreshWeapon(false);

        Vector2 direction = GetAimDirection();
        Vector2 globalDirection = GetGlobalAimDirection();

        bool flipped = direction.X < 0.0f;

        float animationAmount = 0.0f;

        if (isBow)
        {
            UpdateBow(dt);
        }
        else if (isMagic)
        {
            UpdateMagic(dt, globalDirection);
        }
        else
        {
            animationAmount = UpdateMelee(dt);
        }

        UpdateMovementTilt(globalDirection, dt);

        float distanceChange = 0.0f;
        float attackRotation = 0.0f;
        float scaleBoost = 0.0f;

        if (isMagic)
        {
            distanceChange =
                -magicCharge * MagicPullbackDistance;
        }
        else if (!isBow)
        {
            distanceChange =
                animationAmount * MeleeForwardDistance;

            attackRotation =
                animationAmount * MeleeRotationDegrees *
                (flipped ? -1.0f : 1.0f);

            scaleBoost =
                animationAmount * MeleeScaleBoost;
        }

        Vector2 targetPosition =
            OrbitCenterOffset +
            direction *
            (DistanceFromPlayer + distanceChange);

        Position = Position.Lerp(
            targetPosition,
            Smooth(PositionSpeed, dt)
        );

        float offset = weaponOffset;

        if (flipped && ReverseOffsetWhenFlipped)
        {
            offset = -offset;
        }

        float targetRotation =
            direction.Angle() +
            movementTilt +
            Mathf.DegToRad(
                GlobalOffset +
                offset +
                attackRotation
            );

        Rotation = Mathf.LerpAngle(
            Rotation,
            targetRotation,
            Smooth(RotationSpeed, dt)
        );

        FlipV = flipped;

        float finalScale =
            BaseScale * (1.0f + scaleBoost);

        Scale = Vector2.One * finalScale;

        previousPlayerPosition = player.GlobalPosition;
    }


    private Vector2 GetAimDirection()
    {
        Vector2 mouseLocal =
            player.ToLocal(GetGlobalMousePosition());

        return mouseLocal.LengthSquared() > 0.0001f
            ? mouseLocal.Normalized()
            : Vector2.Right;
    }


    private Vector2 GetGlobalAimDirection()
    {
        Vector2 direction =
            GetGlobalMousePosition() -
            player.GlobalPosition;

        return direction.LengthSquared() > 0.0001f
            ? direction.Normalized()
            : Vector2.Right;
    }


    private float UpdateMelee(float dt)
    {
        meleeCooldownTimer += dt;

        if (Input.IsActionJustPressed(AttackAction) &&
            meleeTimer < 0.0f &&
            meleeCooldownTimer >= MeleeCooldown)
        {
            meleeTimer = 0.0f;
            meleeCooldownTimer = 0.0f;
        }

        if (meleeTimer < 0.0f)
        {
            return 0.0f;
        }

        meleeTimer += dt;

        float progress = Mathf.Clamp(
            meleeTimer /
            Mathf.Max(MeleeDuration, 0.001f),
            0.0f,
            1.0f
        );

        float amount = Mathf.Max(
            0.0f,
            Mathf.Sin(progress * Mathf.Pi)
        );

        if (progress >= 1.0f)
        {
            meleeTimer = -1.0f;
        }

        return amount;
    }


    private void UpdateBow(float dt)
    {
        bool held = Input.IsActionPressed(AttackAction);

        if (Input.IsActionJustPressed(AttackAction))
        {
            bowTimer = 0.0f;
            SetBowFrame(2);
        }

        if (held)
        {
            bowTimer += dt;

            if (bowFrame == 1)
            {
                SetBowFrame(2);
            }

            if (bowTimer >= BowFullDrawTime)
            {
                SetBowFrame(3);
            }
        }
        else
        {
            bowTimer = 0.0f;

            if (bowFrame != 1)
            {
                // Strzała
                SetBowFrame(1);
            }
        }
    }


    private void UpdateMagic(
        float dt,
        Vector2 globalDirection)
    {
        magicCooldownTimer += dt;

        if (Input.IsActionJustPressed(AttackAction) &&
            !isMagicCharging &&
            magicCooldownTimer >= MagicCooldown)
        {
            isMagicCharging = true;
            magicCharge = 0.0f;
            magicCooldownTimer = 0.0f;
        }

        if (!isMagicCharging)
        {
            return;
        }

        if (Input.IsActionPressed(AttackAction))
        {
            magicCharge = Mathf.MoveToward(
                magicCharge,
                1.0f,
                dt / Mathf.Max(MagicChargeTime, 0.001f)
            );

            return;
        }

        CastSpell(globalDirection);

        isMagicCharging = false;
        magicCharge = 0.0f;
    }


    private void CastSpell(Vector2 direction)
    {
        if (MageSpellScene == null)
        {
            GD.PushWarning(
                "MageSpellScene is not assigned."
            );

            return;
        }

        Node2D spell =
            MageSpellScene.Instantiate<Node2D>();

        GetTree().CurrentScene.AddChild(spell);

        spell.GlobalPosition =
            GlobalPosition +
            direction * SpellSpawnDistance;

        spell.GlobalRotation = direction.Angle();
    }


    private void UpdateMovementTilt(
        Vector2 aimDirection,
        float dt)
    {
        Vector2 velocity =
            (player.GlobalPosition -
             previousPlayerPosition) / dt;

        float sidewaysSpeed =
            aimDirection.X * velocity.Y -
            aimDirection.Y * velocity.X;

        float amount = SpeedForMaximumTilt <= 0.0f
            ? 0.0f
            : Mathf.Clamp(
                sidewaysSpeed / SpeedForMaximumTilt,
                -1.0f,
                1.0f
            );

        float targetTilt =
            -amount *
            Mathf.DegToRad(MovementTiltDegrees);

        movementTilt = Mathf.Lerp(
            movementTilt,
            targetTilt,
            Smooth(MovementTiltSpeed, dt)
        );
    }


    private void RefreshWeapon(bool force)
    {
        if (!force && Texture == lastTexture)
        {
            return;
        }

        lastTexture = Texture;

        Texture2D diffuse = GetDiffuseTexture();

        if (diffuse == null ||
            string.IsNullOrEmpty(diffuse.ResourcePath))
        {
            return;
        }

        string texturePath = diffuse.ResourcePath;

        weaponName =
            Path.GetFileNameWithoutExtension(texturePath);

        isBow = Has("Bow");
        isMagic = Has("Wand") || Has("Staff");
        weaponOffset = FindWeaponOffset();

        meleeTimer = -1.0f;
        isMagicCharging = false;
        magicCharge = 0.0f;

        if (isBow)
        {
            int slash = texturePath.LastIndexOf('/');

            if (slash >= 0)
            {
                LoadBowTextures(
                    texturePath.Substring(0, slash)
                );
            }

            bowFrame = weaponName.Equals(
                "Bow2",
                StringComparison.OrdinalIgnoreCase)
                ? 2
                : weaponName.Equals(
                    "Bow3",
                    StringComparison.OrdinalIgnoreCase)
                    ? 3
                    : 1;
        }
        else
        {
            bowFrame = 1;
            bowTimer = 0.0f;
        }
    }


    private Texture2D GetDiffuseTexture()
    {
        if (Texture is CanvasTexture canvasTexture &&
            canvasTexture.DiffuseTexture != null)
        {
            return canvasTexture.DiffuseTexture;
        }

        return Texture;
    }


    private void LoadBowTextures(string folder)
    {
        if (folder == bowFolder &&
            bowTextures[1] != null)
        {
            return;
        }

        bowFolder = folder;

        for (int frame = 1; frame <= 3; frame++)
        {
            bowTextures[frame] = LoadTexture(
                $"{folder}/Bow{frame}.png"
            );

            string normalName = frame == 1
                ? "Bow_n.png"
                : $"Bow_n{frame}.png";

            bowNormals[frame] = LoadTexture(
                $"{folder}/{normalName}"
            );
        }
    }


    private void SetBowFrame(int frame)
    {
        if (frame == bowFrame ||
            bowTextures[frame] == null)
        {
            return;
        }

        CanvasTexture canvasTexture = new()
        {
            DiffuseTexture = bowTextures[frame],
            NormalTexture = bowNormals[frame]
        };

        Texture = canvasTexture;
        lastTexture = Texture;
        bowFrame = frame;
    }


    private float FindWeaponOffset()
    {
        if (Has("Great Sword")) return GreatSwordOffset;
        if (Has("Crossbow")) return CrossbowOffset;
        if (Has("Sword")) return SwordOffset;
        if (Has("Spear")) return SpearOffset;
        if (Has("Bow")) return BowOffset;
        if (Has("Wand")) return WandOffset;
        if (Has("Staff")) return StaffOffset;
        if (Has("Axe")) return AxeOffset;
        if (Has("Sphere")) return SphereOffset;
        if (Has("Hammer")) return HammerOffset;
        if (Has("Shield")) return ShieldOffset;
        if (Has("Dagger")) return DaggerOffset;
        if (Has("Gun")) return GunOffset;

        return DefaultOffset;
    }


    private bool Has(string type)
    {
        return weaponName.Contains(
            type,
            StringComparison.OrdinalIgnoreCase
        );
    }


    private static Texture2D LoadTexture(string path)
    {
        return ResourceLoader.Exists(path)
            ? GD.Load<Texture2D>(path)
            : null;
    }


    private static float Smooth(
        float speed,
        float delta)
    {
        return 1.0f -
               Mathf.Exp(
                   -Mathf.Max(speed, 0.01f) *
                   delta
               );
    }
}