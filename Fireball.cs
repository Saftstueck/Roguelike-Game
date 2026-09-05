using Godot;

public partial class Fireball : MageSpell
{
    [Export]
    public float ExplosionDamageMultiplier { get; set; } = 1.5f;

    protected override void OnSpellCollision(
        KinematicCollision2D collision)
    {
        float explosionDamage =
            Damage * ExplosionDamageMultiplier;

        // tutaj dodaj później damage

        QueueFree();
    }
}