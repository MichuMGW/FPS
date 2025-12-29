using Godot;

public partial class GoldManager : Node
{
    [Signal] public delegate void GoldChangedEventHandler(int currentGold);

    public int CurrentGold { get; private set; }

    public void ResetRun()
    {
        CurrentGold = 0;
        EmitSignal(SignalName.GoldChanged, CurrentGold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        CurrentGold += amount;
        EmitSignal(SignalName.GoldChanged, CurrentGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentGold < amount) return false;

        CurrentGold -= amount;
        EmitSignal(SignalName.GoldChanged, CurrentGold);
        return true;
    }
}
