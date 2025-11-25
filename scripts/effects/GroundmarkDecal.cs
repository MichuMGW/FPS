using Godot;

public partial class GroundmarkDecal : Decal
{
    [Export] public float Lifetime = 6f;
    [Export] public float FadeDuration = 2f;

    public override void _Ready()
    {
        FadeOutAndDie();
    }

    private async void FadeOutAndDie()
    {
        await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");

        var tween = CreateTween();
        tween.TweenProperty(this, "albedo_mix", 0f, FadeDuration);

        await ToSignal(tween, "finished");

        QueueFree();
    }
}
