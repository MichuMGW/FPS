using Godot;

public partial class GoldManager : Node
{
    [Signal] public delegate void GoldChangedEventHandler(int currentGold);

    public int CurrentGold { get; private set; }
    public int GoldGained { get; private set; }

    public override void _Ready()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        CurrentGold = 0;
        GoldGained = 0;
        EmitSignal(nameof(GoldChanged), CurrentGold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        CurrentGold += amount;
        GoldGained += amount;
        EmitSignal(nameof(GoldChanged), CurrentGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentGold < amount) return false;

        CurrentGold -= amount;
        EmitSignal(nameof(GoldChanged), CurrentGold);
        return true;
    }
}
