using System.Diagnostics.Tracing;
using Godot;

public partial class ExplosionSpell : Spell {
    public float ExplosionRadius { get; set; } = 1.0f;
    public float MaxExplosionRadius { get; set; } = 5.0f;
    public float GrowthRate { get; set; } = 3.0f; // Jak szybko rośnie obszar
    public RayCast3D raycast;
    private MeshInstance3D indicatorSphere;
    private SphereMesh sphereMesh;
    private bool isCasting = false;

    public ExplosionSpell(string SpellName, float SpellDamage, float SpellCooldown, float ManaCost, string SpellScenePath)
        : base(SpellName, SpellDamage, SpellCooldown, ManaCost, SpellScenePath)
    {}

    public override void _Ready()
    {
        
    }

    private void CreateIndicator(){
        indicatorSphere = new MeshInstance3D();
        sphereMesh = new SphereMesh();
        sphereMesh.Radius = 1.0f;
        sphereMesh.Height = sphereMesh.Radius * 2;
        indicatorSphere.Mesh = sphereMesh;
        indicatorSphere.Visible = true;
        StandardMaterial3D material = new StandardMaterial3D();
        material.Transparency = StandardMaterial3D.TransparencyEnum.Alpha;
        indicatorSphere.SetSurfaceOverrideMaterial(0, material);
    }


    public override void StartCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        // Tworzymy wskaźnik sfery
        CreateIndicator();
        // Dodajemy sferę do świata gry
        caster.AddChild(indicatorSphere);

        raycast = caster.GetNode<RayCast3D>("Head/Camera3D/RayCast3D");
        isCasting = true;
        indicatorSphere.Visible = true;
        ExplosionRadius = 1.0f;

        //DO USUNIĘCIA
        var animation = caster.GetNode<AnimationPlayer>("AnimationPlayer");
        animation.Play("CastExplosion_Load");
        //KONIEC
    }

    public override void HoldCasting(double delta)
    {
        if (!isCasting) return;

        // Sprawdzanie kolizji Raycasta
        if (raycast.IsColliding())
        {
            indicatorSphere.GlobalPosition = raycast.GetCollisionPoint();
            indicatorSphere.GetActiveMaterial(0).Set("albedo_color", new Color(0, 1, 0, 0.25f)); // Zielony
        }
        else
        {
            Vector3 offsetPosition = raycast.GlobalTransform.Basis * new Vector3(0, 0, -25f);
            indicatorSphere.GlobalPosition = raycast.GlobalPosition + offsetPosition;
            indicatorSphere.GetActiveMaterial(0).Set("albedo_color", new Color(1, 0, 0, 0.25f)); // Czerwony
        }

        // Powiększanie sfery
        ExplosionRadius = Mathf.Min(ExplosionRadius + GrowthRate * (float)delta, MaxExplosionRadius);
        ((SphereMesh)indicatorSphere.Mesh).Radius = ExplosionRadius;
        ((SphereMesh)indicatorSphere.Mesh).Height = ExplosionRadius * 2;
    }

    public override void EndCasting(Vector3 position, Vector3 direction, Node3D caster)
    {
        //DO USUNIĘCIA
        var animation = caster.GetNode<AnimationPlayer>("AnimationPlayer");
        animation.Play("Idle");
        //KONIEC

        isCasting = false;
        indicatorSphere.Visible = false;

        if (raycast.IsColliding())
        {
            animation.Play("CastExplosion_Release");
            var explosionInstance = SpellScene.Instantiate() as Node3D;
            caster.GetParent().AddChild(explosionInstance);
            explosionInstance.GlobalPosition = raycast.GetCollisionPoint();

            //Increase radius
            var explosionMesh = explosionInstance.GetNode<MeshInstance3D>("MeshInstance3D");
            var explosionHitbox = explosionInstance.GetNode<CollisionShape3D>("HitboxComponent/CollisionShape3D");

            explosionMesh.Mesh = new SphereMesh {Radius = ExplosionRadius, Height = ExplosionRadius*2};
            explosionHitbox.Shape = new SphereShape3D {Radius = ExplosionRadius};
        }
        indicatorSphere.QueueFree();
    }

}
