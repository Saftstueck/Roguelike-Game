using Godot;
using System.Collections.Generic;

public partial class SpellCTR : Node
{
    private WeaponAttacks weaponAttacks;
    private int selectedNumber = 1;

    private readonly Dictionary<int, string> spells =
        new()
        {
            { 1, "Fireball" },
            { 2, "Arrow" },
            { 3, "Max" }
        };

    public override void _Ready()
    {
        weaponAttacks =
            GetTree()
                .CurrentScene
                .FindChild(
                    "WeaponAttacks",
                    true,
                    false
                ) as WeaponAttacks;
    }

    public override void _Process(double delta)
    {
        if (weaponAttacks == null)
            return;

        if (!Input.IsActionJustPressed(
                "spell_change"))
        {
            return;
        }

        selectedNumber++;

        if (selectedNumber > spells.Count)
            selectedNumber = 1;

        SelectSpell();
    }

    private void SelectSpell()
    {
        if (!spells.TryGetValue(
                selectedNumber,
                out string spellName))
        {
            return;
        }

        weaponAttacks.SelectedSpell =
            spellName;

        GD.Print(
            $"Spell {selectedNumber}: {spellName}"
        );
    }
}