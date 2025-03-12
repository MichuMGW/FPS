using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpawnLocationManager : Node3D
{
	Timer timer;
	List<Marker3D> spawnPoints;
	public override void _Ready()
	{
		spawnPoints = new List<Marker3D>();
		GetChildren().OfType<Marker3D>().ToList().ForEach(marker => spawnPoints.Add(marker));
	}

	public Vector3 GetSpawnLocation()
	{
		var randomMarker = spawnPoints[GD.RandRange(0, spawnPoints.Count-1)];
		return randomMarker.GlobalPosition;
	}
}
