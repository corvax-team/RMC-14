using Content.Shared.EntityEffects;

namespace Content.Shared._CCM.Pathogen.EntityEffects;

public sealed class CCMEntityEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<ExecuteEntityEffectEvent<PathogenInfection>>(OnExecutePathogenInfection);
        SubscribeLocalEvent<ExecuteEntityEffectEvent<BirthBloodburster>>(OnExecuteBirthBloodburster);
    }

    private void OnExecutePathogenInfection(ref ExecuteEntityEffectEvent<PathogenInfection> args)
    {
    }

    private void OnExecuteBirthBloodburster(ref ExecuteEntityEffectEvent<BirthBloodburster> args)
    {
    }
}
