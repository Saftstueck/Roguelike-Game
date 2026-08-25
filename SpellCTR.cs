using Godot;
using System.Collections.Generic;

public partial class SpellCTR : Node
{
    private WeaponCTR weaponController;
    private int selectedNumber = 1;

    private readonly Dictionary<int, string> spells = new()
    {
        { 1, "Fireball" }, //Max dodawaj tutaj spele by można było je używać, dodam jeszcze drugie takie (np.UnlockedSpells) albo ty dodaj idk
        { 2, "Arrow" }, // ta strzała spell jest tylko testowa by wiedzieć czy zmienia spele jak chcesz to możesz ją usunąć
        { 3, "Max" }
    };

    public override void _Ready()
    {
        weaponController =
            GetTree().CurrentScene.FindChild(
                "Weapon",
                true,
                false
            ) as WeaponCTR;

        if (weaponController == null)
        {
            GD.PushError("Could not find WeaponCTR.");
            return;
        }

        weaponController.SelectedSpell = spells[selectedNumber];

        GD.Print(
            $"Spell {selectedNumber}: {weaponController.SelectedSpell}"
        );
    }

    public override void _Process(double delta)
    {
        if (weaponController == null)
            return;

        if (Input.IsActionJustPressed("spell_change"))
        {
            selectedNumber++;

            if (selectedNumber > spells.Count)
                selectedNumber = 1;

            weaponController.SelectedSpell =
                spells[selectedNumber];

            GD.Print(
                $"Spell {selectedNumber}: " +
                $"{weaponController.SelectedSpell}"
            );
        }
    }
}