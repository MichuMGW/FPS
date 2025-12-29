using Godot;
using System.Text;

public partial class ElementOverlay : CanvasLayer
{
    [Export] public NodePath FireButtonPath = "ElementView/VBoxContainer/HBoxContainer/FireButton";
    [Export] public NodePath WaterButtonPath = "ElementView/VBoxContainer/HBoxContainer/WaterButton";
    [Export] public NodePath AirButtonPath = "ElementView/VBoxContainer/HBoxContainer/AirButton";
    [Export] public NodePath NatureButtonPath = "ElementView/VBoxContainer/HBoxContainer/NatureButton";

    [Export] public NodePath DescriptionLabelPath = "ElementView/VBoxContainer/Panel/Label";
    [Export] public NodePath SubViewportPath = "ElementView/SubViewport";

    [Export] public ElementKitDatabase KitDatabase;

    [Export] public bool IsSecondPick = false;

    private Button _fireBtn, _waterBtn, _airBtn, _natureBtn;
    private Label _desc;
    private SubViewport _vp;

    private GameEvents _events;
    private RunElementState _runState;

    public override void _Ready()
    {
        _events = GetTree().Root.GetNodeOrNull<GameEvents>("GameEvents");
        _runState = GetTree().Root.GetNodeOrNull<RunElementState>("RunElementState");

        _fireBtn = GetNode<Button>(FireButtonPath);
        _waterBtn = GetNode<Button>(WaterButtonPath);
        _airBtn = GetNode<Button>(AirButtonPath);
        _natureBtn = GetNode<Button>(NatureButtonPath);

        _vp = GetNode<SubViewport>(SubViewportPath);
        _desc = GetNode<Label>(DescriptionLabelPath);

        WireButton(_fireBtn, Element.Fire);
        WireButton(_waterBtn, Element.Water);
        WireButton(_airBtn, Element.Air);

        // UWAGA: u Ciebie w enumie startowe żywioły to Fire/Water/Nature/Air.
        // Przycisk "NatureButton" na screenie najpewniej ma znaczyć "Nature/Ziemia bazowa".
        WireButton(_natureBtn, Element.Nature);

        ApplySecondPickRules();
        ShowKitPreview(Element.Fire); // domyślnie coś pokaż, żeby panel nie był pusty

        SyncViewportSize();
        GetViewport().SizeChanged += SyncViewportSize;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed)
            GD.Print($"[ElementOverlay] got click: {mb.ButtonIndex}");
    }

    private void SyncViewportSize()
    {
        var s = GetViewport().GetVisibleRect().Size;
        _vp.Size = (Vector2I)s;
    }

    private void WireButton(Button btn, Element element)
    {
        btn.Pressed += () => Pick(element);

        // “podgląd po najechaniu”:
        btn.MouseEntered += () => ShowKitPreview(element);
        btn.FocusEntered += () => ShowKitPreview(element);
    }

    private void ApplySecondPickRules()
    {
        if (!IsSecondPick || _runState == null) return;

        // zablokuj element już wybrany jako pierwszy
        var first = _runState.First;
        if (first == Element.None) return;

        if (first == Element.Fire) _fireBtn.Disabled = true;
        if (first == Element.Water) _waterBtn.Disabled = true;
        if (first == Element.Air) _airBtn.Disabled = true;
        if (first == Element.Nature) _natureBtn.Disabled = true;
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

        // Zamykamy overlay. Jeśli wolisz animację, odpal AnimationPlayer i dopiero QueueFree w callbacku.
        QueueFree();
    }

    private void ShowKitPreview(Element baseElement)
    {
        if (KitDatabase == null)
        {
            _desc.Text = "Brak przypiętej bazy kitów (ElementKitDatabase).";
            return;
        }

        // Podgląd:
        // - dla pierwszego wyboru: pokazujemy kit bazowy
        // - dla drugiego wyboru: pokazujemy wynik kombinacji (first + hovered)
        Element previewElement = baseElement;

        if (IsSecondPick && _runState != null && _runState.HasFirst)
        {
            previewElement = ElementCombiner.Combine(_runState.First, baseElement);
            if (previewElement == Element.None)
            {
                _desc.Text = $"Brak kombinacji dla: {_runState.First} + {baseElement}.";
                return;
            }
        }

        var kit = KitDatabase.GetKit(previewElement);
        if (kit == null)
        {
            _desc.Text = $"Brak kitu dla żywiołu: {previewElement}.";
            return;
        }

        _desc.Text = BuildKitDescription(previewElement, kit);
    }

    private string BuildKitDescription(Element element, ElementKitDefinition kit)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Żywioł: {element}");
        sb.AppendLine();

        AppendSpell(sb, "Lewy", kit.LeftHandSpell);
        AppendSpell(sb, "Prawy", kit.RightHandSpell);
        AppendSpell(sb, "Dash", kit.DashSpell);

        return sb.ToString().TrimEnd();
    }

    private void AppendSpell(StringBuilder sb, string slotName, SpellDefinition def)
    {
        if (def == null)
        {
            sb.AppendLine($"{slotName}: (brak)");
            sb.AppendLine();
            return;
        }

        sb.AppendLine($"{slotName}: {def.DisplayName}");
        if (!string.IsNullOrWhiteSpace(def.Description))
            sb.AppendLine(def.Description.Trim());

        // “krótka ściąga” ze statów, żeby gracz coś widział
        sb.AppendLine($"DMG: {def.DamageModifier} | CD: {def.BaseCooldown:0.##} | Mana: {def.BaseManaCost:0.##} | Range: {def.BaseRange:0.##}");
        sb.AppendLine();
    }
}
