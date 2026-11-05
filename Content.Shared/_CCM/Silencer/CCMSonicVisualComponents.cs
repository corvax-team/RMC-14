namespace Content.Shared._CCM.Silencer;

/// <summary>
///     Sonic carbine bullet drawn by CCMSonicCarbineOverlay instead of a sprite. Client-only marker.
/// </summary>
[RegisterComponent]
public sealed partial class CCMSonicBulletVisualsComponent : Component
{
}

[RegisterComponent]
public sealed partial class CCMSonicImpactEffectComponent : Component
{
    // Must match TimedDespawn lifetime.
    [DataField]
    public float Duration = 0.5f;

    [ViewVariables]
    public TimeSpan Start;

    [ViewVariables]
    public float Seed;
}

/// <summary>
///     Sonic muzzle flash spawned through AmmoComponent.MuzzleFlash, drawn by CCMSonicCarbineOverlay.
/// </summary>
[RegisterComponent]
public sealed partial class CCMSonicMuzzleFlashComponent : Component
{
    // Must match TimedDespawn lifetime.
    [DataField]
    public float Duration = 0.3f;

    [DataField]
    public float MuzzleOffset = 0.33f;

    [ViewVariables]
    public TimeSpan Start;

    [ViewVariables]
    public float Seed;

    [ViewVariables]
    public bool LightTinted;
}
