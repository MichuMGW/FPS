using Godot;

public class PlayerPrimaryActionCastingState : IState
{
    private readonly Player player;

    private SpellSlot slot;
    private SpellDefinition def;

    private float tickTimer;

    public PlayerPrimaryActionCastingState(Player playerContext)
    {
        player = playerContext;
    }

    public void Enter()
    {
        slot = SpellSlot.LeftHand;
        tickTimer = 0f;

        SpellInstance instance = player.Spells.GetInstance(slot);
        if (instance == null)
        {
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
            return;
        }

        def = instance.Definition;

        // Press hook
        player.Spells.Press(slot);

        // Channel: start bez cooldownu
        if (def.CastMode == SpellCastMode.Channel)
        {
            if (player.Spells.TryCast(slot, applyCooldown: false))
                player.PlayLeftArmAnimation("L_CastChannel");
            return;
        }

        // Instant / HoldRepeatCooldown: cast od razu
        if (player.Spells.TryCast(slot, applyCooldown: true))
        {
            if (player.Spells.IsCooldownShort(slot))
            {
                player.PlayLeftArmAnimation("L_CastProjectile");
            }
            else
            {
                player.PlayLeftArmAnimation("L_CastChannel");
            }
        }
            
    }

    public void Exit()
    {
        if (def == null)
            return;

        // Dla Channel kończymy emitter + cooldown
        if (def.CastMode == SpellCastMode.Channel)
        {
            player.Spells.Cancel(slot);

            SpellInstance instance = player.Spells.GetInstance(slot);
            if (instance != null)
                player.Spells.ApplyCooldown(instance);
        }
    }

    public void Update(double delta)
    {
        if (!Input.IsActionPressed("CastLeftSpell"))
        {
            player.ChangePrimaryActionState(PlayerPrimaryActionStateId.None);
            return;
        }

        float dt = (float)delta;

        // Projectile / instant: trzymasz = próbuj recastować gdy cooldown spadnie
        if (def.CastMode == SpellCastMode.HoldRepeatCooldown || def.CastMode == SpellCastMode.Instant)
        {
            if(player.Spells.TryCast(slot, applyCooldown: true) && !player.Spells.IsCooldownShort(slot))
                player.PlayLeftArmAnimation("L_CastProjectile");
            return;
        }

        // Channel: tickrate + hold update
        if (def.CastMode == SpellCastMode.Channel)
        {
            HandleChannel(dt);
            return;
        }

        // Jeśli kiedyś wrzucisz charge na primary: dorobisz analogicznie jak w secondary.
    }

    public void PhysicsUpdate(double delta) { }

    private void HandleChannel(float dt)
    {
        if (def is not ChannelSpellDefinition channelDef)
        {
            GD.PrintErr($"PrimaryActionCastingState: Spell {def.Id} has CastMode=Channel but is not ChannelSpellDefinition.");
            return;
        }

        tickTimer += dt;

        // Update emitter co frame (beam/area podąża za aim)
        player.Spells.Hold(slot, dt);

        float tickRate = Mathf.Max(0.01f, channelDef.TickRate);
        while (tickTimer >= tickRate)
        {
            tickTimer -= tickRate;

            // Tick cast bez cooldownu
            player.Spells.TryCast(slot, applyCooldown: false);
        }
    }
}
