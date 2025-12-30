using Godot;
using System;

public enum RunDifficulty
{
    Casual,   // wolniej
    Normal,   // default
    Hardcore  // szybciej
}

public struct DifficultySnapshot
{
    public float Coeff;                // globalny współczynnik
    public int EnemyLevel;             // “ambient level” (Twoje)
    public float HpMultiplier;
    public float DamageMultiplier;
    public float MoveSpeedMultiplier;

    public float SpawnInterval;
    public int MaxAliveEnemies;
    public int MaxMinionOverCap;
}

public sealed class DifficultyManager
{
    // --- Tuning (Twoje, nie ich) ---
    // Bazowe tempo narastania (na minutę)
    private const float BaseTimeSlope = 0.055f;

    // Drobna krzywizna late-game (żeby po 25-30 min nie było “ciągle to samo”)
    private const float LateRamp = 0.0022f;

    // “Level” z coeff: krok na poziom (nie bierz 0.33, ustaw swoje)
    private const float CoeffPerLevel = 0.08f;

    // Per-level scaling (u nich jest stały wzrost per level, my robimy “podobnie w duchu”)
    private const float HpPerLevel = 0.28f;
    private const float DmgPerLevel = 0.18f;
    private const float SpdPerLevel = 0.06f;

    // Spawn knobs
    // private const float SpawnIntervalStart = 8.0f; //NAPRAWIĆ POTEM
    private const float SpawnIntervalStart = 0.01f;

    private const float SpawnIntervalFloor = 0.60f;

    private const int MaxAliveStart = 60; //NAPRAWIĆ POTEM
    private const int MaxAliveCap = 60;

    private const float BudgetStart = 5.5f;
    private const float BudgetGrowth = 0.11f; // wzrost budżetu od coeff

    // Mikro-skoki “jak stage”, ale bez stage:
    // co N minut lekko dopalasz coeff (np. “fala elite” / “nowa pula wrogów”)
    private const float PulseEveryMinutes = 6.0f;
    private const float PulseStrength = 0.06f; // 6% przyspieszenia na oknie pulsu
    private const float PulseWidth = 0.9f;     // szerokość pulsu w minutach (miękko, nie schodek)

    public DifficultySnapshot GetDifficulty(
        float elapsedSeconds,
        RunDifficulty runDifficulty
    )
    {
        float tMin = Mathf.Max(0f, elapsedSeconds) / 60f;

        // 1) Difficulty pace (u RoR2 różne “mode” zmieniają tempo czasu) :contentReference[oaicite:3]{index=3}
        float pace = runDifficulty switch
        {
            RunDifficulty.Casual   => 0.75f,
            RunDifficulty.Normal   => 1.00f,
            RunDifficulty.Hardcore => 1.35f,
            _ => 1.0f
        };

        // 2) Coeff: liniowo + lekki late ramp (quadratic-ish)
        float coeff = 1.0f + pace * (BaseTimeSlope * tMin + LateRamp * tMin * tMin);

        // 3) “Pseudo-stage pulses”: miękkie “podskoki” co kilka minut,
        // żeby wymuszać zmianę puli wrogów / częstsze elity, bez stage.
        coeff *= 1.0f + Pulse(tMin, PulseEveryMinutes, PulseStrength, PulseWidth);

        // 5) Enemy level z coeff (inspirowane “ambient level” ideą) :contentReference[oaicite:4]{index=4}
        int enemyLevel = Mathf.Clamp(1 + Mathf.FloorToInt((coeff - 1.0f) / CoeffPerLevel), 1, 999);

        // 6) Stat multipliers “per level”
        // (to jest duch RoR2: level robi stały przyrost statów) :contentReference[oaicite:5]{index=5}
        float hpMul = Mathf.Pow(1f + HpPerLevel, enemyLevel - 1);
        float dmgMul = Mathf.Pow(1f + DmgPerLevel, enemyLevel - 1);
        float spdMul = Mathf.Pow(1f + SpdPerLevel, enemyLevel - 1);

        // 7) Spawn: asymptotycznie, nie do zera
        float spawnInterval = Mathf.Max(
            SpawnIntervalFloor,
            SpawnIntervalStart / (1f + 0.55f * Mathf.Log(1f + coeff))
        );

        int maxAlive = Mathf.Clamp(
            MaxAliveStart + Mathf.RoundToInt(10f * Mathf.Log(1f + coeff) + 0.8f * (enemyLevel - 1)),
            MaxAliveStart,
            MaxAliveCap
        );

        int maxMinionOverCap = 50 + Mathf.RoundToInt(15f * (coeff - 1f));

        return new DifficultySnapshot
        {
            Coeff = coeff,
            EnemyLevel = enemyLevel,
            HpMultiplier = hpMul,
            DamageMultiplier = dmgMul,
            MoveSpeedMultiplier = spdMul,
            SpawnInterval = spawnInterval,
            MaxAliveEnemies = maxAlive,
            MaxMinionOverCap = maxMinionOverCap
        };
    }

    // Miękki “puls” zamiast stageFactor: Gaussian-ish wokół wielokrotności period
    private static float Pulse(float tMin, float period, float strength, float width)
    {
        if (period <= 0.01f) return 0f;

        float k = Mathf.Round(tMin / period) * period;     // najbliższa “kotwica”
        float x = (tMin - k) / Mathf.Max(0.01f, width);    // znormalizowana odległość
        float g = Mathf.Exp(-0.5f * x * x);                // 1 w środku, maleje miękko
        return strength * g;
    }
}
