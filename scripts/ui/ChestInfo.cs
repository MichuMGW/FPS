using Godot;

public partial class ChestInfo : Node3D
{
    [Export] public NodePath CostLabelPath = "Sprite3D/SubViewport/CostText/HBoxContainer/CostLabel"; // dostosuj
    private Label _costLabel;
    private ChestManager _chestManager;

    public override void _Ready()
    {
        _costLabel = GetNode<Label>(CostLabelPath);
        Visible = false;
    }

    public void ShowInfo(int cost)
    {
        _costLabel.Text = cost.ToString();
        Visible = true;
    }

    public void HideInfo()
    {
        Visible = false;
    }
}
