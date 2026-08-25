using Godot;

public partial class Max : MageSpell
{
    [Export]
    public float ExplosionDamageMultiplier { get; set; } = 5.0f;

    protected override void OnSpellCollision(
        KinematicCollision2D collision)
    {
        float explosionDamage =
            Damage * ExplosionDamageMultiplier;

        GD.Print(
            $"Fireball exploded for {explosionDamage} damage."
        );

        QueueFree();
    }
}