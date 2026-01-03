using Godot;
using System;

public partial class ManaComponent : Node
{
    [Signal] public delegate void ManaChangedEventHandler(float current, float max);

    [Export] public PlayerStatsManager Stats; // podepnij w Inspectorze albo z kodu
    [Export] public float StartFill01 = 1.0f; // 1 = start z pe³n¹ man¹

    private GameEvents _events;

    public float CurrentMana { get; private set; }

    public float MaxMana => Stats != null ? Mathf.Max(0f, Stats.GetStat(StatId.MaxMana)) : 0f;

    // anty-spam na error (np. trzymasz LMB i co frame “brak many”)
    private double _lastErrorTime = -999.0;
    private const double ErrorCooldown = 0.35;

    public override void _Ready()
    {
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        if (_events == null)
            GD.PushWarning("[ManaComponent] Missing GameEvents autoload.");

        if (Stats == null)
            GD.PushWarning("[ManaComponent] Stats is not assigned.");

        // init current mana
        CurrentMana = MaxMana * Mathf.Clamp(StartFill01, 0f, 1f);

        if (Stats != null)
            Stats.StatChanged += OnStatChanged;

        EmitManaChanged();
    }

    public override void _ExitTree()
    {
        if (Stats != null)
            Stats.StatChanged -= OnStatChanged;
    }

    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        if ((StatId)statId != StatId.MaxMana) return;

        // clamp current to new max
        float max = MaxMana;
        if (CurrentMana > max) CurrentMana = max;
        EmitManaChanged();
    }

    public bool CanAfford(float cost) => cost <= 0f || CurrentMana + 0.0001f >= cost;

    public bool TrySpend(float cost, bool showError = true)
    {
        if (cost <= 0f) return true;

        if (!CanAfford(cost))
        {
            if (showError)
                ShowNotEnoughMana();
            return false;
        }

        CurrentMana = Mathf.Max(0f, CurrentMana - cost);
        EmitManaChanged();
        return true;
    }

    public void Restore(float amount)
    {
        if (amount <= 0f) return;
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
        EmitManaChanged();
    }

    public void SetToFull()
    {
        CurrentMana = MaxMana;
        EmitManaChanged();
    }

    private void EmitManaChanged()
    {
        EmitSignal(nameof(ManaChanged), CurrentMana, MaxMana);
    }

    private void ShowNotEnoughMana()
    {
        double now = Time.GetUnixTimeFromSystem();
        if (now - _lastErrorTime < ErrorCooldown)
            return;

        _lastErrorTime = now;
        _events?.EmitShowError("Brak many", 1.0f);
    }
}
