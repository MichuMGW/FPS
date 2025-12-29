using Godot;

public partial class Chest : Node3D
{
    [Export] public ChestInfo Info;
    [Export] public MeshInstance3D Lock;
    [Export] public MeshInstance3D Base;
    [Export] public Area3D Area;

    public bool IsOpened { get; private set; }
    public bool IsDespawning { get; private set; }

    public override void _Ready()
    {
        Info?.HideInfo();
    }

    public void ShowInfo(int cost)
    {
        if (IsOpened || IsDespawning) return;
        Info?.ShowInfo(cost);
    }

    public void HideInfo()
    {
        if (IsOpened || IsDespawning) return;
        Info?.HideInfo();
    }

    public void ShowNotEnoughGold(int cost)
    {
        if (IsOpened || IsDespawning) return;
        Info?.ShowNotEnoughGold(cost);
    }

    public void LockInteraction()
    {
        if (Area == null) return;
        Area.Monitoring = false;
        Area.Monitorable = false;
    }

    public void DespawnWithTween()
    {
        if (IsDespawning) return;

        IsOpened = true;
        IsDespawning = true;

        LockInteraction();
        Info?.HideInfo();

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);

        if (Lock != null)
        {
            tween.Parallel()
                .TweenProperty(Lock, "scale", Vector3.Zero, 0.5f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
        }

        if (Base != null)
        {
            tween.Parallel()
                .TweenProperty(Base, "scale", new Vector3(1.1f, 0.9f, 1.1f), 0.5f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
        }

        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}
