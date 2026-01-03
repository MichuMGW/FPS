using Godot;

public partial class LoadingOverlay : Control
{
    [Export] public NodePath ProgressBarPath = "ProgressBar";
    private ProgressBar _bar;

    public override void _Ready()
    {
        _bar = GetNodeOrNull<ProgressBar>(ProgressBarPath);
        Visible = true;
        SetProgress(0f);
    }

    public void SetProgress(float p01)
    {
        if (_bar != null)
            _bar.Value = Mathf.Clamp(p01, 0f, 1f) * 100.0f;
    }
}
