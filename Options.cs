using Godot;

public partial class Options : Control
{

	[Export(PropertyHint.File)]
	private string MainMenuPath = "res://main-menu.tscn";


	[Export]
	private string mainmenuButtonPath = "MainMenuButton";

	public override void _Ready()
	{

		var mainmenuButton = GetNodeOrNull<Button>(mainmenuButtonPath);

		if (mainmenuButton == null)
		{
			//nie wiem jak inaczej to zrobić
		}
		else
		{
			mainmenuButton.Pressed += OnMainMenuButtonPressed;
		}
	}


	private void OnMainMenuButtonPressed()
	{
		GetTree().ChangeSceneToFile(MainMenuPath);
	}
}
