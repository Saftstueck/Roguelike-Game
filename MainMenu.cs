using Godot;

public partial class MainMenu : Control
{
    [Export(PropertyHint.File)]
    private string GamePath = "res://Game.tscn";

    [Export]
    private string startButtonPath = "StartButton";

    public override void _Ready()
    {
        var button = GetNodeOrNull<Button>(startButtonPath);
        if (button == null)
        {
            GD.PushWarning($"MainMenu: Button '{startButtonPath}' not found. Add a Button node with that name or change startButtonPath.");
            return;
        }

        button.Pressed += OnStartButtonPressed;
    }

    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToFile(GamePath);
    }
}
