using Godot;

public partial class SpellCastSession
{
    public readonly SpellSlot Slot;
    public readonly SpellInstance Instance;
    public readonly SpellDefinition Def;

    public readonly ISpellBehaviour Behaviour;
    public readonly IPressSpellBehaviour Press;
    public readonly IHoldSpellBehaviour Hold;
    public readonly IReleaseSpellBehaviour Release;
    public readonly ICancelableSpellBehaviour Cancel;

    // Ten sam ctx przez cały cast
    public SpellCastContext Ctx;

    // Per-mode runtime
    public float TickAcc;
    public float ChargeTimeAcc;
    public bool HasAutoCasted;

    public SpellCastSession(SpellSlot slot, SpellInstance instance, SpellCastContext ctx, ISpellBehaviour behaviour)
    {
        Slot = slot;
        Instance = instance;
        Def = instance.Definition;
        Ctx = ctx;

        Behaviour = behaviour;
        Press = behaviour as IPressSpellBehaviour;
        Hold = behaviour as IHoldSpellBehaviour;
        Release = behaviour as IReleaseSpellBehaviour;
        Cancel = behaviour as ICancelableSpellBehaviour;
    }
}
