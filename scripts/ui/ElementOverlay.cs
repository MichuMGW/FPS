using Godot;
using System.Text;

public partial class ElementOverlay : CanvasLayer
{
    [Export] public NodePath HeaderLabelPath = "ElementView/VBoxContainer/Top/HeaderLabel";
    [Export] public NodePath SubHeaderLabelPath = "ElementView/VBoxContainer/Top/SubHeaderLabel";

    [Export] public NodePath FireButtonPath = "ElementView/VBoxContainer/HBoxContainer/FireButton";
    [Export] public NodePath WaterButtonPath = "ElementView/VBoxContainer/HBoxContainer/WaterButton";
    [Export] public NodePath AirButtonPath = "ElementView/VBoxContainer/HBoxContainer/AirButton";
    [Export] public NodePath NatureButtonPath = "ElementView/VBoxContainer/HBoxContainer/NatureButton";

    [Export] public NodePath LeftSpellTitlePath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/LeftSpellVBoxContainer/SpellTitle";
    [Export] public NodePath RightSpellTitlePath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/RightSpellVBoxContainer/SpellTitle";
    [Export] public NodePath DashSpellTitlePath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/DashSpellVBoxContainer/SpellTitle";

    [Export] public NodePath LeftSpellIconPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/LeftSpellVBoxContainer/SpellIcon";
    [Export] public NodePath RightSpellIconPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/RightSpellVBoxContainer/SpellIcon";
    [Export] public NodePath DashSpellIconPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/DashSpellVBoxContainer/SpellIcon";

    [Export] public NodePath LeftSpellDescriptionPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/LeftSpellVBoxContainer/SpellDescription";
    [Export] public NodePath RightSpellDescriptionPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/RightSpellVBoxContainer/SpellDescription";
    [Export] public NodePath DashSpellDescriptionPath = "ElementView/VBoxContainer/Panel/MarginContainer/HBoxContainer/DashSpellVBoxContainer/SpellDescription";

    [Export] public NodePath SubViewportPath = "ElementView/SubViewport";

    [Export] public ElementKitDatabase KitDatabase;
    [Export] public bool IsSecondPick = false;

    private Label _headerLabel;
    private Label _subHeaderLabel;

    private Button _fireBtn, _waterBtn, _airBtn, _natureBtn;
    private Label _leftSpellTitle, _rightSpellTitle, _dashSpellTitle;
    private TextureRect _leftSpellIcon, _rightSpellIcon, _dashSpellIcon;
    private Label _leftSpellDescription, _rightSpellDescription, _dashSpellDescription;
    private SubViewport _vp;


    private GameEvents _events;
    private RunElementState _runState;

    public override void _Ready()
    {
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _runState = GetTree().Root.GetNodeOrNull<RunElementState>("RunElementState");

        _headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
        _subHeaderLabel = GetNodeOrNull<Label>(SubHeaderLabelPath);

        _fireBtn = GetNode<Button>(FireButtonPath);
        _waterBtn = GetNode<Button>(WaterButtonPath);
        _airBtn = GetNode<Button>(AirButtonPath);
        _natureBtn = GetNode<Button>(NatureButtonPath);

        _vp = GetNode<SubViewport>(SubViewportPath);

        _leftSpellTitle = GetNode<Label>(LeftSpellTitlePath);
        _rightSpellTitle = GetNode<Label>(RightSpellTitlePath);
        _dashSpellTitle = GetNode<Label>(DashSpellTitlePath);

        _leftSpellDescription = GetNode<Label>(LeftSpellDescriptionPath);
        _rightSpellDescription = GetNode<Label>(RightSpellDescriptionPath);
        _dashSpellDescription = GetNode<Label>(DashSpellDescriptionPath);

        _leftSpellIcon = GetNode<TextureRect>(LeftSpellIconPath);
        _rightSpellIcon = GetNode<TextureRect>(RightSpellIconPath);
        _dashSpellIcon = GetNode<TextureRect>(DashSpellIconPath);

        WireButton(_fireBtn, Element.Fire);
        WireButton(_waterBtn, Element.Water);
        WireButton(_airBtn, Element.Air);
        WireButton(_natureBtn, Element.Nature);

        ApplySecondPickRules();
        SetTopLabelsIdle();

        HideKitPreview();

        SyncViewportSize();
        GetViewport().SizeChanged += SyncViewportSize;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        ProcessMode = ProcessModeEnum.Always;
    }

    private void SyncViewportSize()
    {
        var s = GetViewport().GetVisibleRect().Size;
        _vp.Size = (Vector2I)s;
    }

    private void WireButton(Button btn, Element element)
    {
        btn.Pressed += () => Pick(element);

        // show preview on hover/focus
        btn.MouseEntered += () => ShowKitPreview(element);
        btn.FocusEntered += () => ShowKitPreview(element);

        // hide preview when leaving hover/focus
        btn.MouseExited += HideKitPreview;
        btn.FocusExited += HideKitPreview;
    }

    private void HideKitPreview()
    {
        ClearSpellUI();
        SetTopLabelsIdle();
    }

    private void ApplySecondPickRules()
    {
        if (!IsSecondPick || _runState == null) return;

        var first = _runState.First;
        if (first == Element.None) return;

        DisableIfNoCombo(first, Element.Fire, _fireBtn);
        DisableIfNoCombo(first, Element.Water, _waterBtn);
        DisableIfNoCombo(first, Element.Air, _airBtn);
        DisableIfNoCombo(first, Element.Nature, _natureBtn);
    }

    private void DisableIfNoCombo(Element first, Element candidate, Button btn)
    {
        if (btn.Disabled) return;

        if (candidate == first) return;

        var combined = ElementCombiner.Combine(first, candidate);
        if (combined == Element.None)
            btn.Disabled = true;
    }

    private void Pick(Element picked)
    {
        if (_runState != null)
        {
            if (!IsSecondPick)
                _runState.SetFirst(picked);
            else
                _runState.SetSecond(picked);
        }

        _events?.EmitElementPicked(picked, IsSecondPick);
        QueueFree();
    }

    private void ShowKitPreview(Element hoveredElement)
    {
        if (_headerLabel != null)
            _headerLabel.Text = IsSecondPick ? "Choose a second element" : "Choose an element";

        if (KitDatabase == null)
        {
            if (_subHeaderLabel != null)
                _subHeaderLabel.Text = "No ElementKitDatabase assigned.";
            ClearSpellUI();
            return;
        }

        Element previewElement = hoveredElement;

        if (IsSecondPick && _runState != null && _runState.HasFirst)
        {
            var first = _runState.First;

            if (hoveredElement == first)
            {
                // retain
                previewElement = first;
                if (_subHeaderLabel != null)
                    _subHeaderLabel.Text = $"Retain element: {ElementName(first)}";
            }
            else
            {
                previewElement = ElementCombiner.Combine(first, hoveredElement);
                if (previewElement == Element.None)
                {
                    if (_subHeaderLabel != null)
                        _subHeaderLabel.Text = $"Combination unavailable: {ElementName(first)} + {ElementName(hoveredElement)}";
                    ClearSpellUI();
                    return;
                }

                if (_subHeaderLabel != null)
                    _subHeaderLabel.Text = $"Element: {ElementName(previewElement)}";
            }
        }
        else
        {
            // first pick
            if (_subHeaderLabel != null)
                _subHeaderLabel.Text = $"Element: {ElementName(previewElement)}";
        }

        var kit = KitDatabase.GetKit(previewElement);
        if (kit == null)
        {
            if (_subHeaderLabel != null)
                _subHeaderLabel.Text = $"No kit found for: {ElementName(previewElement)}";
            ClearSpellUI();
            return;
        }

        BuildKitDescription(previewElement, kit);
    }

    private void BuildKitDescription(Element element, ElementKitDefinition kit)
    {
        ApplySpellToUI(
            titleLabel: _leftSpellTitle,
            descLabel: _leftSpellDescription,
            iconRect: _leftSpellIcon,
            def: kit.LeftHandSpell,
            fallbackTitle: "Left (Basic)"
        );

        ApplySpellToUI(
            titleLabel: _rightSpellTitle,
            descLabel: _rightSpellDescription,
            iconRect: _rightSpellIcon,
            def: kit.RightHandSpell,
            fallbackTitle: "Right"
        );

        ApplySpellToUI(
            titleLabel: _dashSpellTitle,
            descLabel: _dashSpellDescription,
            iconRect: _dashSpellIcon,
            def: kit.DashSpell,
            fallbackTitle: "Dash"
        );
    }

    private void ApplySpellToUI(Label titleLabel, Label descLabel, TextureRect iconRect, SpellDefinition def, string fallbackTitle)
    {
        if (titleLabel == null && descLabel == null && iconRect == null)
            return;

        if (def == null)
        {
            if (titleLabel != null) titleLabel.Text = $"{fallbackTitle}: (none)";
            if (descLabel != null) descLabel.Text = "";
            if (iconRect != null) iconRect.Texture = null;
            return;
        }

        if (titleLabel != null)
            titleLabel.Text = string.IsNullOrWhiteSpace(def.DisplayName) ? fallbackTitle : def.DisplayName;

        var d = (def.Description ?? "").Trim();

        var stats = $"DMG Multiplier: {def.DamageModifier * 100f:0.##}%  |  CD: {def.BaseCooldown:0.##} s  |  Mana: {def.BaseManaCost:0.##}  |  Range: {def.BaseRange:0.##}m";

        if (descLabel != null)
            descLabel.Text = string.IsNullOrWhiteSpace(d) ? stats : $"{d}\n{stats}";

        if (iconRect != null)
            iconRect.Texture = def.Icon;
    }

    private void ClearSpellUI()
    {
        ApplySpellToUI(_leftSpellTitle, _leftSpellDescription, _leftSpellIcon, null, "Left (Basic)");
        ApplySpellToUI(_rightSpellTitle, _rightSpellDescription, _rightSpellIcon, null, "Right");
        ApplySpellToUI(_dashSpellTitle, _dashSpellDescription, _dashSpellIcon, null, "Dash");
    }

    private void SetTopLabelsIdle()
    {
        if (_headerLabel != null)
            _headerLabel.Text = IsSecondPick ? "Choose a second element" : "Choose an element";

        if (_subHeaderLabel != null)
            _subHeaderLabel.Text = "";
    }

    private static string ElementName(Element e)
    {
        return e.ToString();
    }
}
