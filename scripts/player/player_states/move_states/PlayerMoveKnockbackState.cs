using Godot;

public class PlayerMoveKnockbackState : IState, IPhysicsUpdateState
{
    private readonly Player _player;

    // Opcjonalnie: jak bardzo pozwalasz “sterowaæ” w knockbacku (0 = wcale)
    private const float KnockbackControl = 0.0f;

    public PlayerMoveKnockbackState(Player player) => _player = player;

    public void Enter()
    {
        // Jak chcesz: przerwij casty, ¿eby nie by³o “dostajê w ryj i dalej kanalizujê laser”
        _player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
        _player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);

        // Opcjonalnie animki r¹k:
        // _player.PlayLeftArmAnimation("L_Idle");
        // _player.PlayRightArmAnimation("R_Idle");
    }

    public void Exit()
    {
        // nic szczególnego
    }

    public void PhysicsUpdate(double delta)
    {
        float dt = (float)delta;

        // 1) Aktualizacja fizyki knockbacku (obs³uguje prêdkoœæ XZ i ich wygasanie)
        if (_player.Knockback != null)
            _player.Knockback.PhysicsUpdate(delta);

        // 2) Grawitacja (obs³uguje oœ Y)
        _player.Movement.ApplyGravity(dt);

        // 3) Wykonaj ruch (CharacterBody3D.MoveAndSlide)
        // To jest kluczowe - upewnij siê, ¿e Movement.ApplyGravity lub Player wywo³uje MoveAndSlide()!
        _player.MoveAndSlide();

        // 4) Koniec knockbacku
        // Sprawdzamy prêdkoœæ horyzontaln¹ cia³a zamiast wewnêtrznego bufora
        Vector3 horizVel = new Vector3(_player.Velocity.X, 0, _player.Velocity.Z);
        if (horizVel.Length() < 0.1f)
        {
            _player.ChangeMoveState(_player.IsOnFloor() ? PlayerMoveStateId.Grounded : PlayerMoveStateId.Airborne);
        }
    }
}
