using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Enemy : CharacterBody3D, IScalableEnemy
{
    public float MaxHealth { get; set; }
    public float MoveSpeed { get; set; }
    public float Damage { get; set; }

    private float _baseMaxHealth;
    private float _baseMoveSpeed;
    private float _baseDamage;

    public void LoadStatsFromResource(string resourcePath)
    {
        var res = GD.Load<EnemyStatsResource>(resourcePath);
        
        _baseMaxHealth = res.BaseMaxHealth;
        _baseMoveSpeed = res.BaseMoveSpeed;
        _baseDamage    = res.BaseDamage;

        MaxHealth = _baseMaxHealth;
        MoveSpeed = _baseMoveSpeed;
        Damage    = _baseDamage;

        OnStatsChanged(true);
    }

    public void ApplyDifficulty(DifficultySnapshot difficulty)
    {
        MaxHealth = _baseMaxHealth * difficulty.HpMultiplier;
        MoveSpeed = _baseMoveSpeed * difficulty.MoveSpeedMultiplier;
        Damage = _baseDamage    * difficulty.DamageMultiplier;

        OnStatsChanged(false);
    }

    protected abstract void OnStatsChanged(bool initialLoad);

}