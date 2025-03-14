using Godot;

public partial class ExplosionSpell : Spell {
    public float ExplosionRadius {get; set;}
    public ExplosionSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, string SpellScenePath)
        : base(SpellName, SpellDamage, SpellCooldown, ManaCost, SpellScenePath)
    {

    }

    public override void Cast(Vector3 position, Vector3 direction, Node3D caster)
    {

        var raycast = caster.GetNode<RayCast3D>("Head/Camera3D/RayCast3D");

        if (raycast.IsColliding()){
            var explosionInstance = SpellScene.Instantiate() as Node3D;
            caster.GetParent().AddChild(explosionInstance);
            explosionInstance.GlobalPosition = raycast.GetCollisionPoint();
        }
    }

} 