using Godot;
using System;

public partial class TrollArcher : CharacterBody3D
{
    //TESTOWE
    [Export] public Node3D Player;

    private PathfindComponent _pathfind;

    public override void _Ready()
    {
        _pathfind = GetNode<PathfindComponent>("PathfindComponent");

        if (Player == null)
            Player = GetTree().GetFirstNodeInGroup("player") as Node3D;

        _pathfind.Target = Player;
    }
    //KONIEC TEST
}
