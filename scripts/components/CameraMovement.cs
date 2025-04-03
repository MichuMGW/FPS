using Godot;
using System;

public partial class CameraMovement : Node
{
    [Export] public CharacterBody3D body;
    [Export] public Node3D head;
    [Export] public Camera3D camera;
    [Export] public float sensitivity = 0.1f;

    public override void _Input(InputEvent @event)
    {
        // Camera Mode Switch
        if(@event is InputEventMouseButton mouseButton){
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        if(@event.IsActionPressed("ui_cancel")){
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        //Camera FOV change
        if(Input.IsActionPressed("Sprint")){
            camera.Fov = Mathf.Lerp(camera.Fov, 75, 0.1f);
        } else {
            camera.Fov = Mathf.Lerp(camera.Fov, 60, 0.1f);
        }

        // Camera rotation
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            float headRotationX = head.Rotation.X + Mathf.DegToRad(mouseMotion.Relative.Y * -sensitivity);
            headRotationX = Mathf.Clamp(headRotationX, Mathf.DegToRad(-90), Mathf.DegToRad(90));
            head.Rotation = new Vector3(headRotationX, head.Rotation.Y, head.Rotation.Z);

            float bodyRotationY = body.Rotation.Y + Mathf.DegToRad(mouseMotion.Relative.X * -sensitivity);
            body.Rotation = new Vector3(body.Rotation.X, bodyRotationY, body.Rotation.Z);
        }
    }
}
