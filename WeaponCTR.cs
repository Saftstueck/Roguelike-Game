using Godot;
using System;
using System.IO;

public partial class WeaponCTR : Sprite2D
{
    public enum WeaponKind
    {
        Other,
        Bow,
        Wand,
        Staff,
        Hammer,
        Shield
    }

    private WeaponKind currentKind;
    private Texture2D checkedTexture;

    private int bowFrame = 1;
    private string bowFolder;

    private readonly Texture2D[] bowTextures =
        new Texture2D[3];

    private readonly Texture2D[] bowNormals =
        new Texture2D[3];

    public WeaponKind CurrentKind
    {
        get
        {
            RefreshWeapon();
            return currentKind;
        }
    }

    public override void _Ready()
    {
        RefreshWeapon();
    }

    public void RefreshWeapon()
    {
        if (Texture == checkedTexture)
            return;

        checkedTexture = Texture;

        Texture2D diffuse =
            GetDiffuseTexture();

        if (diffuse == null ||
            string.IsNullOrEmpty(
                diffuse.ResourcePath))
        {
            currentKind = WeaponKind.Other;
            return;
        }

        string name =
            Path.GetFileNameWithoutExtension(
                diffuse.ResourcePath
            );

        if (Contains(name, "Bow"))
        {
            currentKind = WeaponKind.Bow;
            LoadBow(diffuse);

            bowFrame =
                Contains(name, "Bow2")
                    ? 2
                    : Contains(name, "Bow3")
                        ? 3
                        : 1;
        }
        else if (Contains(name, "Wand"))
        {
            currentKind = WeaponKind.Wand;
        }
        else if (Contains(name, "Staff"))
        {
            currentKind = WeaponKind.Staff;
        }
        else if (Contains(name, "Hammer"))
        {
            currentKind = WeaponKind.Hammer;
        }
        else if (Contains(name, "Shield"))
        {
            currentKind = WeaponKind.Shield;
        }
        else
        {
            currentKind = WeaponKind.Other;
        }
    }

    private void LoadBow(Texture2D diffuse)
    {
        int slash =
            diffuse.ResourcePath.LastIndexOf('/');

        if (slash < 0)
            return;

        string folder =
            diffuse.ResourcePath.Substring(
                0,
                slash
            );

        if (folder == bowFolder &&
            bowTextures[0] != null)
        {
            return;
        }

        bowFolder = folder;

        bowTextures[0] =
            LoadTexture($"{folder}/Bow1.png");

        bowTextures[1] =
            LoadTexture($"{folder}/Bow2.png");

        bowTextures[2] =
            LoadTexture($"{folder}/Bow3.png");

        bowNormals[0] =
            LoadTexture($"{folder}/Bow_n.png");

        bowNormals[1] =
            LoadTexture($"{folder}/Bow_n2.png");

        bowNormals[2] =
            LoadTexture($"{folder}/Bow_n3.png");
    }

    public void SetBowFrame(int frame)
    {
        RefreshWeapon();

        if (currentKind != WeaponKind.Bow ||
            frame < 1 ||
            frame > 3 ||
            frame == bowFrame)
        {
            return;
        }

        Texture2D diffuse =
            bowTextures[frame - 1];

        if (diffuse == null)
            return;

        Texture = new CanvasTexture
        {
            DiffuseTexture = diffuse,
            NormalTexture =
                bowNormals[frame - 1]
        };

        bowFrame = frame;
        checkedTexture = Texture;
    }

    public Texture2D GetDiffuseTexture()
    {
        if (Texture is CanvasTexture canvas &&
            canvas.DiffuseTexture != null)
        {
            return canvas.DiffuseTexture;
        }

        return Texture;
    }

    private static Texture2D LoadTexture(
        string path)
    {
        if (!ResourceLoader.Exists(path))
            return null;

        return ResourceLoader.Load<Texture2D>(
            path
        );
    }

    private static bool Contains(
        string name,
        string value)
    {
        return name.IndexOf(
            value,
            StringComparison.OrdinalIgnoreCase
        ) >= 0;
    }
}