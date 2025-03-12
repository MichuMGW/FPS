using Godot;
using System;

public partial class WaveManager : Node
{
	//Klasa ma za zadanie zarządzać falami przeciwników.
	[Signal]
	public delegate void WaveStartedEventHandler(int enemiesToSpawn);
	[Signal]
	public delegate void WaveEndedEventHandler();

	[Export]
	Timer spawnTimer;
	[Export]
	Timer waveBreakTimer;
	[Export]
	PackedScene basicEnemyScene;
	[Export]
	WaveUi waveUi;
	SpawnLocationManager spawnLocationManager;
	
	int currentWave = 1;
	int enemiesToSpawn = -3; //ZMIENIĆ NA 5
	const int ENEMIES_PER_WAVE_GROWTH = 5;
	int enemiesSpawned = 0;
	int enemiesLeft;
	float spawnInterval = 2.0f;
	float waveInterval = 10.0f;
	bool isWaveActive = true;
	public override void _Ready()
	{
		spawnLocationManager = Owner.GetNode<SpawnLocationManager>("SpawnLocationManager");
		waveUi = Owner.GetNode<WaveUi>("UI/WaveUI");

		spawnTimer.Timeout += OnSpawnTimerTimeout;
		waveBreakTimer.Timeout += OnWaveBreakTimerTimeout;

		StartWave();
	}

    public override void _Process(double delta)
    {
        if(enemiesLeft == 0 && isWaveActive)
		{
			StopWave();
			isWaveActive = false;
		}

		if(!isWaveActive)
		{
			waveUi.UpdateTimeRemaining((float)waveBreakTimer.TimeLeft);
		}
    }

    public void StartWave()
	{
		isWaveActive = true;
		enemiesToSpawn = ENEMIES_PER_WAVE_GROWTH * currentWave;
		enemiesLeft = enemiesToSpawn;
		spawnTimer.WaitTime = spawnInterval;
		spawnTimer.Start();
		waveBreakTimer.Stop();

		waveUi.showWaveProgress();
		waveUi.UpdateWave(currentWave);
		waveUi.UpdateEnemiesLeft(enemiesLeft);
	}
	public void StopWave()
	{
		spawnTimer.Stop();
		waveBreakTimer.WaitTime = waveInterval;
		waveBreakTimer.Start();

		waveUi.showWaveEnded();
		waveUi.UpdateWaveCompleted(currentWave);
		//emit_signal(nameof(WaveEndedEventHandler));
	}

	public void OnSpawnTimerTimeout()
	{
		if(enemiesToSpawn > 0)
		{
			SpawnEnemy();
		}
		else
		{
			GD.Print("All enemies have been spawned");
			spawnTimer.Stop();
		}
	}

	public void OnWaveBreakTimerTimeout()
	{
		currentWave++;
		StartWave();
	}

	public void SpawnEnemy()
	{

		var enemyInstance = basicEnemyScene.Instantiate() as CharacterBody3D;

		//DODAĆ SKALOWANIE ZDROWIA Z FALĄ
		HealthComponent healthComponent = enemyInstance.GetNode<HealthComponent>("HealthComponent");
		healthComponent.EntityDied += OnEnemyDied;
		

		Owner.AddChild(enemyInstance);
		enemyInstance.GlobalPosition = spawnLocationManager.GetSpawnLocation();
		enemiesSpawned++;
		enemiesToSpawn--;

		waveUi.UpdateEnemiesLeft(enemiesLeft);
	}

	public void OnEnemyDied()
	{
		enemiesSpawned--;
		enemiesLeft--;
		waveUi.UpdateEnemiesLeft(enemiesLeft);

		GD.Print("Enemy died");
	}
}
