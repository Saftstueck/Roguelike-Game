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


    [ExportCategory("Placement")]

    [Export]
    public float DistanceFromPlayer { get; set; } = 32.0f;

    // Moves the center of the entire weapon orbit.
    [Export]
    public Vector2 OrbitCenterOffset { get; set; } =
        Vector2.Zero;

    [Export]
    public float BaseScale { get; set; } = 0.6f;

    [Export]
    public bool FlipVerticallyOnLeft { get; set; } = true;

    [Export]
    public bool ReverseTextureOffsetWhenFlipped { get; set; } =
        true;


    [ExportCategory("Weapon Rotation Offsets")]

    [Export]
    public float GlobalRotationOffsetDegrees { get; set; } =
        0.0f;

    [Export]
    public float DefaultRotationOffsetDegrees { get; set; } =
        0.0f;

    [Export]
    public float SwordRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float GreatSwordRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float SpearRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float BowRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float WandRotationOffsetDegrees { get; set; } =
        45.0f;

    [Export]
    public float StaffRotationOffsetDegrees { get; set; } =
        45.0f;

    [Export]
    public float AxeRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float SphereRotationOffsetDegrees { get; set; } =
        230.0f;

    [Export]
    public float HammerRotationOffsetDegrees { get; set; } =
        135.0f;

    [Export]
    public float ShieldRotationOffsetDegrees { get; set; } =
        150.0f;

    [Export]
    public float ArrowRotationOffsetDegrees { get; set; } =
        45.0f;

    [Export]
    public float DaggerRotationOffsetDegrees { get; set; } =
        45.0f;

    [Export]
    public float CrossbowRotationOffsetDegrees { get; set; } =
        0.0f;

    [Export]
    public float GunRotationOffsetDegrees { get; set; } =
        0.0f;


    [ExportCategory("Following")]

    [Export]
    public float PositionFollowSpeed { get; set; } = 25.0f;

    [Export]
    public float RotationFollowSpeed { get; set; } = 25.0f;

    [Export]
    public float ScaleFollowSpeed { get; set; } = 20.0f;


    [ExportCategory("Mouse Aim Feeling")]

    [Export]
    public float AimLagStrength { get; set; } = 0.035f;

    [Export]
    public float MaximumAimLagDegrees { get; set; } = 15.0f;

    [Export]
    public float AimLagFollowSpeed { get; set; } = 15.0f;


    [ExportCategory("Player Movement Feeling")]

    [Export]
    public float MovementTiltDegrees { get; set; } = 12.0f;

    [Export]
    public float MovementSpeedForMaximumTilt { get; set; } =
        250.0f;

    [Export]
    public float MovementTiltFollowSpeed { get; set; } = 12.0f;


    [ExportCategory("Input")]

    [Export]
    public string SwingAction { get; set; } =
        "weapon_swing";

    [Export]
    public bool RepeatMeleeWhileHeld { get; set; } = false;


    [ExportCategory("Melee Swing")]

    [Export]
    public float SwingDuration { get; set; } = 0.25f;

    [Export]
    public float SwingCooldown { get; set; } = 0.2f;

    [Export]
    public float SwingForwardDistance { get; set; } = 20.0f;

    [Export]
    public float SwingRotationDegrees { get; set; } = -25.0f;

    [Export]
    public bool ReverseSwingWhenFlipped { get; set; } = true;

    [Export]
    public float SwingScaleBoost { get; set; } = 0.08f;

    [Export]
    public float SwingCurvePower { get; set; } = 1.0f;


    [ExportCategory("Bow Drawing")]

    // Time between displaying Bow2 and Bow3.
    [Export]
    public float BowFullDrawTime { get; set; } = 0.35f;


    [ExportCategory("Wand And Staff Casting")]

    [Export]
    public float MagicMoveDuration { get; set; } = 0.3f;

    [Export]
    public float MagicCooldown { get; set; } = 0.25f;

    // How far the wand/staff moves toward the Player.
    [Export]
    public float MagicMoveTowardPlayerDistance { get; set; } =
        12.0f;

    [Export]
    public float MagicMovementCurvePower { get; set; } = 1.0f;

    [Export]
    public float MagicScaleBoost { get; set; } = 0.0f;


    private Node2D player;

    private WeaponKind currentWeaponKind =
        WeaponKind.Unknown;

    private Texture2D previouslyCheckedTexture;
    private float currentTextureRotationOffset;

    private float animationTimer;
    private float cooldownTimer;
    private bool isAnimating;

    private float previousAimAngle;
    private float aimLag;
    private float movementTilt;

    private Vector2 previousPlayerPosition;

    private string bowFolder;
    private Texture2D bow1Texture;
    private Texture2D bow2Texture;
    private Texture2D bow3Texture;

    private Texture2D bow1Normal;
    private Texture2D bow2Normal;
    private Texture2D bow3Normal;

    private int bowFrame = 1;
    private float bowHoldTimer;


    public override void _Ready()
    {
        player = GetParent() as Node2D;

        if (player == null)
        {
            GD.PushError(
                "Weapon must be a direct child of Player."
            );

            SetProcess(false);
            return;
        }

        Vector2 mouseLocal =
            player.ToLocal(GetGlobalMousePosition());

        if (mouseLocal.LengthSquared() < 0.0001f)
        {
            mouseLocal = Vector2.Right;
        }

        Vector2 initialDirection =
            mouseLocal.Normalized();

        previousAimAngle = initialDirection.Angle();
        previousPlayerPosition = player.GlobalPosition;

        cooldownTimer = 1000.0f;

        UpdateWeaponInformation(true);

        bool initiallyFlipped =
            FlipVerticallyOnLeft &&
            initialDirection.X < 0.0f;

        FlipV = initiallyFlipped;

        float initialRotationOffset =
            currentTextureRotationOffset;

        if (initiallyFlipped &&
            ReverseTextureOffsetWhenFlipped)
        {
            initialRotationOffset =
                -initialRotationOffset;
        }

        Position = GetWeaponPosition(
            initialDirection,
            0.0f
        );

        Rotation =
            previousAimAngle +
            Mathf.DegToRad(
                GlobalRotationOffsetDegrees +
                initialRotationOffset
            );

        Scale = Vector2.One * BaseScale;
    }


    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (dt <= 0.0f)
        {
            return;
        }

        UpdateWeaponInformation(false);

        Vector2 mouseGlobal = GetGlobalMousePosition();
        Vector2 mouseLocal = player.ToLocal(mouseGlobal);

        Vector2 aimDirection;

        if (mouseLocal.LengthSquared() > 0.0001f)
        {
            aimDirection = mouseLocal.Normalized();
        }
        else
        {
            aimDirection =
                Vector2.FromAngle(previousAimAngle);
        }

        Vector2 globalAimVector =
            mouseGlobal - player.GlobalPosition;

        Vector2 globalAimDirection =
            globalAimVector.LengthSquared() > 0.0001f
                ? globalAimVector.Normalized()
                : Vector2.Right;

        float aimAngle = aimDirection.Angle();
        float animationAmount = 0.0f;

        if (currentWeaponKind == WeaponKind.Bow)
        {
            UpdateBow(dt);
            StopNormalAnimation();
        }
        else
        {
            bool isMagicWeapon = IsMagicWeapon();

            UpdateNormalAnimationInput(
                dt,
                isMagicWeapon
            );

            animationAmount =
                UpdateNormalAnimation(
                    dt,
                    isMagicWeapon
                );
        }

        UpdateAimLag(aimAngle, dt);

        UpdateMovementTilt(
            globalAimDirection,
            dt
        );

        UpdateWeaponTransform(
            aimDirection,
            aimAngle,
            animationAmount,
            dt
        );

        previousAimAngle = aimAngle;
        previousPlayerPosition = player.GlobalPosition;
    }


    private void UpdateBow(float dt)
    {
        bool buttonHeld =
            Input.IsActionPressed(SwingAction);

        if (Input.IsActionJustPressed(SwingAction))
        {
            bowHoldTimer = 0.0f;
            SetBowFrame(2);
        }

        if (buttonHeld)
        {
            if (bowFrame == 1)
            {
                SetBowFrame(2);
            }

            bowHoldTimer += dt;

            if (bowHoldTimer >= BowFullDrawTime)
            {
                SetBowFrame(3);
            }
        }
        else
        {
            bowHoldTimer = 0.0f;

            if (bowFrame != 1)
            {
                // Arrow shooting can be added here later.
                SetBowFrame(1);
            }
        }
    }


    private void SetBowFrame(int frame)
    {
        if (bowFrame == frame)
        {
            return;
        }

        Texture2D diffuseTexture;
        Texture2D normalTexture;

        switch (frame)
        {
            case 2:
                diffuseTexture = bow2Texture;
                normalTexture = bow2Normal;
                break;

            case 3:
                diffuseTexture = bow3Texture;
                normalTexture = bow3Normal;
                break;

            default:
                frame = 1;
                diffuseTexture = bow1Texture;
                normalTexture = bow1Normal;
                break;
        }

        if (diffuseTexture == null)
        {
            GD.PushWarning(
                $"Bow{frame}.png could not be loaded."
            );

            return;
        }

        CanvasTexture canvasTexture = new()
        {
            DiffuseTexture = diffuseTexture
        };

        if (normalTexture != null)
        {
            canvasTexture.NormalTexture =
                normalTexture;
        }

        Texture = canvasTexture;
        previouslyCheckedTexture = Texture;
        bowFrame = frame;
    }


    private void LoadBowTextures(string folder)
    {
        if (folder == bowFolder &&
            bow1Texture != null)
        {
            return;
        }

        bowFolder = folder;

        bow1Texture = LoadTexture(
            $"{folder}/Bow1.png"
        );

        bow2Texture = LoadTexture(
            $"{folder}/Bow2.png"
        );

        bow3Texture = LoadTexture(
            $"{folder}/Bow3.png"
        );

        bow1Normal = LoadTexture(
            $"{folder}/Bow_n.png"
        );

        bow2Normal = LoadTexture(
            $"{folder}/Bow_n2.png"
        );

        bow3Normal = LoadTexture(
            $"{folder}/Bow_n3.png"
        );
    }


    private static Texture2D LoadTexture(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        return GD.Load<Texture2D>(path);
    }


    private void UpdateNormalAnimationInput(
        float dt,
        bool isMagicWeapon)
    {
        cooldownTimer += dt;

        float requiredCooldown = isMagicWeapon
            ? MagicCooldown
            : SwingCooldown;

        bool attackRequested = RepeatMeleeWhileHeld
            ? Input.IsActionPressed(SwingAction)
            : Input.IsActionJustPressed(SwingAction);

        if (attackRequested &&
            !isAnimating &&
            cooldownTimer >= requiredCooldown)
        {
            isAnimating = true;
            animationTimer = 0.0f;
            cooldownTimer = 0.0f;
        }
    }


    private float UpdateNormalAnimation(
        float dt,
        bool isMagicWeapon)
    {
        if (!isAnimating)
        {
            return 0.0f;
        }

        animationTimer += dt;

        float duration = isMagicWeapon
            ? MagicMoveDuration
            : SwingDuration;

        float curvePower = isMagicWeapon
            ? MagicMovementCurvePower
            : SwingCurvePower;

        duration = Mathf.Max(duration, 0.001f);

        float progress = Mathf.Clamp(
            animationTimer / duration,
            0.0f,
            1.0f
        );

        float sineCurve = Mathf.Max(
            0.0f,
            Mathf.Sin(progress * Mathf.Pi)
        );

        float animationAmount = Mathf.Pow(
            sineCurve,
            Mathf.Max(curvePower, 0.01f)
        );

        if (progress >= 1.0f)
        {
            isAnimating = false;
            animationTimer = 0.0f;
        }

        return animationAmount;
    }


    private void StopNormalAnimation()
    {
        isAnimating = false;
        animationTimer = 0.0f;
    }


    private void UpdateAimLag(
        float aimAngle,
        float dt)
    {
        float difference = GetAngleDifference(
            previousAimAngle,
            aimAngle
        );

        float angularVelocity =
            difference / dt;

        float maximumLag =
            Mathf.DegToRad(MaximumAimLagDegrees);

        float targetLag = Mathf.Clamp(
            -angularVelocity * AimLagStrength,
            -maximumLag,
            maximumLag
        );

        aimLag = Mathf.Lerp(
            aimLag,
            targetLag,
            GetSmoothWeight(AimLagFollowSpeed, dt)
        );
    }


    private void UpdateMovementTilt(
        Vector2 aimDirection,
        float dt)
    {
        Vector2 playerVelocity =
            (player.GlobalPosition -
             previousPlayerPosition) / dt;

        float sidewaysSpeed =
            aimDirection.X * playerVelocity.Y -
            aimDirection.Y * playerVelocity.X;

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
            Mathf.DegToRad(MovementTiltDegrees);

        movementTilt = Mathf.Lerp(
            movementTilt,
            targetTilt,
            GetSmoothWeight(
                MovementTiltFollowSpeed,
                dt
            )
        );
    }


    private void UpdateWeaponTransform(
        Vector2 aimDirection,
        float aimAngle,
        float animationAmount,
        float dt)
    {
        bool isFlipped =
            FlipVerticallyOnLeft &&
            aimDirection.X < 0.0f;

        FlipV = false;

			float scaleY = isFlipped
 	 		  ? -BaseScale
 	  		 : BaseScale;

		Scale = new Vector2(BaseScale, scaleY);

        bool isMagicWeapon = IsMagicWeapon();
        bool isBow =
            currentWeaponKind == WeaponKind.Bow;

        float positionAnimationDistance = 0.0f;

        if (isMagicWeapon)
        {
            // Wand/staff move toward the Player and back.
            positionAnimationDistance =
                -animationAmount *
                MagicMoveTowardPlayerDistance;
        }
        else if (!isBow)
        {
            // Normal melee weapons move toward the mouse.
            positionAnimationDistance =
                animationAmount *
                SwingForwardDistance;
        }

        Vector2 targetPosition = GetWeaponPosition(
            aimDirection,
            positionAnimationDistance
        );

        Position = Position.Lerp(
            targetPosition,
            GetSmoothWeight(PositionFollowSpeed, dt)
        );

        float weaponRotationOffset =
            currentTextureRotationOffset;

        if (isFlipped &&
            ReverseTextureOffsetWhenFlipped)
        {
            weaponRotationOffset =
                -weaponRotationOffset;
        }

        float attackRotation = 0.0f;

        if (!isMagicWeapon && !isBow)
        {
            attackRotation =
                SwingRotationDegrees;

            if (isFlipped &&
                ReverseSwingWhenFlipped)
            {
                attackRotation =
                    -attackRotation;
            }
        }

        float targetRotation =
            aimAngle +
            Mathf.DegToRad(
                GlobalRotationOffsetDegrees +
                weaponRotationOffset
            ) +
            aimLag +
            movementTilt +
            Mathf.DegToRad(attackRotation) *
            animationAmount;

        Rotation = Mathf.LerpAngle(
            Rotation,
            targetRotation,
            GetSmoothWeight(RotationFollowSpeed, dt)
        );

        float scaleBoost = 0.0f;

        if (isMagicWeapon)
        {
            scaleBoost =
                MagicScaleBoost * animationAmount;
        }
        else if (!isBow)
        {
            scaleBoost =
                SwingScaleBoost * animationAmount;
        }

        Vector2 targetScale =
            Vector2.One *
            BaseScale *
            (1.0f + scaleBoost);

        Scale = Scale.Lerp(
            targetScale,
            GetSmoothWeight(ScaleFollowSpeed, dt)
        );
    }


    private Vector2 GetWeaponPosition(
        Vector2 aimDirection,
        float animationDistance)
    {
        return OrbitCenterOffset +
               aimDirection *
               (DistanceFromPlayer +
                animationDistance);
    }


    private bool IsMagicWeapon()
    {
        return
            currentWeaponKind == WeaponKind.Wand ||
            currentWeaponKind == WeaponKind.Staff;
    }


    private void UpdateWeaponInformation(
        bool forceUpdate)
    {
        if (!forceUpdate &&
            Texture == previouslyCheckedTexture)
        {
            return;
        }

        previouslyCheckedTexture = Texture;

        Texture2D diffuseTexture =
            GetDiffuseTexture();

        if (diffuseTexture == null ||
            string.IsNullOrEmpty(
                diffuseTexture.ResourcePath))
        {
            currentWeaponKind =
                WeaponKind.Unknown;

            currentTextureRotationOffset =
                DefaultRotationOffsetDegrees;

            return;
        }

        string resourcePath =
            diffuseTexture.ResourcePath;

        string weaponName =
            Path.GetFileNameWithoutExtension(
                resourcePath
            );

        WeaponKind detectedKind =
            DetectWeaponKind(weaponName);

        if (detectedKind != currentWeaponKind)
        {
            currentWeaponKind = detectedKind;

            isAnimating = false;
            animationTimer = 0.0f;
            cooldownTimer = 1000.0f;

            if (currentWeaponKind != WeaponKind.Bow)
            {
                bowFrame = 1;
                bowHoldTimer = 0.0f;
            }
        }

        currentTextureRotationOffset =
            GetRotationOffset(currentWeaponKind);

        if (currentWeaponKind == WeaponKind.Bow)
        {
            int slashPosition =
                resourcePath.LastIndexOf('/');

            if (slashPosition >= 0)
            {
                string folder =
                    resourcePath.Substring(
                        0,
                        slashPosition
                    );

                LoadBowTextures(folder);
            }

            bowFrame = DetectBowFrame(weaponName);
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


    private static int DetectBowFrame(
        string weaponName)
    {
        if (weaponName.Equals(
                "Bow2",
                StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (weaponName.Equals(
                "Bow3",
                StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return 1;
    }


    private static WeaponKind DetectWeaponKind(
        string weaponName)
    {
        if (ContainsWeaponType(
                weaponName,
                "Great Sword"))
        {
            return WeaponKind.GreatSword;
        }

        if (ContainsWeaponType(
                weaponName,
                "Crossbow"))
        {
            return WeaponKind.Crossbow;
        }

        if (ContainsWeaponType(weaponName, "Sword"))
        {
            return WeaponKind.Sword;
        }

        if (ContainsWeaponType(weaponName, "Spear"))
        {
            return WeaponKind.Spear;
        }

        if (ContainsWeaponType(weaponName, "Bow"))
        {
            return WeaponKind.Bow;
        }

        if (ContainsWeaponType(weaponName, "Wand"))
        {
            return WeaponKind.Wand;
        }

        if (ContainsWeaponType(weaponName, "Staff"))
        {
            return WeaponKind.Staff;
        }

        if (ContainsWeaponType(weaponName, "Axe"))
        {
            return WeaponKind.Axe;
        }

        if (ContainsWeaponType(weaponName, "Sphere"))
        {
            return WeaponKind.Sphere;
        }

        if (ContainsWeaponType(weaponName, "Hammer"))
        {
            return WeaponKind.Hammer;
        }

        if (ContainsWeaponType(weaponName, "Shield"))
        {
            return WeaponKind.Shield;
        }

        if (ContainsWeaponType(weaponName, "Arrow"))
        {
            return WeaponKind.Arrow;
        }

        if (ContainsWeaponType(weaponName, "Dagger"))
        {
            return WeaponKind.Dagger;
        }

        if (ContainsWeaponType(weaponName, "Gun"))
        {
            return WeaponKind.Gun;
        }

        return WeaponKind.Unknown;
    }


    private float GetRotationOffset(
        WeaponKind kind)
    {
        switch (kind)
        {
            case WeaponKind.Sword:
                return SwordRotationOffsetDegrees;

            case WeaponKind.GreatSword:
                return GreatSwordRotationOffsetDegrees;

            case WeaponKind.Spear:
                return SpearRotationOffsetDegrees;

            case WeaponKind.Bow:
                return BowRotationOffsetDegrees;

            case WeaponKind.Wand:
                return WandRotationOffsetDegrees;

            case WeaponKind.Staff:
                return StaffRotationOffsetDegrees;

            case WeaponKind.Axe:
                return AxeRotationOffsetDegrees;

            case WeaponKind.Sphere:
                return SphereRotationOffsetDegrees;

            case WeaponKind.Hammer:
                return HammerRotationOffsetDegrees;

            case WeaponKind.Shield:
                return ShieldRotationOffsetDegrees;

            case WeaponKind.Arrow:
                return ArrowRotationOffsetDegrees;

            case WeaponKind.Dagger:
                return DaggerRotationOffsetDegrees;

            case WeaponKind.Crossbow:
                return CrossbowRotationOffsetDegrees;

            case WeaponKind.Gun:
                return GunRotationOffsetDegrees;

            default:
                return DefaultRotationOffsetDegrees;
        }
    }


    private static bool ContainsWeaponType(
        string weaponName,
        string weaponType)
    {
        return weaponName.IndexOf(
            weaponType,
            StringComparison.OrdinalIgnoreCase
        ) >= 0;
    }


    private static float GetAngleDifference(
        float from,
        float to)
    {
        return Mathf.Atan2(
            Mathf.Sin(to - from),
            Mathf.Cos(to - from)
        );
    }


    private static float GetSmoothWeight(
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