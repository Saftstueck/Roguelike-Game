using Godot;

public partial class Arrow : MageSpell
{
    [Export]
    public float StuckLifetime { get; set; } = 2.0f;

    protected override void OnSpellCollision(
        KinematicCollision2D collision)
    {
        StopMoving();
        RemainingLifetime = StuckLifetime;

       //dodać trzeba dmg system
    }
}