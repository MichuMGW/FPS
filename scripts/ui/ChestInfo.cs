using Godot;

public partial class ChestInfo : Node3D
{
    [Export] public NodePath CostLabelPath = "Sprite3D/SubViewport/CostText/HBoxContainer/CostLabel";

    private Label _costLabel;

    public override void _Ready()
    {
        _costLabel = GetNodeOrNull<Label>(CostLabelPath);
        Visible = false;
    }

    public void ShowCost(int cost)
    {
        if (_costLabel != null) _costLabel.Text = cost.ToString();
        Visible = true;
    }

    public void HideInfo() => Visible = false;
}
