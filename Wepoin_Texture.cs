using Godot;
using System;
using System.IO;
using System.Text.RegularExpressions;

public partial class Wepoin_Texture : Node2D
{
    public void SetWeaponTexture(Texture2D weaponTexture)
    {
        Sprite2D sprite =
            GetNodeOrNull<Sprite2D>("Sprite2D");

        if (sprite == null)
        {
            sprite = new Sprite2D
            {
                Name = "Sprite2D"
            };

            AddChild(sprite);
        }

        Texture2D normalMap = FindNormalMap(weaponTexture);

        CanvasTexture canvasTexture = new()
        {
            DiffuseTexture = weaponTexture
        };

        if (normalMap != null)
        {
            canvasTexture.NormalTexture = normalMap;
        }
        else
        {
            GD.PushWarning(
                $"No normal map found for {weaponTexture.ResourcePath}"
            );
        }

        sprite.Texture = canvasTexture;
    }

    private Texture2D FindNormalMap(Texture2D weaponTexture)
    {
        string weaponPath = weaponTexture.ResourcePath;

        if (string.IsNullOrEmpty(weaponPath))
        {
            return null;
        }

        int slashPosition = weaponPath.LastIndexOf('/');

        if (slashPosition < 0)
        {
            return null;
        }

        string folderPath =
            weaponPath.Substring(0, slashPosition);

        string weaponFile =
            weaponPath.Substring(slashPosition + 1);

        string weaponName =
            Path.GetFileNameWithoutExtension(weaponFile);

        // Max ale z ciebie grubas
        if (weaponName.Equals(
                "Bow1",
                StringComparison.OrdinalIgnoreCase))
        {
            weaponName = "Bow";
        }

        string bestNormalPath = null;
        int longestMatchLength = -1;

        foreach (string file in
                 ResourceLoader.ListDirectory(folderPath))
        {
            if (!file.EndsWith(
                    ".png",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string normalName =
                Path.GetFileNameWithoutExtension(file);

            Match match = Regex.Match(
                normalName,
                @"^(.*)_n(\d*)$",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                continue;
            }

            string normalKey =
                match.Groups[1].Value +
                match.Groups[2].Value;

            bool matches =
                weaponName.Equals(
                    normalKey,
                    StringComparison.OrdinalIgnoreCase) ||
                weaponName.EndsWith(
                    " " + normalKey,
                    StringComparison.OrdinalIgnoreCase);

            if (matches &&
                normalKey.Length > longestMatchLength)
            {
                bestNormalPath = $"{folderPath}/{file}";
                longestMatchLength = normalKey.Length;
            }
        }

        if (bestNormalPath == null)
        {
            return null;
        }

        return GD.Load<Texture2D>(bestNormalPath);
    }
}