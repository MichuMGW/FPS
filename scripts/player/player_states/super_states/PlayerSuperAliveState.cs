using Godot;

public class PlayerSuperAliveState : IState
{
    private readonly Player _player;

    public PlayerSuperAliveState(Player player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.CollisionLayer = PhysicsLayers.PLAYER_BODY;
        _player.CollisionMask = PhysicsLayers.TERRAIN | PhysicsLayers.ENEMY_BODY;

    }

    public void Exit()
    {
    }

    public void Update(double delta)
    {
        // Alive nie musi nic robić globalnie
    }

    public void PhysicsUpdate(double delta)
    {
        // Zostaw, bo knockback obsługujesz w Player._PhysicsProcess
    }
}
