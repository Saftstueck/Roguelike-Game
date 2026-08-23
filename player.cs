public abstract class Character
{
	public string Name {get; protected set; }
	public int MaxHp {get; protected set; }
	public int Hp {get; protected set; }

	public int Mana {get; protected set; }

	public double PhysDamageMultiplikator {get; protected set; }
	public double MagicDamageMultiplikator {get; protected set; }

	public double PhysResistanceMultiplikator {get; protected set; }
	public double MagicResistanceMultiplikator {get; protected set; }

	public Character(string name, int maxHp, int mana)
	{
		Name = name;
		MaxHp = maxHp;
		Hp = maxHp;
		Mana = mana;
	}
}

public class Wizard : Character
{
	public Wizard(): base ("Wizard", 70, 130)
	{
		PhysDamageMultiplikator = 0.9;
		MagicDamageMultiplikator = 1.1; 

		PhysResistanceMultiplikator = 0.9;
		MagicResistanceMultiplikator = 1.1;
	}
}


public class Alchemist : Character
{
	public Alchemist(): base ("Alchemist", 50, 200)
	{
		PhysDamageMultiplikator = 0.7;
		MagicDamageMultiplikator = 1.1; 

		PhysResistanceMultiplikator = 0.7;
		MagicResistanceMultiplikator = 1.3;
	}
}

public class Druid : Character
{
	public Druid(): base ("Druid", 80, 130)
	{
		PhysDamageMultiplikator = 0.4;
		MagicDamageMultiplikator = 1.2; 

		PhysResistanceMultiplikator = 0.6;
		MagicResistanceMultiplikator = 1.1;
	}
}
