using Godot;
using System;

public partial class PlayerManaComponent : Node
{
    [Signal] public delegate void ManaChangedEventHandler(float current, float max);

    [Export] public PlayerStatsManager Stats;
    [Export] public float StartFill01 = 1.0f;

    private GameEvents _events;

    public float CurrentMana { get; private set; }
    public float MaxMana => Stats != null ? Mathf.Max(0f, Stats.GetStat(StatId.MaxMana)) : 0f;
    public float ManaRegenPerSec => Stats != null ? Mathf.Max(0f, Stats.GetStat(StatId.ManaRegen)) : 0f;

    // odśwież UI tylko jak faktycznie zmieniłeś manę (żeby nie spamować sygnałami)
    private const float EmitEpsilon = 0.001f;

    // anty-spam na error
    private double _lastErrorTime = -999.0;
    private const double ErrorCooldown = 0.35;

    public override void _Ready()
    {
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        if (_events == null)
            GD.PushWarning("[PlayerManaComponent] Missing GameEvents autoload.");

        if (Stats == null)
            GD.PushWarning("[PlayerManaComponent] Stats is not assigned.");

        CurrentMana = MaxMana * Mathf.Clamp(StartFill01, 0f, 1f);

        if (Stats != null)
            Stats.StatChanged += OnStatChanged;

        SetProcess(true);
        EmitManaChanged();
    }

    public override void _ExitTree()
    {
        if (Stats != null)
            Stats.StatChanged -= OnStatChanged;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            GD.Print($"ManaRegen={ManaRegenPerSec} MaxMana={MaxMana} dt={(float)delta} timescale={Engine.TimeScale}");
        }

        if (Stats == null) return;

        float regen = ManaRegenPerSec;
        if (regen <= 0f) return;

        float max = MaxMana;
        if (max <= 0f) return;
        if (CurrentMana >= max - EmitEpsilon) return;

        float old = CurrentMana;
        CurrentMana = Mathf.Min(max, CurrentMana + regen * (float)delta);

        if (Mathf.Abs(CurrentMana - old) > EmitEpsilon)
            EmitManaChanged();
    }

    private void OnStatChanged(int statId, float newValue, float oldValue)
    {
        var id = (StatId)statId;

        if (id == StatId.MaxMana)
        {
            float max = MaxMana;
            if (CurrentMana > max) CurrentMana = max;
            EmitManaChanged();
            return;
        }
    }

    public bool CanAfford(float cost)
    {
        if (cost <= 0f || CurrentMana + 0.0001f >= cost) return true;
        return false;
    }

    public bool TrySpend(float cost, bool showError = true)
    {
        if (cost <= 0f) return true;

        if (!CanAfford(cost))
        {
            if (showError) ShowNotEnoughMana();
            return false;
        }

        CurrentMana = Mathf.Max(0f, CurrentMana - cost);
        EmitManaChanged();
        return true;
    }

    public void Restore(float amount)
    {
        if (amount <= 0f) return;

        float old = CurrentMana;
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);

        if (Mathf.Abs(CurrentMana - old) > EmitEpsilon)
            EmitManaChanged();
    }

    private void EmitManaChanged()
    {
        EmitSignal(nameof(ManaChanged), CurrentMana, MaxMana);
    }

    public void ShowNotEnoughMana()
    {
        double now = Time.GetUnixTimeFromSystem();
        if (now - _lastErrorTime < ErrorCooldown)
            return;

        _lastErrorTime = now;
        _events?.EmitShowError("Not enough mana", 1.0f);
    }
}
