using System.Text;
using Content.Shared._CMU14.Medical.Wounds;
using Content.Shared._RMC14.Medical.Unrevivable;
using Content.Shared._RMC14.Stun;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._RMC14.Medical.Examine;

public sealed class RMCMedicalExamineSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly RMCSizeStunSystem _sizeStun = default!;
    [Dependency] private readonly RMCUnrevivableSystem _unrevivable = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RMCMedicalExamineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<RMCMedicalExamineComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(RMCMedicalExamineSystem), -1))
        {
            if (ent.Comp.Simple && _mobState.IsDead(ent.Owner))
            {
                args.PushMarkup(Loc.GetString(ent.Comp.DeadText, ("victim", ent.Owner)));
                return;
            }

            if (HasComp<RMCBlockMedicalExamineComponent>(args.Examiner))
                return;

            args.PushMessage(GetExamineText(ent));
        }
    }

    public FormattedMessage GetExamineText(Entity<RMCMedicalExamineComponent> ent)
    {
        var msg = new FormattedMessage();

        if (TryComp<BloodstreamComponent>(ent, out var bloodstream) && bloodstream.BleedAmount > 0)
        {
            var partsText = GetBleedingPartsText(ent);
            if (partsText != null)
                msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.BleedFromText, ("victim", ent.Owner), ("parts", partsText)));
            else
                msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.BleedText, ("victim", ent.Owner)));
            msg.PushNewline();
        }

        LocId? stateText = null;

        if (_mobState.IsDead(ent))
            stateText = _unrevivable.IsUnrevivable(ent) ? ent.Comp.UnrevivableText : ent.Comp.DeadText;
        else if (_mobState.IsCritical(ent) || _sizeStun.IsKnockedOut(ent))
            stateText = ent.Comp.CritText;

        if (stateText != null)
            msg.AddMarkupOrThrow(Loc.GetString(stateText, ("victim", ent.Owner)));

        return msg;
    }

    private string? GetBleedingPartsText(EntityUid body)
    {
        var seen = new HashSet<(BodyPartType, BodyPartSymmetry)>();
        StringBuilder? sb = null;

        foreach (var (partUid, partComp) in _body.GetBodyChildren(body))
        {
            if (!TryComp<BodyPartWoundComponent>(partUid, out var pw))
                continue;

            var bleeding = false;
            foreach (var wound in pw.Wounds)
            {
                if (wound.Treated)
                    continue;
                if (wound.Bloodloss <= 0f)
                    continue;
                bleeding = true;
                break;
            }

            if (!bleeding)
                continue;

            if (!seen.Add((partComp.PartType, partComp.Symmetry)))
                continue;

            sb ??= new StringBuilder();
            if (sb.Length > 0)
                sb.Append(", ");
            sb.Append(FormatPart(partComp.PartType, partComp.Symmetry));
        }

        return sb?.ToString();
    }

    private static string FormatPart(BodyPartType type, BodyPartSymmetry symmetry)
    {
        var typeText = type switch
        {
            BodyPartType.Head => "head",
            BodyPartType.Torso => "torso",
            BodyPartType.Arm => "arm",
            BodyPartType.Hand => "hand",
            BodyPartType.Leg => "leg",
            BodyPartType.Foot => "foot",
            BodyPartType.Tail => "tail",
            _ => type.ToString().ToLowerInvariant(),
        };
        return symmetry switch
        {
            BodyPartSymmetry.Left => $"left {typeText}",
            BodyPartSymmetry.Right => $"right {typeText}",
            _ => typeText,
        };
    }
}
