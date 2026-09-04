using Godot;

public partial class DeathScreen : Control
{
    [Export(PropertyHint.File)]
    private string GamePath = "res://Game.tscn";

    [Export]
    private string restartButtonPath = "RestartButton";



    public override void _Ready()
    {
        var restartButton = GetNodeOrNull<Button>(restartButtonPath);

        if (restartButton == null)
        {
            // nie wiem jak to inaczej zrobić jak chcesz to zrub jakieś if not czy coś
        }
        else
        {
            restartButton.Pressed += OnRestartButtonPressed;
        }
	}
    
	private void OnRestartButtonPressed()
    {
        GetTree().ChangeSceneToFile(GamePath);
	}
}