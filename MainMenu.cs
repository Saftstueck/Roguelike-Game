using Godot;

public partial class MainMenu : Control
{
    [Export(PropertyHint.File)]
    private string GamePath = "res://Game.tscn";

    [Export(PropertyHint.File)]
    private string OptionsPath = "res://Options.tscn";

    [Export]
    private string startButtonPath = "StartButton";

    [Export]
    private string optionsButtonPath = "OptionsButton";

    public override void _Ready()
    {
        var startButton = GetNodeOrNull<Button>(startButtonPath);

        if (startButton == null)
        {
            // nie wiem jak to inaczej zrobić jak chcesz to zrub jakieś if not czy coś
        }
        else
        {
            startButton.Pressed += OnStartButtonPressed;
        }

        var optionsButton = GetNodeOrNull<Button>(optionsButtonPath);

        if (optionsButton == null)
        {
            // tu tak samo
        }
        else
        {
            optionsButton.Pressed += OnOptionsButtonPressed;
        }
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToFile(GamePath);
    }

    private void OnOptionsButtonPressed()
    {
        GetTree().ChangeSceneToFile(OptionsPath);
    }
}