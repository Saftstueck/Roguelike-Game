using Godot;
using System.Collections.Generic;

public partial class WeaponMovement : Node2D
{
    [ExportCategory("Movement")]
    [Export]
    public float RotationOffset { get; set; } = 45.0f;

    [Export]
    public string BlockAction { get; set; } =
        "weapon_block";

    [Export]
    public float BlockRotationDegrees { get; set; } =
        -90.0f;

    [Export]
    public float MaximumRadius { get; set; } =
        30.0f;

    [Export]
    public bool FlipOnLeft { get; set; } = true;

    [ExportCategory("Melee")]
    [Export]
    public float SweepDuration { get; set; } =
        0.25f;

    [ExportCategory("Hammer")]
    [Export]
    public float HammerGravity { get; set; } =
        700.0f;

    [Export]
    public float HammerFollowStrength { get; set; } =
        18.0f;

    [Export]
    public float HammerDamping { get; set; } =
        5.0f;

    [Export]
    public float HammerMaximumAngularSpeed { get; set; } =
        8.0f;

    [Export]
    public float HammerVelocityRadiusGain { get; set; } =
        2.5f;

    [Export]
    public float HammerMaximumRadius { get; set; } =
        50.0f;

    [ExportCategory("Pixel Collision")]

    [Export(PropertyHint.Layers2DPhysics)]
    public uint WorldCollisionMask { get; set; } = 1;

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float AlphaThreshold { get; set; } = 0.1f;

    [Export(PropertyHint.Range, "1,10,0.5")]
    public float CollisionStep { get; set; } = 5.0f;

    [Export(PropertyHint.Range, "8,128,1")]
    public int MaximumPixelSamples { get; set; } = 32;

    public Vector2 AimDirection { get; private set; } =
        Vector2.Right;

    private Node2D player;
    private WeaponCTR weaponTexture;

    private Texture2D cachedTexture;

    private readonly List<Vector2> outlinePixels =
        new();

    private PhysicsPointQueryParameters2D worldQuery;
    private PhysicsDirectSpaceState2D physicsSpace;

    private bool sweepActive;
    private float sweepTimer;

    private bool hammerActive;
    private float hammerAngle;
    private float hammerRadius;
    private float hammerAngularVelocity;

    public override void _Ready()
    {
        player =
            GetParent()
                .GetNodeOrNull<Node2D>("Player");

        weaponTexture =
            GetNodeOrNull<WeaponCTR>("WeaponCTR");

        if (player == null ||
            weaponTexture == null)
        {
            GD.PushError(
                "WeaponMovement could not find Player or WeaponCTR."
            );

            SetPhysicsProcess(false);
            return;
        }

        worldQuery =
            new PhysicsPointQueryParameters2D
            {
                CollisionMask = WorldCollisionMask,
                CollideWithBodies = true,
                CollideWithAreas = false
            };

        if (player is CollisionObject2D playerBody)
        {
            Godot.Collections.Array<Rid> exclusions =
                new();

            exclusions.Add(playerBody.GetRid());

            worldQuery.Exclude = exclusions;
        }

        GlobalPosition =
            player.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null ||
            weaponTexture == null)
        {
            return;
        }

        float dt = (float)delta;
        physicsSpace =
            GetWorld2D().DirectSpaceState;

        WeaponCTR.WeaponKind kind =
            weaponTexture.CurrentKind;

        Vector2 mouse =
            GetGlobalMousePosition();

        Vector2 mouseOffset =
            mouse - player.GlobalPosition;

        float targetAngle =
            mouseOffset.LengthSquared() > 0.001f
                ? mouseOffset.Angle()
                : 0.0f;

        float mouseRadius =
            Mathf.Min(
                mouseOffset.Length(),
                MaximumRadius
            );

        AimDirection =
            Vector2.FromAngle(targetAngle);

        bool ranged =
            kind == WeaponCTR.WeaponKind.Bow ||
            kind == WeaponCTR.WeaponKind.Wand ||
            kind == WeaponCTR.WeaponKind.Staff;

        if (ranged)
        {
            sweepActive = false;
            sweepTimer = 0.0f;
        }

        float sweepAmount =
            UpdateSweep(dt);

        float radius =
            Mathf.Lerp(
                mouseRadius,
                MaximumRadius,
                sweepAmount
            );

        bool blocking =
            InputMap.HasAction(BlockAction) &&
            Input.IsActionPressed(BlockAction);

        float blockRotation = 0.0f;

        if (blocking &&
            kind != WeaponCTR.WeaponKind.Hammer &&
            kind != WeaponCTR.WeaponKind.Shield)
        {
            blockRotation =
                BlockRotationDegrees;
        }

        CacheOutlinePixels();

        if (kind == WeaponCTR.WeaponKind.Hammer)
        {
            UpdateHammer(
                dt,
                targetAngle,
                radius
            );
        }
        else
        {
            hammerActive = false;
            hammerAngularVelocity = 0.0f;

            bool facingLeft =
                SetWeaponFlip(targetAngle);

            float signedOffset =
                facingLeft
                    ? -RotationOffset
                    : RotationOffset;

            GlobalRotation =
                targetAngle +
                Mathf.DegToRad(
                    signedOffset +
                    blockRotation
                );

            PlaceAtPolarPosition(
                targetAngle,
                radius
            );
        }
    }

    public void StartSweep()
    {
        if (sweepActive)
            return;

        sweepActive = true;
        sweepTimer = 0.0f;
    }

    private float UpdateSweep(float delta)
    {
        if (!sweepActive)
            return 0.0f;

        sweepTimer += delta;

        float progress =
            Mathf.Clamp(
                sweepTimer /
                Mathf.Max(
                    SweepDuration,
                    0.001f
                ),
                0.0f,
                1.0f
            );

        float amount =
            Mathf.Sin(progress * Mathf.Pi);

        if (progress >= 1.0f)
        {
            sweepActive = false;
            sweepTimer = 0.0f;
        }

        return amount;
    }

    private void UpdateHammer(
        float delta,
        float targetAngle,
        float baseRadius)
    {
        if (!hammerActive)
        {
            hammerActive = true;
            hammerAngle = targetAngle;
            hammerRadius = baseRadius;
            hammerAngularVelocity = 0.0f;

            ApplyHammerPolarTransform(
                hammerAngle,
                hammerRadius
            );
        }

        float previousAngle =
            hammerAngle;

        float previousRadius =
            hammerRadius;

        float angleError =
            Mathf.AngleDifference(
                hammerAngle,
                targetAngle
            );

        float gravityRadius =
            Mathf.Max(
                hammerRadius,
                1.0f
            );

        float gravityAcceleration =
            Mathf.Cos(hammerAngle) *
            HammerGravity /
            gravityRadius;

        float angularAcceleration =
            angleError *
            HammerFollowStrength +
            gravityAcceleration -
            hammerAngularVelocity *
            HammerDamping;

        hammerAngularVelocity +=
            angularAcceleration *
            delta;

        hammerAngularVelocity =
            Mathf.Clamp(
                hammerAngularVelocity,
                -HammerMaximumAngularSpeed,
                HammerMaximumAngularSpeed
            );

        hammerAngle +=
            hammerAngularVelocity *
            delta;

        hammerAngle =
            Mathf.Wrap(
                hammerAngle,
                -Mathf.Pi,
                Mathf.Pi
            );

        float wantedRadius =
            baseRadius +
            Mathf.Abs(
                hammerAngularVelocity
            ) *
            HammerVelocityRadiusGain;

        wantedRadius =
            Mathf.Clamp(
                wantedRadius,
                0.0f,
                Mathf.Max(
                    HammerMaximumRadius,
                    MaximumRadius
                )
            );

        MoveHammerOnCircle(
            previousAngle,
            previousRadius,
            hammerAngle,
            wantedRadius
        );
    }

    private void MoveHammerOnCircle(
        float previousAngle,
        float previousRadius,
        float wantedAngle,
        float wantedRadius)
    {
        float angleChange =
            Mathf.AngleDifference(
                previousAngle,
                wantedAngle
            );

        float arcDistance =
            Mathf.Abs(angleChange) *
            Mathf.Max(
                previousRadius,
                wantedRadius
            );

        float radialDistance =
            Mathf.Abs(
                wantedRadius -
                previousRadius
            );

        float movementDistance =
            Mathf.Max(
                arcDistance,
                radialDistance
            );

        int steps =
            Mathf.CeilToInt(
                movementDistance /
                Mathf.Max(
                    CollisionStep,
                    0.5f
                )
            );

        if (steps < 1)
            steps = 1;

        float safeAngle =
            previousAngle;

        float safeRadius =
            previousRadius;

        for (int step = 1;
             step <= steps;
             step++)
        {
            float amount =
                (float)step / steps;

            float testAngle =
                previousAngle +
                angleChange * amount;

            float testRadius =
                Mathf.Lerp(
                    previousRadius,
                    wantedRadius,
                    amount
                );

            ApplyHammerPolarTransform(
                testAngle,
                testRadius
            );

            if (TouchesWorld())
            {
                hammerAngle = safeAngle;
                hammerRadius = safeRadius;
                hammerAngularVelocity = 0.0f;

                ApplyHammerPolarTransform(
                    hammerAngle,
                    hammerRadius
                );

                return;
            }

            safeAngle = testAngle;
            safeRadius = testRadius;
        }

        hammerAngle = wantedAngle;
        hammerRadius = wantedRadius;
    }

    private void ApplyHammerPolarTransform(
        float angle,
        float radius)
    {
        bool facingLeft =
            SetWeaponFlip(angle);

        float signedOffset =
            facingLeft
                ? -RotationOffset
                : RotationOffset;

        GlobalRotation =
            angle +
            Mathf.DegToRad(signedOffset);

        GlobalPosition =
            player.GlobalPosition +
            Vector2.FromAngle(angle) *
            radius;
    }

    private bool SetWeaponFlip(float angle)
    {
        weaponTexture.FlipH = false;
        weaponTexture.Rotation = 0.0f;

        bool facingLeft =
            FlipOnLeft &&
            Mathf.Cos(angle) < 0.0f;

        weaponTexture.FlipV =
            facingLeft;

        return facingLeft;
    }

    private void PlaceAtPolarPosition(
        float angle,
        float wantedRadius)
    {
        Vector2 radialDirection =
            Vector2.FromAngle(angle);

        wantedRadius =
            Mathf.Clamp(
                wantedRadius,
                0.0f,
                MaximumRadius
            );

        float safeRadius = 0.0f;

        GlobalPosition =
            player.GlobalPosition;

        if (wantedRadius <= 0.0f)
            return;

        int steps =
            Mathf.CeilToInt(
                wantedRadius /
                Mathf.Max(
                    CollisionStep,
                    0.5f
                )
            );

        if (steps < 1)
            steps = 1;

        for (int step = 1;
             step <= steps;
             step++)
        {
            float radius =
                wantedRadius *
                ((float)step / steps);

            GlobalPosition =
                player.GlobalPosition +
                radialDirection * radius;

            if (TouchesWorld())
            {
                GlobalPosition =
                    player.GlobalPosition +
                    radialDirection * safeRadius;

                return;
            }

            safeRadius = radius;
        }
    }

    private void CacheOutlinePixels()
    {
        Texture2D texture =
            weaponTexture.GetDiffuseTexture();

        if (texture == cachedTexture)
            return;

        cachedTexture = texture;
        outlinePixels.Clear();

        if (texture == null)
            return;

        Image image =
            texture.GetImage();

        if (image == null ||
            image.IsEmpty())
        {
            return;
        }

        if (image.IsCompressed())
            image.Decompress();

        int width =
            image.GetWidth();

        int height =
            image.GetHeight();

        Vector2 origin =
            weaponTexture.Centered
                ? new Vector2(
                    width,
                    height
                ) * 0.5f
                : Vector2.Zero;

        List<Vector2> completeOutline =
            new();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (IsTransparent(
                        image,
                        x,
                        y))
                {
                    continue;
                }

                if (!IsOutlinePixel(
                        image,
                        x,
                        y))
                {
                    continue;
                }

                completeOutline.Add(
                    new Vector2(
                        x + 0.5f,
                        y + 0.5f
                    ) -
                    origin
                );
            }
        }

        int sampleLimit =
            MaximumPixelSamples < 1
                ? 1
                : MaximumPixelSamples;

        int skip =
            Mathf.CeilToInt(
                (float)completeOutline.Count /
                sampleLimit
            );

        if (skip < 1)
            skip = 1;

        for (int i = 0;
             i < completeOutline.Count;
             i += skip)
        {
            outlinePixels.Add(
                completeOutline[i]
            );
        }
    }

    private bool IsOutlinePixel(
        Image image,
        int x,
        int y)
    {
        return
            IsTransparent(image, x - 1, y) ||
            IsTransparent(image, x + 1, y) ||
            IsTransparent(image, x, y - 1) ||
            IsTransparent(image, x, y + 1);
    }

    private bool IsTransparent(
        Image image,
        int x,
        int y)
    {
        if (x < 0 ||
            y < 0 ||
            x >= image.GetWidth() ||
            y >= image.GetHeight())
        {
            return true;
        }

        return
            image.GetPixel(x, y).A <
            AlphaThreshold;
    }

    private bool TouchesWorld()
    {
        if (worldQuery == null ||
            physicsSpace == null ||
            outlinePixels.Count == 0)
        {
            return false;
        }

        worldQuery.CollisionMask =
            WorldCollisionMask;

        foreach (Vector2 pixel
                 in outlinePixels)
        {
            Vector2 localPixel =
                pixel;

            if (weaponTexture.FlipV)
                localPixel.Y = -localPixel.Y;

            localPixel +=
                weaponTexture.Offset;

            worldQuery.Position =
                weaponTexture.ToGlobal(
                    localPixel
                );

            if (physicsSpace.IntersectPoint(
                    worldQuery,
                    1).Count > 0)
            {
                return true;
            }
        }

        return false;
    }
}
