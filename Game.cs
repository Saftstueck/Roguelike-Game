using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public partial class Game : Node2D
{
    private static readonly PackedScene MageSpellsScene =
        GD.Load<PackedScene>("res://MageSpells/MageSpell.tscn");

    private Node mageSpellsLibrary;

    [Export] public string SelectedSpell = "Fireball";
    [Export] public float SpellSpawnDistance = 16.0f;
    
    [Export(PropertyHint.Dir)]
    public string WeaponFolder { get; set; } =
        "res://textures/Weapons";

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("wepoin_equip"))
        {
            GiveRandomWeaponTexture();
        }
    }

    private void GiveRandomWeaponTexture()
    {
        List<string> weaponTextures = new();
        List<string> normalMaps = new();

        foreach (string file in ResourceLoader.ListDirectory(WeaponFolder))
        {
            if (!file.EndsWith(
                    ".png",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string textureName =
                Path.GetFileNameWithoutExtension(file);

            if (textureName.Contains(
                    "_n",
                    StringComparison.OrdinalIgnoreCase))
            {
                normalMaps.Add(file);
            }
            else
            {
                weaponTextures.Add(file);
            }
        }

        if (weaponTextures.Count == 0)
        {
            GD.PushError(
                $"No weapon textures found in {WeaponFolder}"
            );

            return;
        }

        int randomIndex =
            System.Random.Shared.Next(weaponTextures.Count);

        string diffuseFile = weaponTextures[randomIndex];

        Texture2D diffuseTexture = GD.Load<Texture2D>(
            MakeTexturePath(diffuseFile)
        );

        string normalFile = FindMatchingNormalMap(
            diffuseFile,
            normalMaps
        );

        CanvasTexture finalTexture = new()
        {
            DiffuseTexture = diffuseTexture
        };

        if (normalFile != null)
        {
            finalTexture.NormalTexture = GD.Load<Texture2D>(
                MakeTexturePath(normalFile)
            );
        }

        SetWeaponTexture(finalTexture);
    }

    private void SetWeaponTexture(Texture2D texture)
{
    Node weaponNode = GetTree().Root.FindChild(
        "Weapon",
        true,
        false
    );

    if (weaponNode == null)
    {
        GD.PushError("Could not find a node named Weapon.");
        return;
    }

    if (weaponNode is Sprite2D weaponSprite)
    {
        weaponSprite.Texture = texture;
        return;
    }

    GD.PushError(
        $"Weapon was found, but it is {weaponNode.GetType().Name}, not Sprite2D."
    );
}

    private string FindMatchingNormalMap(
        string diffuseFile,
        List<string> normalMaps)
    {
        string weaponName =
            Path.GetFileNameWithoutExtension(diffuseFile);

        if (weaponName.Equals(
                "Bow1",
                StringComparison.OrdinalIgnoreCase))
        {
            weaponName = "Bow";
        }

        string bestMatch = null;
        int longestMatchLength = -1;

        foreach (string normalFile in normalMaps)
        {
            string normalName =
                Path.GetFileNameWithoutExtension(normalFile);


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

            bool isMatchingNormal =
                weaponName.Equals(
                    normalKey,
                    StringComparison.OrdinalIgnoreCase) ||
                weaponName.EndsWith(
                    " " + normalKey,
                    StringComparison.OrdinalIgnoreCase);

            if (isMatchingNormal &&
                normalKey.Length > longestMatchLength)
            {
                bestMatch = normalFile;
                longestMatchLength = normalKey.Length;
            }
        }

        return bestMatch;
    }

    private string MakeTexturePath(string fileName)
    {
        return $"{WeaponFolder.TrimEnd('/')}/{fileName}";
    }
}