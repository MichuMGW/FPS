using Godot;
using System;

public partial class GameEvents : Node
{
    [Signal] public delegate void GameStartedEventHandler();
	[Signal] public delegate void ExperiencePointCollectedEventHandler(int currentExperience);
	[Signal] public delegate void MenuEnabledEventHandler(bool isMenuEnabled);
	[Signal] public delegate void RunEndedEventHandler();
	[Signal] public delegate void RunTimeUpdatedEventHandler(float elapsed, float total);

	// NEW: sygnały pod skrzynię (opcjonalnie, ale przydają się)
    [Signal] public delegate void ChestRewardStartedEventHandler();
    [Signal] public delegate void ChestRewardEndedEventHandler();
    // GameEvents.cs (dopisz)
    [Signal] public delegate void ElementPickRequestedEventHandler(bool isSecondPick);
    [Signal] public delegate void ElementPickedEventHandler(Element picked, bool isSecondPick);


	// Menu pauzy
	private PackedScene _gameMenuScene;
	private Control _gameMenuInstance;
	private bool _isMenuEnabled = false;

	// overlay skrzyni / reward cutscenka
	private PackedScene _chestRewardScene;
    private ChestRewardOverlay _chestRewardInstance;
    private ChestManager _chestManager;
    private bool _isChestRewardActive = false;
    private ItemInventory _inventory;
    private ItemDefinition _pendingChestItem;

    private PackedScene _elementOverlayScene;

	private int _pauseRequests = 0;
	private Camera3D _playerCamera;


	private Node mainScene;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_gameMenuScene = GD.Load<PackedScene>("res://scenes/game_menu_scene.tscn");
		_chestRewardScene = GD.Load<PackedScene>("res://scenes/ui/reward_overlay.tscn");
		_playerCamera = GetTree().GetFirstNodeInGroup("player_camera") as Camera3D;

        _elementOverlayScene = GD.Load<PackedScene>("res://scenes/ui/element_overlay.tscn");

        _chestManager = GetTree().GetFirstNodeInGroup("chest_manager") as ChestManager;

        _inventory = GetTree().Root.GetNodeOrNull<ItemInventory>("ItemInventory");
        if (_inventory == null)
            GD.PushWarning("GameEvents: missing ItemInventory reference.");

		mainScene = GetTree().CurrentScene;

        // CHYBA TRZA POTEM PRZENIEĆ DO GAME DIRECTOR ALBO GDZIE INDZIEJ
        _chestManager.SpawnChests();

        SubscribeEvents();
	}

    public void SubscribeEvents()
    {
        _chestManager.ChestOpened += OnChestOpened;
    }

    public void StartGame()
    {
        EmitSignal(nameof(GameStarted));
    }

	public void EmitExperiencePointCollected(int currentExperience)
	{
		EmitSignal(nameof(ExperiencePointCollected), currentExperience);
		//GD.Print("Experience Point Collected");
	}

	//Może do usunięcia
	public void EmitMenuEnabled(bool isMenuEnabled)
	{
		EmitSignal(nameof(MenuEnabled), isMenuEnabled);
		_isMenuEnabled = isMenuEnabled;
	}

    public void RequestElementPick(bool isSecondPick)
    {
        EmitSignal(nameof(ElementPickRequested), isSecondPick);
    }

    public void EmitElementPicked(Element picked, bool isSecondPick)
    {
        EmitSignal(nameof(ElementPicked), picked, isSecondPick);
    }


    private void OnChestOpened(ItemDefinition item, int cost, Chest chest)
    {
        // item może być null jeśli lista pusta
        PackedScene preview = item?.PreviewScene;
        _pendingChestItem = item;

        StartChestReward(preview, item); // <- typed optionalData, patrz niżej
    }

    public void ClaimChestReward()
    {
        if (_pendingChestItem != null && _inventory != null)
            _inventory.AddItem(_pendingChestItem);

        _pendingChestItem = null;
    }



    public override void _Input(InputEvent @event)
    {
		//DEBUG - otwieranie menu pauzy
		if (Input.IsActionPressed("debug_cheat_open_menu"))
		{
			StartChestReward(null, null);
			
		}
		//ENDOFDEBUG

        if (!(@event.IsActionPressed("ui_cancel")))
            return;

        // Menu tylko w MainScene (jak miałeś) i tylko jeśli nie trwa cutscenka skrzyni
        if (GetTree().CurrentScene?.Name != "MainScene")
            return;

        if (_isChestRewardActive)
            return;

        TogglePauseMenu();
    }

	private void TogglePauseMenu()
    {
        if (!_isMenuEnabled)
        {
            _gameMenuInstance = (Control)_gameMenuScene.Instantiate();
            _gameMenuInstance.ProcessMode = ProcessModeEnum.Always;

            RequestPause();
            GetTree().Root.AddChild(_gameMenuInstance);

            EmitMenuEnabled(true);
        }
        else
        {
            ReleasePause();
            Input.MouseMode = Input.MouseModeEnum.Captured;

            _gameMenuInstance?.QueueFree();
            _gameMenuInstance = null;

            EmitMenuEnabled(false);
        }
    }

	/// <summary>
    /// Odpal overlay skrzyni "nad grą".
    /// itemScene: PackedScene z modelem/rigem itemu (albo cokolwiek, co overlay sobie obsłuży).
    /// optionalData: np. nazwa, rarity, opis itd (jak będziesz chciał).
    /// </summary>
    public void StartChestReward(PackedScene itemScene, ItemDefinition item)
    {
        if (_isChestRewardActive)
            return;

        // Nie odpalaj skrzyni, jeśli masz już menu (albo zamknij menu, jak wolisz)
        if (_isMenuEnabled)
        {
            // Zamykamy menu, bo cutscenka jest ważniejsza niż ludzkie wciśnięcie ESC.
            TogglePauseMenu();
        }

        _chestRewardInstance = _chestRewardScene.Instantiate() as ChestRewardOverlay;
        _chestRewardInstance.ProcessMode = ProcessModeEnum.Always;

        // Cutscenka powinna działać w pauzie
        RequestPause();

        GetTree().Root.AddChild(_chestRewardInstance);

        _isChestRewardActive = true;
        EmitSignal(nameof(ChestRewardStarted));

        // Kontrakt: overlay ma metodę Start(itemScene, optionalData, gameEvents)
        // Jeśli jej nie ma, to po prostu się wyświetli i tyle (ale ty chcesz animacje, więc dodasz).
        _chestRewardInstance.Start(itemScene, item.DisplayName, item.Description, this);

        // Kursor zwykle odblokowujemy na czas cutscenki
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    /// <summary>
    /// Zamyka overlay skrzyni i oddaje kontrolę grze.
    /// To może być wywołane z overlay (np. po kliknięciu).
    /// </summary>
    public void CloseChestReward()
    {
        if (!_isChestRewardActive)
            return;

        _chestRewardInstance?.QueueFree();
        _chestRewardInstance = null;

        _isChestRewardActive = false;

        ReleasePause();
        Input.MouseMode = Input.MouseModeEnum.Captured;

        EmitSignal(nameof(ChestRewardEnded));
    }

    public void StartElementPick(bool isSecondPick)
    {
        var overlay = _elementOverlayScene.Instantiate() as ElementOverlay;
        overlay.ProcessMode = ProcessModeEnum.Always;
        overlay.IsSecondPick = isSecondPick;

        // przypnij bazę kitów (albo przez Export w scenie, jak wolisz)
        overlay.KitDatabase = GD.Load<ElementKitDatabase>("res://data/ElementKitDatabase.tres");

        _events.RequestPause();
        GetTree().Root.AddChild(overlay);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    // ----------------------------
    // Pauza z licznikami (żeby menu + skrzynia nie gryzły się)
    // ----------------------------

    private void RequestPause()
    {
        _pauseRequests++;
        GetTree().Paused = true;
    }

    private void ReleasePause()
    {
        _pauseRequests = Math.Max(0, _pauseRequests - 1);
        if (_pauseRequests == 0)
            GetTree().Paused = false;
    }


}
