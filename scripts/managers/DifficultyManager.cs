using Godot;

public struct DifficultySnapshot
{
    public float HpMultiplier;
    public float DamageMultiplier;
    public float MoveSpeedMultiplier;
    public float SpawnInterval;
    public int MaxAliveEnemies;
}

public class DifficultyManager
{
    //TODO: Skalowanie zrobić w sposób inny niż liniowy
    public DifficultySnapshot GetDifficulty(float normalizedTime, float elapsedSeconds)
    {
        normalizedTime = Mathf.Clamp(normalizedTime, 0f, 1f);

        float hp = 1f + normalizedTime * 3.0f; // na końcu 4x HP
        float damage = 1f + normalizedTime * 2.0f; // na końcu 3x DMG
        float speed = 1f + normalizedTime * 1.0f; // na końcu 2x speed

        // Spawn: na początku wolno, potem szybciej
        float spawnInterval = Mathf.Lerp(3.0f, 0.7f, normalizedTime); // od 3s do ~0.7s

        // Limit mobów rośnie z czasem
        int maxAlive = (int)Mathf.Lerp(8, 40, normalizedTime); // od 8 do 40

        return new DifficultySnapshot
        {
            HpMultiplier = hp,
            DamageMultiplier = damage,
            MoveSpeedMultiplier = speed,
            SpawnInterval = spawnInterval,
            MaxAliveEnemies = maxAlive
        };
    }
}
