using System;

public class WeaponType

{
    public string Name {get; }
    public double PhysDamage {get; }
    public double MagicDamage {get; }

    public WeaponType(string name, double physDamage, double magicDamage)
    {
        Name = name;
        PhysDamage = physDamage;
        MagicDamage = magicDamage;
    }
}

public class Weapon
{
    public WeaponType Type { get; }
    public WeaponQuality Quality { get; }
    public WeaponMaterial Material { get; }

    public Weapon(WeaponType type)
    {
        Type = type;
        Quality = RandomQuality();
        Material = RandomMaterial();
    }
    private static WeaponQuality RandomQuality()
    {
        WeaponQuality[] values = Enum.GetValues<WeaponQuality>();
        return values[Random.Shared.Next(values.Length)];
    }

    private static WeaponMaterial RandomMaterial()
    {
         WeaponMaterial[] values = Enum.GetValues<WeaponMaterial>();
        return values[Random.Shared.Next(values.Length)];
    }
}


public static class WeaponTypes
{
    public static readonly WeaponType Sword =
        new("Sword", 2.0, 0.0);

    public static readonly WeaponType Dagger =
        new("Dagger", 1.5, 0.0);

    public static readonly WeaponType Bow =
        new("Bow", 2.0, 0.0);

    public static readonly WeaponType Crossbow =
        new("Crossbow", 2.5, 0.0);

    public static readonly WeaponType Gun =
        new("Gun", 3.5, 0.0);

    public static readonly WeaponType Wand =
        new("Wand", 0.0, 2.0);

    public static readonly WeaponType Staff =
        new("Staff", 0.0, 2.5);

    public static readonly WeaponType Hammer =
        new("Hammer", 3.0, 0.0);

    public static readonly WeaponType PaladinsForbiddenGun =
        new("Paladin's Forbidden Gun", 8.0, 0.5);
}

public enum WeaponQuality
{
    Broken,
    Worn,
    Bloodied,
    Cursed,
    Ancient,
    Divine,
    Artefact
}

public enum WeaponMaterial
{
    Wooden,
    Copper,
    Iron,
    Golden,
    Diamond,
}