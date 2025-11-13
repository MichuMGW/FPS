// using Godot;
// using System;

// public partial class EnemyFollow : State
// {
//     [Export]
//     private CharacterBody3D enemy;
//     [Export]
//     private RegularEnemyNavigationComponent navigationComponent;
//     [Export]
//     private const float MAX_SPEED = 5f;
//     private CharacterBody3D player;
//     [Export]
//     public HitboxDetection hitboxDetection;

//     private AnimationPlayer animationPlayer;
//     public override void Enter()
//     {
//         animationPlayer = GetOwner<CharacterBody3D>().GetNode<AnimationPlayer>("AnimationPlayer");
//         player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
//         hitboxDetection.PlayerSpotted += OnPlayerInRange;
//         animationPlayer.Play("Walk");
//     }

//     private void OnPlayerInRange()
//     {
//         EmitSignal(SignalName.Transitioned, this, "EnemyAttack");
//     }


//     public override void Exit()
//     {
//         GD.Print("EXIT EnemyFollow");
//         enemy.Velocity = enemy.Velocity.MoveToward(Vector3.Zero, 0.25f);
//     }

//     public override void Update(double delta)
//     {
//         // var targetPosition = new Vector3(player.GlobalPosition.X, enemy.GlobalPosition.Y, player.GlobalPosition.Z);
//         // enemy.LookAt(targetPosition, Vector3.Up, true);
//         var position2d = new Vector2(enemy.GlobalPosition.X, enemy.GlobalPosition.Z);
//         var playerPosition2d = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Z);
//         var direction = (playerPosition2d - position2d);
//         enemy.Rotation = new Vector3(0, Mathf.LerpAngle(enemy.Rotation.Y, Mathf.Atan2(direction.X, direction.Y),(float)delta * 5), 0);
//     }

//     public override void Physics_Update(double delta)
//     {
//         var newVelocity = navigationComponent.GetVelocity(enemy) * MAX_SPEED;
        
//         navigationComponent.UpdateTargetLocation();
//         enemy.Velocity = enemy.Velocity.MoveToward(newVelocity, 0.25f);
//         enemy.MoveAndSlide();
//     }

// }
