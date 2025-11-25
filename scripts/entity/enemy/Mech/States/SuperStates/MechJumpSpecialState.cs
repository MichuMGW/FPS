using Godot;

public class MechJumpSpecialState : IState
{
    private readonly Mech _owner;

    private enum Phase
    {
        Windup,
        Jump,
        Air,
        PrepareToLand,
        Land
    }

    private Phase _phase;
    private float _timer;
    private Vector3 _landingPos;

    private const float WindupDuration = 3.5f;
    private const float JumpDuration = 0.3f;
    private const float AirDuration = 3f;
    private const float PrepareToLandDuration = 3f;
    private const float LandDuration = 4f;
    private const float JumpHeight = 100f;

    public MechJumpSpecialState(Mech owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        GD.Print("Enter JUMP State");
        _phase = Phase.Windup;
        _timer = WindupDuration;

        _owner.LookAtActive = false;

        _owner.Pathfind.Active = false;
        _owner.VelocityComp.StopInstantly();
        _owner.VelocityComp.Active = false; // żeby komponent nie ruszał ciała

        _owner.PlayLocomotion("Mech_CrouchBeforeJump");
    }

    public void Exit()
    {
        _owner.VelocityComp.Active = true;
        _owner.AddAfterSpecialCooldown();
        _owner.ResetJumpCooldown();
    }

    public void Update(double delta)
    {
        _timer -= (float)delta;

        switch (_phase)
        {
            case Phase.Windup:
                if (_timer <= 0f)
                {
                    // start "skoku"
                    _phase = Phase.Jump;
                    _timer = JumpDuration;

                    _owner.PlayLocomotion("Mech_Jump");
                    // TODO: tutaj spawnuj indykator AoE na ziemi w _landingPos
                    // np. _owner.SpawnJumpIndicator(_landingPos);
                }
                break;
            case Phase.Jump:
                if (_timer <= 0f)
                {
                    // przejście do Air
                    _phase = Phase.Air;
                    _timer = AirDuration;
                    _owner.Visible = false;

                    // mech znika po sekundzie lotu
                    // ale najpierw damy mu wzbić się w górę (reszta w PhysicsUpdate)
                }
                break;

            case Phase.Air:
                if (_timer <= 0f)
                {
                    if (_owner.Player != null){
                        _landingPos = GetLandingPosition(_owner.Player);
                    }
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

                    // teleport nad miejsce lądowania
                    _owner.GlobalPosition = _landingPos;
                    _owner.Visible = true;

                    _owner.VelocityComp.Active = true;

                    _owner.LookAt(_owner.Player.GlobalPosition, Vector3.Up, true);

                    _owner.PlayLocomotion("Mech_Land");
                    _owner.DebreesPrtcl.Emitting = true;
                    SpawnDecal();
                }
                break;

            case Phase.Land:
                if (_timer <= 0f)
                {
                    _owner.ChangeSuperState(MechSuperStateId.Normal);
                }
                break;
        }
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
        GD.Print("Indicator position: " + indicator.GlobalPosition);
        GD.Print("Player position: " + _owner.Player.GlobalPosition);
        var newScale = _owner.LandingDamageRadius;

        tween.TweenProperty(indicator, "scale", new Vector3(newScale, newScale, newScale), PrepareToLandDuration);
        // tween.TweenProperty(indicator, "height", _owner.LandingDamageRadius * 2, PrepareToLandDuration);
        tween.TweenCallback(Callable.From(indicator.QueueFree));
    }

    private MeshInstance3D CreateIndicator()
    {
        var indicatorSphere = new MeshInstance3D();
        var sphereMesh = new SphereMesh();

        sphereMesh.Radius = 1f;
        sphereMesh.Height = 2f;
        
        indicatorSphere.Mesh = sphereMesh;
        indicatorSphere.Visible = true;

        var material = new StandardMaterial3D();
        material.Transparency = StandardMaterial3D.TransparencyEnum.Alpha;
        material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        material.AlbedoColor = new Color(1, 0, 0, 0.25f);
        indicatorSphere.SetSurfaceOverrideMaterial(0, material);

        return indicatorSphere;
    }

    public void PhysicsUpdate(double delta)
    {
        _owner.VelocityComp.StopInstantly();

        if (_phase == Phase.Jump)
        {
            _owner.GlobalPosition += Vector3.Up * (JumpHeight * (float)delta);
        }
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
