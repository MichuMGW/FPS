public sealed class EnemyUnlockConfig
{
    public float UnlockTimeSeconds;
    public string ScenePath;
    public float Weight;
    public bool Unlocked;

    // Opcjonalne filtry pod DifficultySnapshot:
    public int MinEnemyLevel;
    public int MaxEnemyLevel;
    public float MinCoeff;
    public float MaxCoeff;

    public EnemyUnlockConfig(
        float unlockTimeSeconds,
        string scenePath,
        float weight = 1f,
        int minEnemyLevel = 1,
        int maxEnemyLevel = 999,
        float minCoeff = 0f,
        float maxCoeff = float.MaxValue
    )
    {
        UnlockTimeSeconds = unlockTimeSeconds;
        ScenePath = scenePath;
        Weight = weight;
        Unlocked = false;

        MinEnemyLevel = minEnemyLevel;
        MaxEnemyLevel = maxEnemyLevel;
        MinCoeff = minCoeff;
        MaxCoeff = maxCoeff;
    }
}
