using Godot;
using System;
using System.Collections.Generic;

public partial class ChestManager : Node3D
{
	[Signal] public delegate void ChestOpenedEventHandler(ItemDefinition item, int cost, Chest chest);

	[Export] public PackedScene ChestScene;
	[Export] public int ChestCount = 8;

	[Export] public Vector2 MapHalfExtents = new(285f, 285f);
	[Export] public Texture2D SpawnMaskTexture;
	[Export] public bool FlipV = true;

	[Export] public uint TerrainCollisionMask = PhysicsLayers.TERRAIN;
	[Export] public float RaycastHeight = 120f;

	[Export] public float MinChestDistance = 20f;
	[Export] public int MaxTriesPerChest = 80;

	[Export] public float InteractDistance = 4.5f;
	[Export] public int BaseOpenCost = 20;

	[Export] public Godot.Collections.Array<ItemDefinition> AvailableItems = new();

	public int CurrentOpenCost { get; private set; } = 1;

	private Image _maskImage;
	private int _maskW, _maskH;

	private RayCast3D _playerRay;
	private Node3D _player;

	private readonly List<Chest> _chests = new();
	private Chest _focusedChest;
	private GameEvents _events;

	public override void _Ready()
	{
		GD.Randomize();

		_playerRay = GetTree().GetFirstNodeInGroup("player_ray") as RayCast3D;
		_player = GetTree().GetFirstNodeInGroup("player") as Node3D;
		_events = GetTree().Root.GetNodeOrNull("GameEvents") as GameEvents;

		InitMask();

		CurrentOpenCost = Math.Max(0, BaseOpenCost);
	}

	public override void _Process(double delta)
	{
		UpdateFocusChest();
		UpdateChestInfo();
		HandleOpenInput();
	}

	public void SetOpenCost(int cost) => CurrentOpenCost = Math.Max(0, cost);

	public void SpawnChests()
	{
		if (ChestScene == null)
		{
			GD.PushError("[ChestManager] ChestScene not assigned.");
			return;
		}

		ClearSpawned();

		var placed = new List<Vector3>();

		for (int i = 0; i < ChestCount; i++)
		{
			if (!TryGetChestPosition(placed, out var pos))
			{
				GD.PushWarning($"[ChestManager] Could not place chest #{i}.");
				continue;
			}

			var node = ChestScene.Instantiate<Chest>(); // <- typed instantiate
			AddChild(node);

			node.GlobalPosition = pos;

			// losowa rotacja Y
			var r = node.Rotation;
			r.Y = (float)GD.RandRange(0, Mathf.Tau);
			node.Rotation = r;

			_chests.Add(node);
			placed.Add(pos);
		}
	}

	// ---------------- Focus + UI ----------------

	private void UpdateFocusChest()
	{
		if (_playerRay == null || !_playerRay.IsColliding())
		{
			SetFocused(null);
			return;
		}

		var collider = _playerRay.GetCollider() as Node;
		if (collider == null)
		{
			SetFocused(null);
			return;
		}

		// Raycast trafia w Area3D -> idziemy po rodzicach do Chest
		var chest = FindParentChest(collider);
		SetFocused(chest);
	}

	private Chest FindParentChest(Node n)
	{
		while (n != null)
		{
			if (n is Chest c) return c;
			n = n.GetParent();
		}
		return null;
	}

	private void SetFocused(Chest chest)
	{
		if (_focusedChest == chest) return;

		_focusedChest?.HideInfo();
		_focusedChest = chest;
	}

	private void UpdateChestInfo()
	{
		if (_focusedChest == null || _player == null) return;

		if (_focusedChest.IsOpened)
		{
			_focusedChest.HideInfo();
			return;
		}

		float dist = _focusedChest.GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist <= InteractDistance)
			_focusedChest.ShowInfo(CurrentOpenCost);
		else
			_focusedChest.HideInfo();
	}

	private void HandleOpenInput()
	{
		if (_focusedChest == null || _focusedChest.IsOpened) return;
		if (_player == null) return;

		float dist = _focusedChest.GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist > InteractDistance) return;

		if (!Input.IsActionJustPressed("interact"))
			return;

		OpenFocused();
	}

	private void OpenFocused()
	{
		var chest = _focusedChest;
		if (chest == null) return;

		// TODO: sprawdź zasób gracza (klucze itd.)
		SetFocused(null);
		var item = RollItem();

		_events.RequestChestReward(item, CurrentOpenCost);
		chest.Open();
		// EmitSignal(nameof(ChestOpened), item, CurrentOpenCost, chest);
	}

	private ItemDefinition RollItem()
	{
		if (AvailableItems == null || AvailableItems.Count == 0)
			return null;

		int total = 0;
		foreach (var it in AvailableItems)
			total += Math.Max(1, GetWeight(it.Rarity));

		int r = (int)GD.RandRange(0, total);
		foreach (var it in AvailableItems)
		{
			r -= Math.Max(1, GetWeight(it.Rarity));
			if (r < 0) return it;
		}

		return AvailableItems[0];
	}

	// ---------------- Spawning helpers ----------------

	private void ClearSpawned()
	{
		foreach (var c in _chests) c?.QueueFree();
		_chests.Clear();
		_focusedChest = null;
	}

	private bool TryGetChestPosition(List<Vector3> placed, out Vector3 pos)
	{
		pos = default;

		var world = GetWorld3D();
		if (world == null) return false;

		var space = world.DirectSpaceState;

		for (int i = 0; i < MaxTriesPerChest; i++)
		{
			float x = (float)GD.RandRange(-MapHalfExtents.X, MapHalfExtents.X);
			float z = (float)GD.RandRange(-MapHalfExtents.Y, MapHalfExtents.Y);
			var candidate = new Vector3(x, 0f, z);

			if (!IsAllowedByMask(candidate))
				continue;

			var from = candidate + Vector3.Up * RaycastHeight;
			var to = candidate + Vector3.Down * RaycastHeight * 2f;

			var ray = PhysicsRayQueryParameters3D.Create(from, to);
			ray.CollisionMask = TerrainCollisionMask;

			var hit = space.IntersectRay(ray);
			if (hit.Count == 0) continue;

			Vector3 groundPos = (Vector3)hit["position"];
			Vector3 normal = (Vector3)hit["normal"];
			if (normal.Y < 0.7f) continue;

			bool tooClose = false;
			foreach (var p in placed)
			{
				if (p.DistanceTo(groundPos) < MinChestDistance)
				{
					tooClose = true;
					break;
				}
			}
			if (tooClose) continue;

			pos = groundPos;
			return true;
		}

		return false;
	}

	private void InitMask()
	{
		if (SpawnMaskTexture == null) return;
		_maskImage = SpawnMaskTexture.GetImage();
		_maskW = _maskImage.GetWidth();
		_maskH = _maskImage.GetHeight();
	}

	private bool IsAllowedByMask(Vector3 worldPos)
	{
		if (_maskImage == null) return true;

		float u = (worldPos.X + MapHalfExtents.X) / (MapHalfExtents.X * 2f);
		float v = (worldPos.Z + MapHalfExtents.Y) / (MapHalfExtents.Y * 2f);

		u = Mathf.Clamp(u, 0f, 1f);
		v = Mathf.Clamp(v, 0f, 1f);
		if (FlipV) v = 1f - v;

		int px = (int)(u * (_maskW - 1));
		int py = (int)(v * (_maskH - 1));

		return _maskImage.GetPixel(px, py).R > 0.5f;
	}

	private int GetWeight(ItemRarity r) => r switch
	{
		ItemRarity.Common => 70,
		ItemRarity.Rare => 22,
		ItemRarity.Epic => 7,
		ItemRarity.Legendary => 1,
		_ => 1
	};

}
