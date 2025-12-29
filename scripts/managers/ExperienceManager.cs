using Godot;

public partial class ExperienceManager : Node
{
    [Signal] public delegate void ExpChangedEventHandler(int currentExp, int currentLevel, int expToNext);
    [Signal] public delegate void LevelUpEventHandler(int newLevel);

    public int CurrentExp { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    // Progi: przykład (krzywa rosnąca)
    // private int GetExpForLevel(int level) => 50 + (level - 1) * 30 + (level - 1) * (level - 1) * 10;
        //ZMIENIĆ NA KRZYWĄ
    private int GetExpForLevel(int level) => 1;

    public int ExpToNext => GetExpForLevel(CurrentLevel);

    public void ResetRun()
    {
        CurrentExp = 0;
        CurrentLevel = 1;
        EmitSignal(SignalName.ExpChanged, CurrentExp, CurrentLevel, ExpToNext);
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        CurrentExp += amount;

        while (CurrentExp >= ExpToNext)
        {
            CurrentExp -= ExpToNext;
            CurrentLevel++;
            EmitSignal(SignalName.LevelUp, CurrentLevel);
        }

        EmitSignal(SignalName.ExpChanged, CurrentExp, CurrentLevel, ExpToNext);
    }
}
