using Godot;

[GlobalClass]
public partial class ElementStatusProfile : Resource
{
    [Export] public Element Element;

    [ExportGroup("Burning (Fire, Magma, Storm, Dark)")]
    [Export] public bool EnableBurning = false;
    [Export] public float BurningDuration = 3f;
    [Export] public float BurningTickRate = 0.25f;
    [Export] public float BurningDotPerTick = 5f; // BAZA dla tego żywiołu

    [ExportGroup("Slow (Water, Ice, Dark)")]
    [Export] public bool EnableSlow = false;
    [Export(PropertyHint.Range, "0,0.95,0.01")]
    public float SlowMultiplier = 0.3f; // -30%
    [Export] public float SlowDuration = 2.5f;

    [ExportGroup("Bleed (Nature, Magma, Poison)")]
    [Export] public bool EnableBleed = false;
    [Export] public float BleedDuration = 4f;
    [Export] public float BleedTickRate = 0.5f;
    [Export] public float BleedDotPerTick = 3f;

    [ExportGroup("Earth Stun Buildup (Earth)")]
    [Export] public bool EnableEarthBuildup = false;
    [Export] public float EarthStunThreshold = 100f;  // próg “mocy”
    [Export] public float EarthBuildupDecayPerSecond = 10f; // spadanie buildupu
    [Export] public float EarthStunDuration = 1.0f;
}
