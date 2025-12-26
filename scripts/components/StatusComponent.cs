using Godot;

public partial class StatusComponent : Node
{
    [Export] public NodePath HurtboxComponentPath = "../HurtboxComponent";

    // Opcjonalnie: jak chcesz, możesz tu trzymać listę profili dla elementów (np. z kitu),
    // ale w Twoim podejściu profil idzie w HitInfo, więc nie jest to wymagane.

    private HurtboxComponent _hurtbox;

    // ---------------- Burning runtime ----------------
    private bool _burning;
    private float _burningTimeLeft;
    private float _burningTickRate;
    private float _burningTickAcc;
    private float _burningDotPerTick;

    // ---------------- Slow runtime ----------------
    private bool _slowed;
    private float _slowTimeLeft;
    private float _slowPercent;      // 0.3 = -30%
    private float _speedMultiplier;  // 0.7 = 70% speed

    // ---------------- Bleed runtime ----------------
    private bool _bleeding;
    private float _bleedTimeLeft;
    private float _bleedTickRate;
    private float _bleedTickAcc;
    private float _bleedDotPerTick;

    // ---------------- Earth runtime ----------------
    private float _earthBuildup;
    private float _earthDecayPerSecond;

    private bool _stunned;
    private float _stunTimeLeft;

	// components
	private HealthComponent _health;
	private VelocityComponent _velocity;

    public override void _Ready()
    {
        _hurtbox = GetNodeOrNull<HurtboxComponent>(HurtboxComponentPath);
        if (_hurtbox == null)
        {
            GD.PushError($"{Name}: StatusComponent missing HurtboxComponent at '{HurtboxComponentPath}'.");
            return;
        }

        _hurtbox.Hit += OnHit;

		_health = GetOwner().GetNodeOrNull<HealthComponent>("HealthComponent");
		_velocity = GetOwner().GetNodeOrNull<VelocityComponent>("VelocityComponent");

        // ApplyMoveSpeedMultiplier(1f);
        // ApplyStunState(false);
    }

    public override void _ExitTree()
    {
        if (_hurtbox != null)
            _hurtbox.Hit -= OnHit;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        TickBurning(dt);
        TickBleed(dt);
        TickSlow(dt);
        TickEarth(dt);
        TickStun(dt);

    }

    private void OnHit(HitInfo hit)
    {
        if (hit == null)
            return;

        var profile = hit.StatusProfile;
        if (profile == null)
            return;

        // Jeśli chcesz sanity-check: profil powinien pasować do elementu hita
        // (czasem będziesz chciał w Dark mieć burning+slow, więc to tylko ostrzeżenie)
        if (profile.Element != hit.Element && profile.Element != Element.None)
        {
            GD.PushWarning($"{Name}: Hit profile element {profile.Element} differs from hit element {hit.Element}.");
        }

        // Burning
        if (profile.EnableBurning)
        {
            // U Ciebie: base dot per tick z profilu, skalowanie ze spella (np. Fireball vs Explosion)
            float dot = profile.BurningDotPerTick * Mathf.Max(0f, hit.BurningDotMultiplier);

            ApplyBurning(
                duration: profile.BurningDuration,
                tickRate: Mathf.Max(0.01f, profile.BurningTickRate),
                dotPerTick: dot
            );
        }

        // Slow
        if (profile.EnableSlow)
        {
            // profile.SlowMultiplier = 0.3 oznacza -30%
            // hit.SlowBonus (jeśli masz) możesz potraktować jako dodatkowy % slowa, np. +0.1 => -40%
            float slowPercent = Mathf.Clamp(profile.SlowMultiplier + hit.SlowBonus, 0f, 0.95f);

            ApplySlow(
                duration: profile.SlowDuration,
                slowPercent: slowPercent
            );
        }

        // Bleed
        if (profile.EnableBleed)
        {
            float dot = profile.BleedDotPerTick * Mathf.Max(0f, hit.BleedDotMultiplier);

            ApplyBleed(
                duration: profile.BleedDuration,
                tickRate: Mathf.Max(0.01f, profile.BleedTickRate),
                dotPerTick: dot
            );
        }

        // Earth buildup + stun
        if (profile.EnableEarthBuildup)
        {
            ApplyEarthBuildup(
                add: Mathf.Max(0f, hit.EarthBuildupPerHit),
                threshold: Mathf.Max(1f, profile.EarthStunThreshold),
                decayPerSec: Mathf.Max(0f, profile.EarthBuildupDecayPerSecond),
                stunDuration: Mathf.Max(0.01f, profile.EarthStunDuration)
            );
        }
    }

    // ====================== Burning ======================

    private void ApplyBurning(float duration, float tickRate, float dotPerTick)
    {
        if (duration <= 0f || dotPerTick <= 0f)
            return;

        // refresh duration, intensywność bierzemy max (FireExplosion > Fireball)
        _burning = true;
        _burningTimeLeft = Mathf.Max(_burningTimeLeft, duration);
        _burningTickRate = tickRate;

        _burningDotPerTick = Mathf.Max(_burningDotPerTick, dotPerTick);

        // nie resetuję akumulatora ticków, żeby nie było “free tick” spam przy spam-hitach
    }

    private void TickBurning(float dt)
    {
        if (!_burning)
            return;

        _burningTimeLeft -= dt;
        if (_burningTimeLeft <= 0f)
        {
            _burning = false;
            _burningTickAcc = 0f;
            _burningDotPerTick = 0f;
            return;
        }

        _burningTickAcc += dt;
        while (_burningTickAcc >= _burningTickRate)
        {
            _burningTickAcc -= _burningTickRate;
            DealStatusDamage(_burningDotPerTick, Element.Fire);
        }
    }

    // ====================== Bleed ======================

    private void ApplyBleed(float duration, float tickRate, float dotPerTick)
    {
        if (duration <= 0f || dotPerTick <= 0f)
            return;

        _bleeding = true;
        _bleedTimeLeft = Mathf.Max(_bleedTimeLeft, duration);
        _bleedTickRate = tickRate;

        _bleedDotPerTick = Mathf.Max(_bleedDotPerTick, dotPerTick);
    }

    private void TickBleed(float dt)
    {
        if (!_bleeding)
            return;

        _bleedTimeLeft -= dt;
        if (_bleedTimeLeft <= 0f)
        {
            _bleeding = false;
            _bleedTickAcc = 0f;
            _bleedDotPerTick = 0f;
            return;
        }

        _bleedTickAcc += dt;
        while (_bleedTickAcc >= _bleedTickRate)
        {
            _bleedTickAcc -= _bleedTickRate;
            DealStatusDamage(_bleedDotPerTick, Element.Nature);
        }
    }

    // ====================== Slow ======================

    private void ApplySlow(float duration, float slowPercent)
    {
        if (duration <= 0f)
            return;

        _slowed = true;
        _slowTimeLeft = Mathf.Max(_slowTimeLeft, duration);

        // bierzemy “najgorszy” slow (większy %)
        _slowPercent = Mathf.Max(_slowPercent, slowPercent);

        // speed multiplier = 1 - slowPercent
        _speedMultiplier = Mathf.Clamp(1f - _slowPercent, 0.05f, 1f);
        ApplyMoveSpeedMultiplier(_speedMultiplier);
    }

    private void TickSlow(float dt)
    {
        if (!_slowed)
            return;

        _slowTimeLeft -= dt;
        if (_slowTimeLeft <= 0f)
        {
            _slowed = false;
            _slowPercent = 0f;
            _speedMultiplier = 1f;
            ApplyMoveSpeedMultiplier(1f);
        }
    }

    // ====================== Earth buildup ======================

    private void ApplyEarthBuildup(float add, float threshold, float decayPerSec, float stunDuration)
    {
        if (add <= 0f)
            return;

        _earthBuildup += add;

        // decay ustawiamy “na czas trwania buildupu”
        _earthDecayPerSecond = Mathf.Max(_earthDecayPerSecond, decayPerSec);

        if (!_stunned && _earthBuildup >= threshold)
        {
            ApplyStun(stunDuration);

            // po stunie reset buildupu (najczyściej)
            _earthBuildup = 0f;
        }
    }

    private void TickEarth(float dt)
    {
        if (_earthBuildup <= 0f)
            return;

        if (_earthDecayPerSecond <= 0f)
            return;

        _earthBuildup = Mathf.Max(0f, _earthBuildup - _earthDecayPerSecond * dt);

        // jak spadnie do zera, czyścimy decay, bo inaczej zostaje “na zawsze”
        if (_earthBuildup <= 0f)
            _earthDecayPerSecond = 0f;
    }

    // ====================== Stun ======================

    private void ApplyStun(float duration)
    {
        _stunned = true;
        _stunTimeLeft = Mathf.Max(_stunTimeLeft, duration);
        ApplyStunState(true);
    }

    private void TickStun(float dt)
    {
        if (!_stunned)
            return;

        _stunTimeLeft -= dt;
        if (_stunTimeLeft <= 0f)
        {
            _stunned = false;
            ApplyStunState(false);
        }
    }

    // ====================== Integration hooks ======================

    private void DealStatusDamage(float amount, Element sourceElement)
    {
		_health.ApplyStatusDamage(amount, sourceElement);
    }

    private void ApplyMoveSpeedMultiplier(float mult)
    {
        _velocity.SetMoveSpeedMultiplier(mult);
    }

	//DO OGARNIĘCIA: obsługa stuna w AI
    private void ApplyStunState(bool stunned)
    {
        var owner = GetOwner();
        if (owner == null)
            return;

        var brain = owner.GetNodeOrNull<Node>("AIComponent");
        if (brain != null && brain.HasMethod("SetStunned"))
            brain.Call("SetStunned", stunned);
    }
}
