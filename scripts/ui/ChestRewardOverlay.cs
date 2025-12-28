using Godot;
using System;
using System.Threading.Tasks;

public partial class ChestRewardOverlay : CanvasLayer
{
    [Export] private NodePath DimmerPath = "Dimmer";
    [Export] private NodePath RewardViewPath = "RewardView";
    [Export] private NodePath RewardCameraPath = "RewardView/SubViewport/Node3D/Camera3D";
    [Export] private NodePath SubViewportPath = "RewardView/SubViewport";
    [Export] private NodePath ChestPath = "RewardView/SubViewport/Reward3D/Chest";
    [Export] private NodePath ChestAnimPath = "RewardView/SubViewport/Reward3D/Chest/AnimationPlayer";
    [Export] private NodePath ItemSpawnPath = "RewardView/SubViewport/Reward3D/ItemSpawn";
    [Export] private NodePath HeroPointPath = "RewardView/SubViewport/Reward3D/ItemSpawn"; // popraw w scenie na HeroPoint
    [Export] private NodePath UIAnimPath = "UIAnim";

    [Export] public string ChestOpenAnimName = "ChestOpen";
    [Export] public float DimmerTargetAlpha = 0.65f;

    [Export] public float ItemRiseDuration = 0.45f;
    [Export] public float ItemMoveToCenterDuration = 0.55f;
    [Export] public float ItemDelayBeforeMove = 0.5f;

    [Export] public float IdleRotationSpeed = 1.5f; // rad/s
    [Export] public float IdleHoverAmplitude = 0.15f;
    [Export] public float IdleHoverSpeed = 2.0f;

    private ColorRect _dimmer;
    private TextureRect _rewardView;
    private SubViewport _vp;
    private Camera3D _rewardCamera;
    private TextureRect _freezeFrame;
    private Label _itemName;
    private Label _itemDescription;

    private Node3D _chest;
    private AnimationPlayer _chestAnim;

    private Marker3D _itemSpawn;
    private Marker3D _heroPoint;

    private AnimationPlayer _uiAnim;

    private GameEvents _events;

    private Node3D _itemInstance;
    private ItemDefinition _itemDef;
    private bool _idleEnabled = false;
    private float _t = 0f;

    private bool _canClose = false;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _dimmer = GetNode<ColorRect>(DimmerPath);
        _vp = GetNode<SubViewport>(SubViewportPath);
        _rewardView = GetNode<TextureRect>(RewardViewPath);
        _rewardCamera = GetNode<Camera3D>(RewardCameraPath);

        _chest = GetNode<Node3D>(ChestPath);
        _chestAnim = GetNodeOrNull<AnimationPlayer>(ChestAnimPath);

        _itemSpawn = GetNode<Marker3D>(ItemSpawnPath);

        // Jeśli pomyliłeś pathy w exportach, to przynajmniej wybuchnie od razu, a nie po tygodniu.
        _heroPoint = GetNode<Marker3D>(HeroPointPath);

        _uiAnim = GetNodeOrNull<AnimationPlayer>(UIAnimPath);

        _itemName = GetNode<Label>("ItemText/ItemName");
        _itemDescription = GetNode<Label>("ItemText/ItemDescription");

        // Startowo wszystko ciemne
        var c = _dimmer.Color;
        c.A = 0f;
        _dimmer.Color = c;

        _vp.TransparentBg = true;
        _rewardView.Texture = _vp.GetTexture();

        SyncViewportSize();
        GetViewport().SizeChanged += SyncViewportSize;
    }

    public void Start(ItemDefinition item, GameEvents events)
{
    _events = events;
    _itemDef = item;

    _canClose = false;
    SetTextAlpha(0f);

    FadeDimmerTo(DimmerTargetAlpha, 0.25f);

    if (_chestAnim != null && _chestAnim.HasAnimation(ChestOpenAnimName))
        _chestAnim.Play(ChestOpenAnimName);

    _itemInstance = CreateItemInstance(item?.PreviewScene);
    _itemInstance.Visible = true;
    _itemInstance.Scale = Vector3.Zero;

    _itemInstance.GlobalPosition = _itemSpawn.GlobalPosition;
    _itemInstance.GlobalRotation = _itemSpawn.GlobalRotation;

    RunSequence(item.DisplayName, item.Description);
}

    private async void RunSequence(string itemTitle, string itemDescription)
    {
        // krótka pauza żeby klapa skrzyni ruszyła
        await ToSignal(GetTree().CreateTimer(0.12f, processInPhysics: false, ignoreTimeScale: true), "timeout");

        // pokaż item

        // wysuń do góry (lokalnie w osi Y świata)
        // var riseTarget = _itemInstance.GlobalPosition + new Vector3(0f, 0.6f, 0f);
        // await TweenGlobalPosition(_itemInstance, riseTarget, ItemRiseDuration, Tween.TransitionType.Cubic, Tween.EaseType.Out);

        // mała przerwa “dramatyzm”
        await ToSignal(GetTree().CreateTimer(ItemDelayBeforeMove, processInPhysics: false, ignoreTimeScale: true), "timeout");

        // przesuń na hero point (czyli “środek ekranu” w tym mini-viewportcie)
        await TweenToHeroPoint();

        // POPRAWIC JAK DODAM CHESTMANAGER
        // SetItemText("TITLE", "description");
        // OnItemArrivedAtCenter();
        ShowItemText(itemTitle, itemDescription);

        // teraz idle
        _idleEnabled = true;
        _canClose = true;
    }

    private Node3D CreateItemInstance(PackedScene itemScene)
    {
        Node3D n;

        if (itemScene != null)
        {
            n = itemScene.Instantiate<Node3D>();
        }
        else
        {
            // debug placeholder: zwykła bryła
            var mesh = new MeshInstance3D();
            mesh.Mesh = new BoxMesh();
            n = new Node3D();
            n.AddChild(mesh);
        }

        // Wrzucamy do tego samego świata viewportu: Reward3D
        var reward3D = _itemSpawn.GetParent<Node3D>();
        reward3D.AddChild(n);

        // Pilnujemy, żeby działało w pauzie
        n.ProcessMode = ProcessModeEnum.Always;

        return n;
    }

    private void SyncViewportSize()
    {
        var s = GetViewport().GetVisibleRect().Size;
        _vp.Size = (Vector2I)s;
    }

    private void FadeDimmerTo(float targetAlpha, float duration)
    {
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenMethod(Callable.From<float>(a =>
        {
            var col = _dimmer.Color;
            col.A = a;
            _dimmer.Color = col;
        }), _dimmer.Color.A, targetAlpha, duration);
    }

    private async Task TweenGlobalPosition(Node3D node, Vector3 target, float duration, Tween.TransitionType trans, Tween.EaseType ease)
    {
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.SetTrans(trans);
        tween.SetEase(ease);
        tween.TweenProperty(node, "global_position", target, duration);
        await ToSignal(tween, "finished");
    }

    private async Task TweenToHeroPoint()
    {
        _itemInstance.Scale = Vector3.Zero;

        var moveTween = CreateTween();
        moveTween.SetPauseMode(Tween.TweenPauseMode.Process);
        moveTween.SetTrans(Tween.TransitionType.Cubic);
        moveTween.SetEase(Tween.EaseType.InOut);

        // równolegle: ruch
        moveTween.TweenProperty(_itemInstance, "global_position", _heroPoint.GlobalPosition, ItemMoveToCenterDuration);

        // równolegle: skala (możesz dać Ease.Out żeby “ładniej” dobijało do 1)
        moveTween.Parallel()
            .TweenProperty(_itemInstance, "scale", Vector3.One, ItemMoveToCenterDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        await ToSignal(moveTween, "finished");     
    }

    public override void _Process(double delta)
    {
        if (!_idleEnabled || _itemInstance == null)
            return;

        _t += (float)delta;

        // obrót
        _itemInstance.RotateY(IdleRotationSpeed * (float)delta);

        // lewitacja (względem pozycji hero point, żeby nie dryfowało)
        var basePos = _heroPoint.GlobalPosition;
        var y = Mathf.Sin(_t * IdleHoverSpeed) * IdleHoverAmplitude;
        _itemInstance.GlobalPosition = new Vector3(basePos.X, basePos.Y + y, basePos.Z);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_canClose)
            return;

        if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_accept")) // || @event.IsMouseButtonPressed(MouseButton.Left)
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Close()
    {
        _idleEnabled = false;

        if (_itemDef != null)
            _events?.ClaimChestReward(_itemDef);

        FadeDimmerTo(0f, 0.20f);

        _itemInstance?.QueueFree();
        _itemInstance = null;

        _events?.CloseChestReward();

        QueueFree();
    }

    public void SetItemText(string name, string description)
    {
        _itemName.Text = name;
        _itemDescription.Text = description;
    }

    private void SetTextAlpha(float a)
    {
        _itemName.Modulate = new Color(1, 1, 1, a);
        _itemDescription.Modulate = new Color(1, 1, 1, a);
    }

    public void ShowItemText(string name, string description)
    {
        _itemName.Text = name;
        _itemDescription.Text = description;

        SetTextAlpha(0f);

        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);

        tween.TweenProperty(_itemName, "modulate:a", 1f, 0.35f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(_itemDescription, "modulate:a", 1f, 0.35f)
            .SetDelay(0.05f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }


    // private void OnItemArrivedAtCenter()
    // {
    //     ShowItemText(_itemDisplayName, _itemDescriptionText);
    //     _idleEnabled = true;
    // }

}
