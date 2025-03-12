using Godot;
using System;

public partial class WaveUi : Control
{
	[Export]
	public Label currentWaveLabel;
	[Export]
	public Label enemiesLeftLabel;
	[Export]
	public Label waveEndedLabel;
	[Export]
	public Label TimeRemainingLabel;
	[Export]
	public VBoxContainer waveProgress;
	[Export]
	public VBoxContainer waveCompleted;


	public void UpdateWave(int wave)
	{
		currentWaveLabel.Text = "Wave: " + wave;
	}

	public void UpdateEnemiesLeft(int enemiesLeft)
	{
		enemiesLeftLabel.Text = "Enemies left: " + enemiesLeft;
	}

	public void showWaveEnded()
	{
		waveProgress.Visible = false;
		waveCompleted.Visible = true;
	}

	public void UpdateWaveCompleted(int wave)
	{
		waveEndedLabel.Text = "Wave " + wave + " completed!";
	}
	public void UpdateTimeRemaining(float time)
	{
		TimeRemainingLabel.Text = "Time remaining: " + time.ToString("0.0");
	}


	public void showWaveProgress()
	{
		waveCompleted.Visible = false;
		waveProgress.Visible = true;
	}

}
