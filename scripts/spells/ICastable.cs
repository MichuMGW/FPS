using System;
using Godot;

public interface ICastable{
     void StartCasting(Vector3 position, Vector3 direction, Node3D caster);
    void HoldCasting(double delta);
    void EndCasting(Vector3 position, Vector3 direction, Node3D caster);
}