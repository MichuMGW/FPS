using Godot;

public class PlayerSecondaryActionCastingState : IState
{
    private readonly Player player;

    private SpellSlot slot;
    private SpellDefinition def;

    private float charge;
    private bool hasCast;
    private float tickTimer;

    public PlayerSecondaryActionCastingState(Player playerContext) => player = playerContext;

    public void Enter()
    {
        slot = player.CurrentSecondaryCastingSlot;
        charge = 0f;
        hasCast = false;
        tickTimer = 0f;

        SpellInstance instance = player.Spells.GetInstance(slot);
        if (instance == null)
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        def = instance.Definition;

        // Jeśli to charge i spell jest na cooldownie, nie zaczynaj w ogóle (żadnego indicatora)
        if ((def.CastMode == SpellCastMode.ChargeRelease || def.CastMode == SpellCastMode.ChargeAuto) && !instance.CanCast)
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        // Press hook (tworzy indicator dla charge)
        player.Spells.Press(slot);

        if (def.CastMode == SpellCastMode.Instant)
        {
            if (player.Spells.TryCast(slot, applyCooldown: true))
                player.PlayRightArmAnimation("R_CastProjectile");

            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (def.CastMode == SpellCastMode.Channel)
        {
            if (player.Spells.TryCast(slot, applyCooldown: false))
                player.PlayRightArmAnimation("R_CastChannel");
        }

        if (def.CastMode == SpellCastMode.ChargeRelease || def.CastMode == SpellCastMode.ChargeAuto)
        {
            player.PlayRightArmAnimation("R_CastPrepare");
            return;
        }

        if (def.CastMode == SpellCastMode.HoldRepeatCooldown)
        {
            if (player.Spells.TryCast(slot, applyCooldown: true))
                player.PlayRightArmAnimation("R_CastProjectile");
        }
    }

    public void Exit()
    {
        if (def == null)
            return;

        if (def.CastMode == SpellCastMode.Channel)
        {
            player.Spells.Cancel(slot);

            SpellInstance instance = player.Spells.GetInstance(slot);
            if (instance != null)
                player.Spells.ApplyCooldown(instance);
        }

        // Jeśli wyszedłeś z charge przez cancel/przerwanie, sprzątnij indicator
        if (def.CastMode == SpellCastMode.ChargeRelease || def.CastMode == SpellCastMode.ChargeAuto)
        {
            player.Spells.Cancel(slot);
        }
    }

    public void Update(double delta)
    {
        float dt = (float)delta;

        if (Input.IsActionJustPressed("CastDash") && slot != SpellSlot.Dash)
        {
            player.Spells.Cancel(slot);
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (slot == SpellSlot.Dash)
        {
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        if (!IsHeld(slot))
        {
            HandleRelease();
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
            return;
        }

        switch (def.CastMode)
        {
            case SpellCastMode.HoldRepeatCooldown:
                player.Spells.TryCast(slot, applyCooldown: true);
                break;

            case SpellCastMode.Channel:
                HandleChannel(dt);
                break;

            case SpellCastMode.ChargeRelease:
                HandleCharge(dt, autoCast: false);
                break;

            case SpellCastMode.ChargeAuto:
                HandleCharge(dt, autoCast: true);
                break;
        }
    }

    public void PhysicsUpdate(double delta) { }

    private void HandleChannel(float dt)
    {
        if (def is not ChannelSpellDefinition channelDef)
        {
            GD.PrintErr($"SecondaryActionCastingState: Spell {def.Id} has CastMode=Channel but is not ChannelSpellDefinition.");
            return;
        }

        tickTimer += dt;

        player.Spells.Hold(slot, dt);

        float tickRate = Mathf.Max(0.01f, channelDef.TickRate);
        while (tickTimer >= tickRate)
        {
            tickTimer -= tickRate;
            player.Spells.TryCast(slot, applyCooldown: false);
        }
    }

    private void HandleCharge(float dt, bool autoCast)
    {
        float chargeTime = GetChargeTimeOrFallback(def);

        charge += dt;

        // update indicator
        SpellCastContext ctx = player.Spells.BuildContext(slot);
        if (ctx.Caster != null)
        {
            ctx.Charge01 = Mathf.Clamp(charge / Mathf.Max(0.01f, chargeTime), 0f, 1f);

            var behaviour = player.Spells.GetBehaviourForDebug(def);
            if (behaviour is IHoldSpellBehaviour hold)
                hold.OnHeld(ctx, dt);
        }

        if (autoCast && !hasCast && charge >= chargeTime)
        {
            // Auto-cast = release bez puszczania
            bool didCast = ReleaseCharge();
            if (didCast)
                player.PlayRightArmAnimation("R_CastRelease");

            hasCast = true;
            player.ChangeSecondaryActionState(PlayerSecondaryActionStateId.None);
        }
    }

    private void HandleRelease()
    {
        if (def.CastMode != SpellCastMode.ChargeRelease)
            return;

        bool didCast = ReleaseCharge();
        if (didCast)
            player.PlayRightArmAnimation("R_CastRelease");
    }

    private bool ReleaseCharge()
    {
        float chargeTime = GetChargeTimeOrFallback(def);

        SpellCastContext ctx = player.Spells.BuildContext(slot);
        if (ctx.Caster == null)
            return false;

        ctx.Charge01 = Mathf.Clamp(charge / Mathf.Max(0.01f, chargeTime), 0f, 1f);

        var behaviour = player.Spells.GetBehaviourForDebug(def);
        if (behaviour is not IReleaseSpellBehaviour release)
            return false;

        bool didCast = release.OnReleased(ctx);

        if (didCast)
        {
            SpellInstance instance = player.Spells.GetInstance(slot);
            if (instance != null)
                player.Spells.ApplyCooldown(instance);
        }

        return didCast;
    }

    private bool IsHeld(SpellSlot s)
    {
        if (s == SpellSlot.RightHand)
            return Input.IsActionPressed("CastRightSpell");

        return false;
    }

    private float GetChargeTimeOrFallback(SpellDefinition spellDef)
    {
        if (spellDef is ExplosionSpellDefinition explosion)
            return Mathf.Max(0.01f, explosion.ChargeTime);

        GD.PrintErr($"SecondaryActionCastingState: Spell {spellDef.Id} uses Charge cast mode but has no ChargeTime (not ExplosionSpellDefinition). Using fallback 0.75s.");
        return 0.75f;
    }
}
