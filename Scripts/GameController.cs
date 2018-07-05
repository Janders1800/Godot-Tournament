using Godot;
using System;

public class GameController : Spatial
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
            GetTree().Quit();
        }

        if (Input.IsActionJustPressed("restart"))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
