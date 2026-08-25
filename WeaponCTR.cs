using Godot;
using System;
using System.IO;

public partial class WeaponCTR : Sprite2D
{
    private enum WeaponKind
    {
        Unknown,
        Sword,
        GreatSword,
        Spear,
        Bow,
        Wand,
        Staff,
        Axe,
        Sphere,
        Hammer,
        Shield,
        Arrow,
        Dagger,
        Crossbow,
        Gun
    }

    private static readonly PackedScene MageSpellsScene =
        ResourceLoader.Load<PackedScene>(
            "res://MageSpell.tscn"
        );

    [ExportCategory("Placement")]
    [Export] public float DistanceFromPlayer { get; set; } = 32.0f;
    [Export] public Vector2 OrbitCenterOffset { get; set; } = Vector2.Zero;
    [Export] public float BaseScale { get; set; } = 0.6f;
    [Export] public bool FlipOnLeft { get; set; } = true;
    [Export] public bool ReverseRotationOffsetWhenFlipped { get; set; } = true;

    [ExportCategory("Following")]
    [Export] public float PositionFollowSpeed { get; set; } = 25.0f;
    [Export] public float RotationFollowSpeed { get; set; } = 25.0f;
    [Export] public float ScaleFollowSpeed { get; set; } = 20.0f;

    [ExportCategory("Mouse Feeling")]
    [Export] public float AimLagStrength { get; set; } = 0.035f;
    [Export] public float MaximumAimLagDegrees { get; set; } = 15.0f;
    [Export] public float AimLagFollowSpeed { get; set; } = 15.0f;

    [ExportCategory("Movement Feeling")]
    [Export] public float MovementTiltDegrees { get; set; } = 12.0f;
    [Export] public float MovementSpeedForMaximumTilt { get; set; } = 250.0f;
    [Export] public float MovementTiltFollowSpeed { get; set; } = 12.0f;

    [ExportCategory("Input")]
    [Export] public string AttackAction { get; set; } = "weapon_swing";
    [Export] public bool RepeatMeleeWhileHeld { get; set; } = false;

    [ExportCategory("Melee")]
    [Export] public float SwingDuration { get; set; } = 0.25f;
    [Export] public float SwingCooldown { get; set; } = 0.2f;
    [Export] public float SwingForwardDistance { get; set; } = 20.0f;
    [Export] public float SwingRotationDegrees { get; set; } = -25.0f;
    [Export] public float SwingScaleBoost { get; set; } = 0.08f;
    [Export] public float SwingCurvePower { get; set; } = 1.0f;
    [Export] public bool ReverseSwingWhenFlipped { get; set; } = true;

    [ExportCategory("Bow")]
    [Export] public float BowFullDrawTime { get; set; } = 0.35f;
    [Export] public string BowProjectileName { get; set; } = "Arrow";
    [Export] public float BowProjectileSpawnDistance { get; set; } = 16.0f;

    [ExportCategory("Magic")]
    [Export] public string SelectedSpell { get; set; } = "Fireball";
    [Export] public float MagicChargeTime { get; set; } = 0.5f;
    [Export] public float MagicCooldown { get; set; } = 0.25f;
    [Export] public float MagicPullbackDistance { get; set; } = 12.0f;
    [Export] public float MagicPullbackCurvePower { get; set; } = 1.0f;
    [Export] public float MagicScaleBoost { get; set; } = 0.08f;
    [Export] public float SpellSpawnDistance { get; set; } = 16.0f;

    [ExportCategory("Rotation Offsets")]
    [Export] public float GlobalRotationOffset { get; set; } = 0.0f;
    [Export] public float DefaultRotationOffset { get; set; } = 0.0f;
    [Export] public float SwordRotationOffset { get; set; } = 135.0f;
    [Export] public float GreatSwordRotationOffset { get; set; } = 135.0f;
    [Export] public float SpearRotationOffset { get; set; } = 135.0f;
    [Export] public float BowRotationOffset { get; set; } = 135.0f;
    [Export] public float WandRotationOffset { get; set; } = 45.0f;
    [Export] public float StaffRotationOffset { get; set; } = 45.0f;
    [Export] public float AxeRotationOffset { get; set; } = 135.0f;
    [Export] public float SphereRotationOffset { get; set; } = 230.0f;
    [Export] public float HammerRotationOffset { get; set; } = 135.0f;
    [Export] public float ShieldRotationOffset { get; set; } = 150.0f;
    [Export] public float ArrowRotationOffset { get; set; } = 45.0f;
    [Export] public float DaggerRotationOffset { get; set; } = 45.0f;
    [Export] public float CrossbowRotationOffset { get; set; } = 0.0f;
    [Export] public float GunRotationOffset { get; set; } = 0.0f;

    private Node2D player;
    private Node mageSpellsLibrary;

    private WeaponKind weaponKind = WeaponKind.Unknown;
    private Texture2D previouslyCheckedTexture;
    private float textureRotationOffset;

    private Vector2 previousPlayerPosition;
    private float previousAimAngle;
    private float aimLag;
    private float movementTilt;

    private bool meleeSwinging;
    private float meleeTimer;
    private float meleeCooldownTimer = 999.0f;

    private bool magicCharging;
    private float magicCharge;
    private float magicCooldownTimer = 999.0f;

    private bool bowDrawing;
    private string bowFolder;
    private float bowHoldTimer;
    private int bowFrame = 1;

    private Texture2D bow1;
    private Texture2D bow2;
    private Texture2D bow3;
    private Texture2D bowNormal1;
    private Texture2D bowNormal2;
    private Texture2D bowNormal3;

    public override void _Ready()
    {
        player = GetParent() as Node2D;

        if (player == null)
        {
            SetProcess(false);
            return;
        }

        if (MageSpellsScene != null)
        {
            mageSpellsLibrary =
                MageSpellsScene.Instantiate();
        }

        Vector2 direction =
            player.ToLocal(
                GetGlobalMousePosition()
            ).Normalized();

        if (direction.LengthSquared() < 0.001f)
            direction = Vector2.Right;

        previousAimAngle = direction.Angle();
        previousPlayerPosition = player.GlobalPosition;

        UpdateWeaponInformation(true);

        Position =
            OrbitCenterOffset +
            direction * DistanceFromPlayer;

        Rotation =
            direction.Angle() +
            Mathf.DegToRad(
                GlobalRotationOffset +
                textureRotationOffset
            );

        Scale = Vector2.One * BaseScale;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (dt <= 0.0f || player == null)
            return;

        UpdateWeaponInformation(false);

        Vector2 mouseGlobal =
            GetGlobalMousePosition();

        Vector2 mouseLocal =
            player.ToLocal(mouseGlobal);

        Vector2 aimDirection =
            mouseLocal.LengthSquared() > 0.001f
                ? mouseLocal.Normalized()
                : Vector2.FromAngle(previousAimAngle);

        Vector2 globalDirection =
            mouseGlobal - player.GlobalPosition;

        if (globalDirection.LengthSquared() > 0.001f)
            globalDirection = globalDirection.Normalized();
        else
            globalDirection = Vector2.Right;

        float aimAngle = aimDirection.Angle();
        float animationAmount = 0.0f;

        if (weaponKind == WeaponKind.Bow)
        {
            CancelMelee();
            CancelMagic();

            UpdateBow(
                dt,
                globalDirection
            );
        }
        else if (IsMagicWeapon())
        {
            CancelMelee();
            CancelBow();

            animationAmount =
                UpdateMagic(
                    dt,
                    globalDirection
                );
        }
        else
        {
            CancelMagic();
            CancelBow();

            animationAmount =
                UpdateMelee(dt);
        }

        UpdateAimLag(aimAngle, dt);
        UpdateMovementTilt(globalDirection, dt);

        UpdateWeaponTransform(
            aimDirection,
            aimAngle,
            animationAmount,
            dt
        );

        previousAimAngle = aimAngle;
        previousPlayerPosition = player.GlobalPosition;
    }

    private float UpdateMelee(float dt)
    {
        meleeCooldownTimer += dt;

        bool attackRequested =
            RepeatMeleeWhileHeld
                ? Input.IsActionPressed(AttackAction)
                : Input.IsActionJustPressed(AttackAction);

        if (attackRequested &&
            !meleeSwinging &&
            meleeCooldownTimer >= SwingCooldown)
        {
            meleeSwinging = true;
            meleeTimer = 0.0f;
            meleeCooldownTimer = 0.0f;
        }

        if (!meleeSwinging)
            return 0.0f;

        meleeTimer += dt;

        float progress = Mathf.Clamp(
            meleeTimer /
            Mathf.Max(SwingDuration, 0.001f),
            0.0f,
            1.0f
        );

        float amount = Mathf.Pow(
            Mathf.Max(
                0.0f,
                Mathf.Sin(progress * Mathf.Pi)
            ),
            Mathf.Max(SwingCurvePower, 0.01f)
        );

        if (progress >= 1.0f)
        {
            meleeSwinging = false;
            meleeTimer = 0.0f;
        }

        return amount;
    }

    private void UpdateBow(
        float dt,
        Vector2 direction)
    {
        if (Input.IsActionJustPressed(AttackAction))
        {
            bowDrawing = true;
            bowHoldTimer = 0.0f;
            SetBowFrame(2);
        }

        if (bowDrawing &&
            Input.IsActionPressed(AttackAction))
        {
            bowHoldTimer += dt;

            if (bowHoldTimer >= BowFullDrawTime)
                SetBowFrame(3);
            else
                SetBowFrame(2);
        }

        if (bowDrawing &&
            Input.IsActionJustReleased(AttackAction))
        {
            float charge = Mathf.Clamp(
                bowHoldTimer /
                Mathf.Max(BowFullDrawTime, 0.001f),
                0.0f,
                1.0f
            );

            CastSpell(
                BowProjectileName,
                direction,
                charge,
                BowProjectileSpawnDistance
            );

            bowDrawing = false;
            bowHoldTimer = 0.0f;

            SetBowFrame(1);
        }

        if (!bowDrawing &&
            !Input.IsActionPressed(AttackAction))
        {
            SetBowFrame(1);
        }
    }

    private float UpdateMagic(
        float dt,
        Vector2 direction)
    {
        magicCooldownTimer += dt;

        if (Input.IsActionJustPressed(AttackAction) &&
            !magicCharging &&
            magicCooldownTimer >= MagicCooldown)
        {
            magicCharging = true;
            magicCharge = 0.0f;
        }

        if (!magicCharging)
            return 0.0f;

        if (Input.IsActionPressed(AttackAction))
        {
            magicCharge +=
                dt /
                Mathf.Max(
                    MagicChargeTime,
                    0.001f
                );

            magicCharge = Mathf.Clamp(
                magicCharge,
                0.0f,
                1.0f
            );
        }
        else
        {
            float releasedCharge = magicCharge;

            magicCharging = false;
            magicCharge = 0.0f;
            magicCooldownTimer = 0.0f;

            CastSelectedSpell(
                direction,
                releasedCharge
            );

            return 0.0f;
        }

        return Mathf.Pow(
            magicCharge,
            Mathf.Max(
                MagicPullbackCurvePower,
                0.01f
            )
        );
    }

    private void CastSelectedSpell(
        Vector2 direction,
        float charge)
    {
        CastSpell(
            SelectedSpell,
            direction,
            charge,
            SpellSpawnDistance
        );
    }

    private void CastSpell(
        string spellName,
        Vector2 direction,
        float charge,
        float spawnDistance)
    {
        if (mageSpellsLibrary == null)
            return;

        MageSpell template =
            mageSpellsLibrary.GetNodeOrNull<MageSpell>(
                new NodePath(spellName)
            );

        if (template == null)
            return;

        MageSpell spell =
            template.Duplicate() as MageSpell;

        if (spell == null)
            return;

        GetTree().CurrentScene.AddChild(spell);

        direction = direction.Normalized();

        if (direction.LengthSquared() < 0.001f)
            direction = Vector2.Right;

        spell.GlobalPosition =
            GlobalPosition +
            direction * spawnDistance;

        spell.Launch(
            direction,
            charge
        );
    }

    private void SetBowFrame(int frame)
    {
        if (frame == bowFrame)
            return;

        Texture2D diffuse;
        Texture2D normal;

        switch (frame)
        {
            case 2:
                diffuse = bow2;
                normal = bowNormal2;
                break;

            case 3:
                diffuse = bow3;
                normal = bowNormal3;
                break;

            default:
                frame = 1;
                diffuse = bow1;
                normal = bowNormal1;
                break;
        }

        if (diffuse == null)
            return;

        CanvasTexture canvasTexture = new()
        {
            DiffuseTexture = diffuse,
            NormalTexture = normal
        };

        Texture = canvasTexture;
        previouslyCheckedTexture = Texture;
        bowFrame = frame;
    }

    private void LoadBowTextures(string folder)
    {
        if (folder == bowFolder &&
            bow1 != null)
        {
            return;
        }

        bowFolder = folder;

        bow1 = LoadTexture(
            $"{folder}/Bow1.png"
        );

        bow2 = LoadTexture(
            $"{folder}/Bow2.png"
        );

        bow3 = LoadTexture(
            $"{folder}/Bow3.png"
        );

        bowNormal1 = LoadTexture(
            $"{folder}/Bow_n.png"
        );

        bowNormal2 = LoadTexture(
            $"{folder}/Bow_n2.png"
        );

        bowNormal3 = LoadTexture(
            $"{folder}/Bow_n3.png"
        );
    }

    private static Texture2D LoadTexture(
        string path)
    {
        if (!ResourceLoader.Exists(path))
            return null;

        return ResourceLoader.Load<Texture2D>(path);
    }

    private void UpdateWeaponTransform(
        Vector2 aimDirection,
        float aimAngle,
        float animationAmount,
        float dt)
    {
        bool flipped =
            FlipOnLeft &&
            aimDirection.X < 0.0f;

        FlipV = false;

        bool magic = IsMagicWeapon();
        bool bow =
            weaponKind == WeaponKind.Bow;

        float animationDistance = 0.0f;

        if (magic)
        {
            animationDistance =
                -animationAmount *
                MagicPullbackDistance;
        }
        else if (!bow)
        {
            animationDistance =
                animationAmount *
                SwingForwardDistance;
        }

        Vector2 targetPosition =
            OrbitCenterOffset +
            aimDirection *
            (
                DistanceFromPlayer +
                animationDistance
            );

        Position = Position.Lerp(
            targetPosition,
            SmoothWeight(
                PositionFollowSpeed,
                dt
            )
        );

        float rotationOffset =
            textureRotationOffset;

        if (flipped &&
            ReverseRotationOffsetWhenFlipped)
        {
            rotationOffset =
                -rotationOffset;
        }

        float swingRotation = 0.0f;

        if (!magic && !bow)
        {
            swingRotation =
                SwingRotationDegrees;

            if (flipped &&
                ReverseSwingWhenFlipped)
            {
                swingRotation =
                    -swingRotation;
            }
        }

        float targetRotation =
            aimAngle +
            Mathf.DegToRad(
                GlobalRotationOffset +
                rotationOffset +
                swingRotation *
                animationAmount
            ) +
            aimLag +
            movementTilt;

        Rotation = Mathf.LerpAngle(
            Rotation,
            targetRotation,
            SmoothWeight(
                RotationFollowSpeed,
                dt
            )
        );

        float scaleBoost = 0.0f;

        if (magic)
        {
            scaleBoost =
                MagicScaleBoost *
                animationAmount;
        }
        else if (!bow)
        {
            scaleBoost =
                SwingScaleBoost *
                animationAmount;
        }

        float finalSize =
            BaseScale *
            (1.0f + scaleBoost);

        Vector2 targetScale = new(
            finalSize,
            flipped
                ? -finalSize
                : finalSize
        );

        Scale = Scale.Lerp(
            targetScale,
            SmoothWeight(
                ScaleFollowSpeed,
                dt
            )
        );
    }

    private void UpdateAimLag(
        float aimAngle,
        float dt)
    {
        float difference =
            Mathf.Atan2(
                Mathf.Sin(
                    aimAngle -
                    previousAimAngle
                ),
                Mathf.Cos(
                    aimAngle -
                    previousAimAngle
                )
            );

        float angularSpeed =
            difference / dt;

        float maximumLag =
            Mathf.DegToRad(
                MaximumAimLagDegrees
            );

        float targetLag = Mathf.Clamp(
            -angularSpeed *
            AimLagStrength,
            -maximumLag,
            maximumLag
        );

        aimLag = Mathf.Lerp(
            aimLag,
            targetLag,
            SmoothWeight(
                AimLagFollowSpeed,
                dt
            )
        );
    }

    private void UpdateMovementTilt(
        Vector2 aimDirection,
        float dt)
    {
        Vector2 velocity =
            (
                player.GlobalPosition -
                previousPlayerPosition
            ) / dt;

        float sidewaysSpeed =
            aimDirection.X * velocity.Y -
            aimDirection.Y * velocity.X;

        float normalizedSpeed = 0.0f;

        if (MovementSpeedForMaximumTilt > 0.0f)
        {
            normalizedSpeed = Mathf.Clamp(
                sidewaysSpeed /
                MovementSpeedForMaximumTilt,
                -1.0f,
                1.0f
            );
        }

        float targetTilt =
            -normalizedSpeed *
            Mathf.DegToRad(
                MovementTiltDegrees
            );

        movementTilt = Mathf.Lerp(
            movementTilt,
            targetTilt,
            SmoothWeight(
                MovementTiltFollowSpeed,
                dt
            )
        );
    }

    private void UpdateWeaponInformation(
        bool force)
    {
        if (!force &&
            Texture == previouslyCheckedTexture)
        {
            return;
        }

        previouslyCheckedTexture = Texture;

        Texture2D diffuse =
            GetDiffuseTexture();

        if (diffuse == null ||
            string.IsNullOrEmpty(
                diffuse.ResourcePath
            ))
        {
            weaponKind =
                WeaponKind.Unknown;

            textureRotationOffset =
                DefaultRotationOffset;

            return;
        }

        string weaponName =
            Path.GetFileNameWithoutExtension(
                diffuse.ResourcePath
            );

        WeaponKind detected =
            DetectWeaponKind(weaponName);

        if (detected != weaponKind)
        {
            weaponKind = detected;

            CancelMelee();
            CancelMagic();
            CancelBow();

            meleeCooldownTimer = 999.0f;
            magicCooldownTimer = 999.0f;
        }

        textureRotationOffset =
            GetRotationOffset(weaponKind);

        if (weaponKind == WeaponKind.Bow)
        {
            int slash =
                diffuse.ResourcePath.LastIndexOf('/');

            if (slash >= 0)
            {
                string folder =
                    diffuse.ResourcePath.Substring(
                        0,
                        slash
                    );

                LoadBowTextures(folder);
            }

            bowFrame =
                DetectBowFrame(weaponName);
        }
    }

    private Texture2D GetDiffuseTexture()
    {
        if (Texture is CanvasTexture canvas &&
            canvas.DiffuseTexture != null)
        {
            return canvas.DiffuseTexture;
        }

        return Texture;
    }

    private static WeaponKind DetectWeaponKind(
        string name)
    {
        if (Contains(name, "Great Sword"))
            return WeaponKind.GreatSword;

        if (Contains(name, "Crossbow"))
            return WeaponKind.Crossbow;

        if (Contains(name, "Sword"))
            return WeaponKind.Sword;

        if (Contains(name, "Spear"))
            return WeaponKind.Spear;

        if (Contains(name, "Bow"))
            return WeaponKind.Bow;

        if (Contains(name, "Wand"))
            return WeaponKind.Wand;

        if (Contains(name, "Staff"))
            return WeaponKind.Staff;

        if (Contains(name, "Axe"))
            return WeaponKind.Axe;

        if (Contains(name, "Sphere"))
            return WeaponKind.Sphere;

        if (Contains(name, "Hammer"))
            return WeaponKind.Hammer;

        if (Contains(name, "Shield"))
            return WeaponKind.Shield;

        if (Contains(name, "Arrow"))
            return WeaponKind.Arrow;

        if (Contains(name, "Dagger"))
            return WeaponKind.Dagger;

        if (Contains(name, "Gun"))
            return WeaponKind.Gun;

        return WeaponKind.Unknown;
    }

    private float GetRotationOffset(
        WeaponKind kind)
    {
        return kind switch
        {
            WeaponKind.Sword =>
                SwordRotationOffset,

            WeaponKind.GreatSword =>
                GreatSwordRotationOffset,

            WeaponKind.Spear =>
                SpearRotationOffset,

            WeaponKind.Bow =>
                BowRotationOffset,

            WeaponKind.Wand =>
                WandRotationOffset,

            WeaponKind.Staff =>
                StaffRotationOffset,

            WeaponKind.Axe =>
                AxeRotationOffset,

            WeaponKind.Sphere =>
                SphereRotationOffset,

            WeaponKind.Hammer =>
                HammerRotationOffset,

            WeaponKind.Shield =>
                ShieldRotationOffset,

            WeaponKind.Arrow =>
                ArrowRotationOffset,

            WeaponKind.Dagger =>
                DaggerRotationOffset,

            WeaponKind.Crossbow =>
                CrossbowRotationOffset,

            WeaponKind.Gun =>
                GunRotationOffset,

            _ =>
                DefaultRotationOffset
        };
    }

    private static int DetectBowFrame(
        string name)
    {
        if (name.Equals(
                "Bow2",
                StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (name.Equals(
                "Bow3",
                StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return 1;
    }

    private static bool Contains(
        string name,
        string type)
    {
        return name.IndexOf(
            type,
            StringComparison.OrdinalIgnoreCase
        ) >= 0;
    }

    private bool IsMagicWeapon()
    {
        return
            weaponKind == WeaponKind.Wand ||
            weaponKind == WeaponKind.Staff;
    }

    private void CancelMelee()
    {
        meleeSwinging = false;
        meleeTimer = 0.0f;
    }

    private void CancelMagic()
    {
        magicCharging = false;
        magicCharge = 0.0f;
    }

    private void CancelBow()
    {
        bowDrawing = false;
        bowHoldTimer = 0.0f;
    }

    private static float SmoothWeight(
        float speed,
        float dt)
    {
        return 1.0f -
               Mathf.Exp(
                   -Mathf.Max(
                       speed,
                       0.01f
                   ) *
                   dt
               );
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(
                mageSpellsLibrary))
        {
            mageSpellsLibrary.Free();
        }

        mageSpellsLibrary = null;
    }
}