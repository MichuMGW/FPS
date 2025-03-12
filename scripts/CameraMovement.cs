using Godot;
using System;

public partial class CameraMovement : Node3D
{
    [Export]
    public CharacterBody3D body;
    [Export]
    public Camera3D camera;
    [Export]
    public float sensitivity = 0.1f;

    public override void _Process(double delta)
    {
        
    }

    public override void _Input(InputEvent @event)
    {
        // Camera Mode Switch
        if(@event is InputEventMouseButton mouseButton){
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        if(@event.IsActionPressed("ui_cancel")){
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        // Camera rotation
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            float headRotationX = Rotation.X + Mathf.DegToRad(mouseMotion.Relative.Y * -sensitivity);
            headRotationX = Mathf.Clamp(headRotationX, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            Rotation = new Vector3(headRotationX, Rotation.Y, Rotation.Z);

            float bodyRotationY = body.Rotation.Y + Mathf.DegToRad(mouseMotion.Relative.X * -sensitivity);
            body.Rotation = new Vector3(body.Rotation.X, bodyRotationY, body.Rotation.Z);
        }
    }
}
