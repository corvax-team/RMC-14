namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    public const int MaxZLevelsBelowRendering = 3;

    private const float ZGravityForce = 9.8f;
    private const float ZVelocityLimit = 20.0f;
    private const int MaxStepsPerFrame = 10;
    private const float HighGroundTransitionEdge = 0.6f;
    private const int HighGroundFallImpactSafeTileRadius = 2;
    private const float ImpactVelocityLimit = 3f;
    private const int ClientSoftBodyTrackingLimit = 4096;
    private const int ClientHardBodyTrackingLimit = 8192;
    private const float ClientBodyTrackingRecoveryCooldown = 5f;
}
