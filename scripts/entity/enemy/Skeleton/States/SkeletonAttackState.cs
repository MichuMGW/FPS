using System;
using Godot;

public partial class SkeletonAttackState : IState
{
    private Skeleton _owner;
    private float _time;
    private bool _hitboxEnabled;
    private const float HitboxEnableTime = 0.54f;
    private const float HitboxActiveDuration = 0.18f;
    public SkeletonAttackState(Skeleton owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        var targetPos = _owner.PlayerAimTarget.GlobalPosition;
        targetPos.Y = _owner.GlobalPosition.Y;

        _owner.LookAt(targetPos, Vector3.Up, true);

        _time = 0f;
        _hitboxEnabled = false;

        _owner.Hitbox.Active = false;

        _owner.Animation.Play("Skeleton_Attack", 0.2f);
        _owner.Animation.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        _owner.ChangeState(SkeletonStateId.Chase);
    }

    public void Exit()
    {
        _owner.Hitbox.Active = false;
        _owner.Animation.AnimationFinished -= OnAnimationFinished;
    }

    public void PhysicsUpdate(double delta)
    {
        
    }

    public void Update(double delta)
    {
        _time += (float)delta;

        // włącz hitbox
        if (!_hitboxEnabled && _time >= HitboxEnableTime)
        {
            _hitboxEnabled = true;
            _owner.Hitbox.Active = true;
        }

        // wyłącz hitbox
        if (_hitboxEnabled && _time >= HitboxEnableTime + HitboxActiveDuration)
        {
            _hitboxEnabled = false;
            _owner.Hitbox.Active = false;
        }
    }
}