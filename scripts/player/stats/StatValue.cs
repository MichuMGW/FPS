public struct StatValue
{
    public float Base;
    public float Additive;
    public float Multiplier;

    public float Final => (Base + Additive) * Multiplier;
}