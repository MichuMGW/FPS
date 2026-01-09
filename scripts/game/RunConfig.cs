using Godot;
using System;

public partial class RunConfig : Node
{
    public RunDifficulty Difficulty { get; private set; } = RunDifficulty.Normal;

    public void SetDifficulty(RunDifficulty d) => Difficulty = d;

    public void ResetToDefaults()
    {
        Difficulty = RunDifficulty.Normal;
    }

    public string GetDifficultyName() => Difficulty.ToString();

}
