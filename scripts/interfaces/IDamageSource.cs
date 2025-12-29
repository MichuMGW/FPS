using Godot;

public interface IDamageSource
{
    float GetDamage();
    Element GetDamageType();

    float GetCritChance();
    float GetCritMultiplier();

    bool CanHitAgain(Node3D target);
    void RegisterHit(Node3D target);

    ElementStatusProfile GetStatusProfile();
    float GetBurningDotMultiplier();
    float GetBleedDotMultiplier();
    float GetSlowBonus();
    float GetEarthBuildupPerHit();
}
