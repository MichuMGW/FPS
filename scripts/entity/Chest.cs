using Godot;

public partial class Chest : Node3D
{
    [Export] public ChestInfo Info;
    [Export] public MeshInstance3D Lock;
    [Export] public MeshInstance3D Base;
    [Export] public Area3D Area;
    [Export] public string OpenAnim = "ChestOpen";

    public bool IsOpened { get; private set; }
    public bool IsDespawning { get; private set; }

    public override void _Ready()
    {
        Info?.Hide();
    }

    public void ShowInfo(int cost)
    {
        if (IsOpened) return;
        Info?.ShowInfo(cost);
    }

    public void HideInfo()
    {
        if (IsOpened) return;
        Info?.Hide();
    }

    public void Open()
    {
        if (IsOpened || IsDespawning) return;
        IsOpened = true;
        IsDespawning = true;

        Area.Monitoring = false;
        Area.Monitorable = false;

        Info?.HideInfo();

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);
        tween.Parallel()
            .TweenProperty(Lock, "scale", new Vector3(0f, 0f, 0f), 0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        tween.Parallel()
            .TweenProperty(Base, "scale", new Vector3(1.1f, 0.9f, 1.1f), 0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(() =>
        {
            QueueFree();
        }));
        
    }
}
