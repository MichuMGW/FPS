public enum SpellCastMode
{
    Instant,               // klik = cast
    HoldRepeatCooldown,    // trzymasz = castuj ile się da (projectile left hand)
    Channel,               // trzymasz = działa, puszczasz = kończy, cooldown po puszczeniu (area/beam)
    ChargeRelease,         // trzymasz = ładuje, puszczasz = cast zależny od charge
    ChargeAuto             // trzymasz = ładuje, cast jak doładuje (nie trzeba puszczać)   
}