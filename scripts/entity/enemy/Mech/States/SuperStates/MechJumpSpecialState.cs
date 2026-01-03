using Godot;

public class MechJumpSpecialState : IState, IUpdateState, IPhysicsUpdateState
{
    private readonly Mech _owner;

    private enum Phase { Windup, Jump, Air, PrepareToLand, Land }

    private Phase _phase;
    private float _timer;
    private Vector3 _landingPos;

    private const float WindupDuration = 3.5f;
    private const float JumpDuration = 0.3f;
    private const float AirDuration = 3f;
    private const float PrepareToLandDuration = 3f;
    private const float LandDuration = 4f;
    private const float JumpHeight = 100f;

    private const float LandingHitboxActiveSeconds = 0.2f;

    public MechJumpSpecialState(Mech owner) => _owner = owner;

    public void Enter()
    {
        GD.Print("Enter JUMP State");
        _phase = Phase.Windup;
        _timer = WindupDuration;

        _owner.LookAtActive = false;

        _owner.Pathfind.Active = false;
        _owner.VelocityComp.StopInstantly();
        _owner.VelocityComp.Active = false;

        // safety: hitbox ma być martwy zanim zaczniemy
        if (_owner.LandingHitbox != null)
            _owner.LandingHitbox.Active = false;

        _owner.PlayLocomotion("Mech_CrouchBeforeJump");
    }

    public void Exit()
    {
        _owner.VelocityComp.Active = true;
        _owner.AddAfterSpecialCooldown();
        _owner.ResetJumpCooldown();

        // safety: nie zostawiaj aktywnego AoE
        if (_owner.LandingHitbox != null)
            _owner.LandingHitbox.Active = false;
    }

    public void Update(double delta)
    {
        _timer -= (float)delta;

        switch (_phase)
        {
            case Phase.Windup:
                if (_timer <= 0f)
                {
                    _phase = Phase.Jump;
                    _timer = JumpDuration;
                    _owner.PlayLocomotion("Mech_Jump");
                }
                break;

            case Phase.Jump:
                if (_timer <= 0f)
                {
                    _phase = Phase.Air;
                    _timer = AirDuration;
                    _owner.Visible = false;
                }
                break;

            case Phase.Air:
                if (_timer <= 0f)
                {
                    if (_owner.Player != null)
                        _landingPos = GetLandingPosition(_owner.Player);

                    _phase = Phase.PrepareToLand;
                    _timer = PrepareToLandDuration;

                    CreateLandingIndicator(_landingPos);
                }
                break;

            case Phase.PrepareToLand:
                if (_timer <= 0f)
                {
                    _phase = Phase.Land;
                    _timer = LandDuration;

                    _owner.GlobalPosition = _landingPos;
                    _owner.Visible = true;

                    _owner.VelocityComp.Active = true;

                    FacePlayerYawOnly();

                    _owner.PlayLocomotion("Mech_Land");
                    _owner.DebreesParticles.Emitting = true;
                    SpawnDecal();

                    // >>> TU MA BIĆ <<<
                    ActivateLandingHitbox();
                }
                break;

            case Phase.Land:
                if (_timer <= 0f)
                    _owner.ChangeSuperState(MechSuperStateId.Normal);
                break;
        }
    }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.StopInstantly();

        if (_phase == Phase.Jump)
            _owner.GlobalPosition += Vector3.Up * (JumpHeight * (float)delta);
    }

    private void ActivateLandingHitbox()
    {
        var hb = _owner.LandingHitbox;
        if (hb == null) return;

        hb.Damage = _owner.Damage * 3f;
        hb.OneShot = true;
        hb.RehitCooldownSeconds = 0f;

        // radius z exportowanego CollisionShape3D
        SetLandingHitboxRadius(_owner.LandingDamageRadius);

        // reset pamięci trafień (ważne przy OneShot)
        hb.ResetHitMemory();

        hb.Active = true;

        var tree = _owner.GetTree();
        if (tree == null) return;

        var t = tree.CreateTimer(LandingHitboxActiveSeconds);
        t.Timeout += () =>
        {
            // owner mógł umrzeć / zostać zwolniony zanim timer strzeli
            if (_owner == null || !GodotObject.IsInstanceValid(_owner)) return;
            if (_owner.LandingHitbox == null || !GodotObject.IsInstanceValid(_owner.LandingHitbox)) return;

            _owner.LandingHitbox.Active = false;
        };
    }

    private void SetLandingHitboxRadius(float radius)
    {
        var cs = _owner.LandingHitboxCollisionShape;
        if (cs?.Shape is SphereShape3D sphere)
        {
            sphere.Radius = radius;
            return;
        }

        // fallback: jeśli masz inny shape, chociaż skaluj Area (lepsze to niż nic)
        if (_owner.LandingHitbox != null)
            _owner.LandingHitbox.Scale = new Vector3(radius, radius, radius);
    }

    // ---- reszta Twojego kodu bez zmian ----

    private void FacePlayerYawOnly()
    {
        if (_owner.Player == null) return;

        Vector3 toPlayer = _owner.Player.GlobalPosition - _owner.GlobalPosition;
        toPlayer.Y = 0f;

        if (toPlayer.LengthSquared() < 0.0001f)
            return;

        float yaw = Mathf.Atan2(toPlayer.X, toPlayer.Z);
        var rot = _owner.Rotation;
        rot.Y = yaw;
        _owner.Rotation = rot;
    }

    private Vector3 GetLandingPosition(Node3D body)
    {
        var pos = body.GlobalPosition;
        pos.Y = GetGroundHeight(pos);
        return pos;
    }

    private void CreateLandingIndicator(Vector3 position)
    {
        var indicator = CreateIndicator();
        _owner.GetTree().CurrentScene.AddChild(indicator);

        indicator.Scale = Vector3.Zero;

        var tween = _owner.CreateTween();
        indicator.GlobalPosition = position;

        var newScale = _owner.LandingDamageRadius;
        tween.TweenProperty(indicator, "scale", new Vector3(newScale, newScale, newScale), PrepareToLandDuration);
        tween.TweenCallback(Callable.From(indicator.QueueFree));
    }

    private MeshInstance3D CreateIndicator()
    {
        var indicatorSphere = new MeshInstance3D();
        var sphereMesh = new SphereMesh { Radius = 1f, Height = 2f };
        indicatorSphere.Mesh = sphereMesh;

        var material = new StandardMaterial3D
        {
            Transparency = StandardMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            AlbedoColor = new Color(1, 0, 0, 0.25f)
        };
        indicatorSphere.SetSurfaceOverrideMaterial(0, material);

        return indicatorSphere;
    }

    public float GetGroundHeight(Vector3 position)
    {
        var space = _owner.GetWorld3D().DirectSpaceState;

        var from = position + Vector3.Up * 10f;
        var to = position + Vector3.Down * 1000f;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;
        query.Exclude = new Godot.Collections.Array<Rid> { ((CharacterBody3D)_owner.Player).GetRid() };

        var result = space.IntersectRay(query);
        if (result.Count > 0)
            return ((Vector3)result["position"]).Y;

        return position.Y;
    }

    public void SpawnDecal()
    {
        var decal = _owner.LandingDecalScene.Instantiate<GroundmarkDecal>();
        var newScale = _owner.LandingDamageRadius;
        _owner.GetTree().CurrentScene.AddChild(decal);
        decal.Scale = new Vector3(newScale, newScale, newScale);
        decal.GlobalPosition = _owner.GlobalPosition;
    }
}
