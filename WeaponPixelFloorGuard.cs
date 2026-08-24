using Godot;
using System.Collections.Generic;

public partial class WeaponPixelFloorGuard : Node2D
{
    [ExportCategory("Floor Detection")]

    // Select only the TileMap/floor physics layer.
    [Export(PropertyHint.Layers2DPhysics)]
    public uint FloorCollisionMask { get; set; } = 1;

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float AlphaThreshold { get; set; } = 0.1f;

    // How far the Weapon moves upward per correction.
    [Export]
    public float PushStep { get; set; } = 0.5f;

    // Maximum correction distance is PushStep × MaximumSteps.
    [Export]
    public int MaximumSteps { get; set; } = 64;


    private Sprite2D weapon;
    private Node2D player;

    private Texture2D cachedTexture;

    private readonly List<Vector2> outlinePixels = new();

    private PhysicsPointQueryParameters2D floorQuery;


    public override void _Ready()
    {
        weapon = GetParent() as Sprite2D;

        if (weapon == null)
        {
            GD.PushError(
                "PixelFloorGuard must be a child of the Weapon Sprite2D."
            );

            SetProcess(false);
            return;
        }

        player = weapon.GetParent() as Node2D;

        SetupFloorQuery();
    }


    public override void _Process(double delta)
    {
        PreventFloorClipping();
    }


    private void SetupFloorQuery()
    {
        floorQuery = new PhysicsPointQueryParameters2D
        {
            CollisionMask = FloorCollisionMask,
            CollideWithBodies = true,
            CollideWithAreas = false
        };

        // Do not detect the Player's own collision.
        if (player is CollisionObject2D playerCollider)
        {
            Godot.Collections.Array<Rid> exclusions = new();

            exclusions.Add(playerCollider.GetRid());

            floorQuery.Exclude = exclusions;
        }
    }


    private void PreventFloorClipping()
    {
        CacheOutlinePixels();

        if (outlinePixels.Count == 0)
        {
            return;
        }

        floorQuery.CollisionMask = FloorCollisionMask;

        for (int step = 0;
             step < MaximumSteps;
             step++)
        {
            if (!AnyVisiblePixelInsideFloor())
            {
                break;
            }

            // Moves Weapon, Attacks and CollisionShape2D together.
            weapon.GlobalPosition +=
                Vector2.Up * PushStep;
        }
    }


    private bool AnyVisiblePixelInsideFloor()
    {
        PhysicsDirectSpaceState2D space =
            GetWorld2D().DirectSpaceState;

        foreach (Vector2 outlinePixel in outlinePixels)
        {
            Vector2 localPixel = outlinePixel;

            // Sprite flipping does not modify its Node2D transform,
            // so the pixel position must be mirrored manually.
            if (weapon.FlipH)
            {
                localPixel.X = -localPixel.X;
            }

            if (weapon.FlipV)
            {
                localPixel.Y = -localPixel.Y;
            }

            localPixel += weapon.Offset;

            floorQuery.Position =
                weapon.ToGlobal(localPixel);

            if (space.IntersectPoint(
                    floorQuery,
                    1
                ).Count > 0)
            {
                return true;
            }
        }

        return false;
    }


    private void CacheOutlinePixels()
    {
        Texture2D diffuseTexture =
            GetDiffuseTexture();

        // Rebuild only when the equipped weapon changes.
        if (diffuseTexture == cachedTexture)
        {
            return;
        }

        cachedTexture = diffuseTexture;
        outlinePixels.Clear();

        if (diffuseTexture == null)
        {
            return;
        }

        Image image = diffuseTexture.GetImage();

        if (image == null)
        {
            return;
        }

        int width = image.GetWidth();
        int height = image.GetHeight();

        Vector2 origin = weapon.Centered
            ? new Vector2(width, height) / 2.0f
            : Vector2.Zero;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (IsTransparent(image, x, y))
                {
                    continue;
                }

                if (!IsOutlinePixel(image, x, y))
                {
                    continue;
                }

                outlinePixels.Add(
                    new Vector2(
                        x + 0.5f,
                        y + 0.5f
                    ) -
                    origin
                );
            }
        }
    }


    private Texture2D GetDiffuseTexture()
    {
        if (weapon.Texture is CanvasTexture canvasTexture &&
            canvasTexture.DiffuseTexture != null)
        {
            return canvasTexture.DiffuseTexture;
        }

        return weapon.Texture;
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

        return image.GetPixel(x, y).A <
               AlphaThreshold;
    }
}