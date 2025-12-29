using Godot;

public partial class ChestInfo : Node3D
{
    [Export] public NodePath CostLabelPath = "Sprite3D/SubViewport/CostText/HBoxContainer/CostLabel";
    [Export] public NodePath HintLabelPath = "Sprite3D/SubViewport/HintText/HintLabel";

    private Label _costLabel;
    private Label _hintLabel;

    private const string DefaultHint = "Press \"E\" to open";

    public override void _Ready()
    {
        _costLabel = GetNodeOrNull<Label>(CostLabelPath);
        _hintLabel = GetNodeOrNull<Label>(HintLabelPath);
        Visible = false;
    }

    public void ShowInfo(int cost)
    {
        if (_costLabel != null) _costLabel.Text = cost.ToString();
        SetHint(DefaultHint);
        Visible = true;
    }

    public void HideInfo()
    {
        Visible = false;
    }

    public void SetHint(string text)
    {
        if (_hintLabel != null) _hintLabel.Text = text;
    }

    public void ShowNotEnoughGold(int cost)
    {
        // możesz to sformatować jak chcesz
        SetHint($"Not enough gold ({cost})");
    }
}
