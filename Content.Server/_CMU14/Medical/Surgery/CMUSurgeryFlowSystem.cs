using System.Collections.Generic;
using Content.Shared._CMU14.Medical;
using Content.Shared._CMU14.Medical.Surgery;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.Medical.Surgery;
using Content.Shared._RMC14.Repairable;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CMU14.Medical.Surgery;

public sealed class CMUSurgeryFlowSystem : SharedCMUSurgeryFlowSystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly CMUSurgeryDispatchSystem _dispatch = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SkillsSystem _skills = default!;

    private const float StepDoAfterSeconds = 2f;
    private static readonly EntProtoId<SkillDefinitionComponent> SurgerySkill = "RMCSkillSurgery";
    private static readonly float[] SurgeryStepDelayMultipliers = { 1.25f, 1f, 0.75f, 0.55f, 0.4f };

    private static readonly SoundSpecifier WelderStepSound = new SoundCollectionSpecifier("Welder");

    private static readonly Dictionary<string, SoundSpecifier> ToolCategorySounds = new()
    {
        ["scalpel"] = new SoundCollectionSpecifier("RMCSurgeryScalpel"),
        ["hemostat"] = new SoundCollectionSpecifier("RMCSurgeryHemostat"),
        ["retractor"] = new SoundCollectionSpecifier("RMCSurgeryRetractor"),
        ["cautery"] = new SoundCollectionSpecifier("RMCSurgeryCautery"),
        ["bone_saw"] = new SoundCollectionSpecifier("RMCSurgerySaw"),
        ["bone_setter"] = new SoundCollectionSpecifier("RMCSurgerySplint"),
        ["organ_clamp"] = new SoundCollectionSpecifier("RMCSurgeryOrgan"),
    };

    protected override void StartStepDoAfter(EntityUid patient, CMUSurgeryArmedStepComponent armed, EntityUid surgeon, EntityUid tool, EntityUid targetPart)
    {
        var delay = ResolveStepDoAfterDelay(surgeon);
        var ev = new CMUSurgeryStepDoAfterEvent(
            armed.SurgeryId,
            armed.LeafSurgeryId,
            armed.StepIndex,
            armed.TargetPartType,
            armed.TargetSymmetry);
        var doAfter = new DoAfterArgs(EntityManager, surgeon, delay, ev, patient, targetPart, tool)
        {
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            NeedHand = true,
            CancelDuplicate = false,
        };
        if (!DoAfter.TryStartDoAfter(doAfter))
            return;

        if (HasComp<BlowtorchComponent>(tool))
        {
            _audio.PlayPvs(WelderStepSound, tool);
            return;
        }

        if (armed.RequiredToolCategory is { } category
            && ToolCategorySounds.TryGetValue(category, out var sound))
        {
            _audio.PlayPvs(sound, patient);
        }
    }

    private TimeSpan ResolveStepDoAfterDelay(EntityUid surgeon)
    {
        var multiplier = _skills.GetSkillDelayMultiplier(surgeon, SurgerySkill, SurgeryStepDelayMultipliers);
        return TimeSpan.FromSeconds(StepDoAfterSeconds * multiplier);
    }

    protected override void ApplyWrongToolDamage(EntityUid surgeon, EntityUid patient, EntityUid tool, string damageType, float amount)
    {
        var multiplier = Cfg.GetCVar(CMUMedicalCCVars.SurgeryWrongToolDamageMultiplier);
        var scaled = amount * multiplier;
        if (scaled <= 0f)
        {
            // CCVar = 0 collapses Strict back to Lenient: no damage, just
            // a popup so the medic still gets the "wrong tool" feedback.
            Popup.PopupEntity(Loc.GetString("cmu-medical-surgery-wrong-tool"), patient, surgeon, PopupType.SmallCaution);
            return;
        }

        var spec = CMUWrongToolDamageTable.MakeSpec(damageType, scaled);
        _damage.TryChangeDamage(patient, spec, ignoreResistances: false, origin: surgeon);

        Popup.PopupEntity(
            Loc.GetString("cmu-medical-surgery-wrong-tool-damage", ("tool", Name(tool))),
            patient,
            surgeon,
            PopupType.MediumCaution);
    }

    protected override void RunStepEffect(EntityUid patient, CMUSurgeryArmedStepComponent armed, EntityUid surgeon, EntityUid? tool, EntityUid? targetPart)
    {
        // Resolve the step proto id from the CURRENTLY RESOLVED surgery
        // (which may be a prereq like CMSurgeryOpenIncision, not the leaf
        // the medic picked) so V1 SharedCMUSurgerySystem applies the
        // organ remove / bone set / cauterize / reattach side effects.
        var stepProtoId = ResolveStepPrototypeId(armed.SurgeryId, armed.StepIndex);
        if (stepProtoId is null)
        {
            ClearArmed(patient, armed);
            return;
        }

        if (RmcSurgery.GetSingleton(stepProtoId) is not { } stepEnt)
        {
            ClearArmed(patient, armed);
            return;
        }

        EntityUid stepPart = patient;
        if (targetPart is { } part
            && TryComp<BodyPartComponent>(part, out var targetPartComp)
            && targetPartComp.PartType == armed.TargetPartType
            && targetPartComp.Symmetry == armed.TargetSymmetry)
        {
            stepPart = part;
        }
        else if (TryFindClickedPart(patient, null, armed.TargetPartType, armed.TargetSymmetry, out var foundPart))
        {
            stepPart = foundPart;
        }

        var tools = new List<EntityUid>();
        if (tool is { } usedTool && Exists(usedTool))
            tools.Add(usedTool);

        foreach (var held in Hands.EnumerateHeld(surgeon))
        {
            if (!tools.Contains(held))
                tools.Add(held);
        }

        var stepEvent = new CMSurgeryStepEvent(surgeon, patient, stepPart, tools);
        RaiseLocalEvent(stepEnt, ref stepEvent);

        // Idempotent on subsequent steps, but EnsureSurgeryInFlight
        // refreshes the surgeon snapshot each time so a fresh surgeon
        // picking up an abandoned-but-armed surgery is credited as the
        // new operator.
        var leafId = string.IsNullOrEmpty(armed.LeafSurgeryId) ? armed.SurgeryId : armed.LeafSurgeryId;
        var leafDisplay = ResolveLeafDisplayName(leafId);
        EnsureSurgeryInFlight(patient, stepPart, surgeon, leafId, leafDisplay, armed.TargetPartType, armed.TargetSymmetry);

        if (RmcSurgery.GetSingleton(leafId) is { } leafEnt
            && TryComp<CMSurgeryComponent>(leafEnt, out var leafComp)
            && armed.SurgeryId == leafId)
        {
            if (armed.StepIndex >= leafComp.Steps.Count - 1)
            {
                var completeEvLast = new CMSurgeryCompleteEvent(patient, surgeon, leafId);
                RaiseLocalEvent(patient, ref completeEvLast);
                RemComp<CMUSurgeryArmedStepComponent>(patient);
                ClearSurgeryInFlight(patient);
                _dispatch.RefreshUiForPatient(patient);
                return;
            }

            if (TryResolveStepAt(leafId, armed.StepIndex + 1, out var nextLinear))
            {
                armed.SurgeryId = nextLinear.ResolvedSurgeryId;
                armed.StepIndex = nextLinear.StepIndex;
                armed.RequiredToolCategory = nextLinear.ToolCategory;
                armed.StepLabel = nextLinear.StepLabel;
                armed.ArmedAt = Timing.CurTime;
                Dirty(patient, armed);
                _dispatch.RefreshUiForPatient(patient);
                return;
            }
        }

        if (TryResolveNextStep(patient, stepPart, leafId, out var next))
        {
            armed.SurgeryId = next.ResolvedSurgeryId;
            armed.StepIndex = next.StepIndex;
            armed.RequiredToolCategory = next.ToolCategory;
            armed.StepLabel = next.StepLabel;
            armed.ArmedAt = Timing.CurTime;
            Dirty(patient, armed);
            _dispatch.RefreshUiForPatient(patient);
            return;
        }

        var completeEv = new CMSurgeryCompleteEvent(patient, surgeon, leafId);
        RaiseLocalEvent(patient, ref completeEv);
        RemComp<CMUSurgeryArmedStepComponent>(patient);
        ClearSurgeryInFlight(patient);
        _dispatch.RefreshUiForPatient(patient);
    }

    private string ResolveLeafDisplayName(string leafId)
    {
        if (TryGetMetadata(leafId, out var metadata))
            return metadata.DisplayName ?? leafId;
        if (Prototypes.TryIndex<EntityPrototype>(leafId, out var proto))
            return proto.Name;
        return leafId;
    }

    private string? ResolveStepPrototypeId(string surgeryId, int stepIndex)
    {
        if (!Prototypes.TryIndex<EntityPrototype>(surgeryId, out var proto))
            return null;
        if (!proto.TryGetComponent<CMSurgeryComponent>(out var surgeryComp, _compFactory))
            return null;
        if (stepIndex < 0 || stepIndex >= surgeryComp.Steps.Count)
            return null;
        return surgeryComp.Steps[stepIndex];
    }
}
