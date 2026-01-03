using Godot;
using System;

//Komponent zarządzający stanem zdrowia bytu.
public partial class HealthComponent : Node
{
    [Signal] public delegate void EntityDiedEventHandler();

	[Export] public float MaxHealth {get; set;} = 100f;
    [Export] public HurtboxComponent Hurtbox {get; set;}
	public bool Active {get; set; } = true;
	private bool isCurrentlyOnFire = false;
	private PackedScene floatingDamageScene;
    private float _currentHealth;
	public float CurrentHealth {
        get
        {
            return _currentHealth;
        }
        set
        {
            if (value <= 0)
            {
                _currentHealth = 0;
                Die();
            }
            else if (value > MaxHealth)
            {
                _currentHealth = MaxHealth;
            }
            else _currentHealth = value;
        }
    }
	private float fireDamage = 0f;
	protected Node3D _owner;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();

		CurrentHealth = MaxHealth;
		floatingDamageScene = GD.Load<PackedScene>("res://scenes/effects/floating_damage.tscn");

        SubscribeEvents();
	}

	private void SubscribeEvents()
    {
        Hurtbox.Hit += OnHit;
    }

	private void UnsubscribeEvents()
    {
        Hurtbox.Hit -= OnHit;
    }

    public void OnHit(HitInfo hitInfo)
    {
        if (hitInfo.Source is not IDamageSource damageSource)
            return;
        
        float damage = hitInfo.FinalDamage;
		damage *= hitInfo.DamageMultiplier;

		Element damageType = damageSource.GetDamageType();

		//TUTAJ MOGĘ DODAĆ RESISTY
		//np. damage = ApplyResistance(damage, damageType);

		TakeDamage(damage);
		ShowDamage(hitInfo.HitPosition, damage, ElementColors.GetColor(damageType)); //KOLOR MOŻNA UZALEŻNIĆ OD ELEMENTU
    }

	public void ApplyStatusDamage(float damage, Element element)
	{
		if (!Active)
			return;

		TakeDamage(damage);
		// switch (element)
		// {
		// 	case Element.Fire:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(1, 0.5f, 0));
		// 		break;
		// 	case Element.Earth:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(0.6f, 0.4f, 0.2f));
		// 		break;
		// 	case Element.Water:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(0, 0.5f, 1));
		// 		break;
		// 	case Element.Air:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(0.8f, 0.8f, 0.8f));
		// 		break;
		// 	// case Element.Lightning:
		// 	// 	ShowDamage(enemy.GlobalPosition, damage, new Color(1, 1, 0));
		// 	// 	break;
		// 	case Element.Ice:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(0.5f, 0.8f, 1));
		// 		break;
		// 	default:
		// 		ShowDamage(_owner.GlobalPosition, damage, new Color(1, 1, 1));
		// 		break;
		// }

		// GD.Print($"Applying {damage} {element} status damage.");
		ShowDamage(_owner.GlobalPosition, damage, ElementColors.GetStatusEffectColor(element));
	}

	public virtual void TakeDamage(float damage)
	{
		CurrentHealth -= damage;
	}

    public virtual void Die()
    {
		Active = false;
		UnsubscribeEvents();
        EmitSignal(nameof(EntityDied));
    }

	public void Heal(float amount){
		CurrentHealth += amount;
	}

	public void ShowDamage(Vector3 position, float damage, Color color){
		var damageInt = Mathf.CeilToInt(damage);
		var floatingDamage = (FloatingDamage)floatingDamageScene.Instantiate();
		GetTree().CurrentScene.AddChild(floatingDamage);
		floatingDamage.GlobalPosition = position;
		floatingDamage.ShowDamage(damageInt, color);
	}
}
