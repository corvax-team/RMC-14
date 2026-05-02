using System;
using System.Collections.Generic;
using Content.Shared._CMU14.Medical;
using Content.Shared._CMU14.Medical.Bones;
using Content.Shared._CMU14.Medical.Items;
using Content.Shared._CMU14.Medical.Wounds;
using Content.Shared._RMC14.Medical.Wounds;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Shared._CMU14.Medical.Examine;

public sealed class CMUMedicalExamineSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CMUHumanMedicalComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CMUHumanMedicalComponent> ent, ref ExaminedEvent args)
    {
        if (!_cfg.GetCVar(CMUMedicalCCVars.Enabled))
            return;

        var target = Identity.Entity(ent, EntityManager, args.Examiner);
        using (args.PushGroup(nameof(CMUMedicalExamineSystem), -1))
        {
            if (_cfg.GetCVar(CMUMedicalCCVars.WoundsEnabled))
                AddWoundLines(ent, args, target);

            if (_cfg.GetCVar(CMUMedicalCCVars.BoneEnabled))
                AddFractureLines(ent, args, target);
        }
    }

    private void AddWoundLines(EntityUid body, ExaminedEvent args, EntityUid target)
    {
        var now = _timing.CurTime;
        var partSummaries = new List<string>();

        foreach (var (partUid, part) in _body.GetBodyChildren(body))
        {
            var descriptions = new List<string>();
            if (TryComp<BodyPartWoundComponent>(partUid, out var wounds))
            {
                for (var i = 0; i < wounds.Wounds.Count; i++)
                {
                    var wound = wounds.Wounds[i];
                    var size = i < wounds.Sizes.Count ? wounds.Sizes[i] : WoundSize.Deep;
                    descriptions.Add(DescribeWound(wound, size, now));
                }
            }

            if (HasComp<CMUEscharComponent>(partUid))
                descriptions.Add("charred burn tissue");

            if (descriptions.Count == 0)
                continue;

            partSummaries.Add($"{FormatPartName(part.PartType, part.Symmetry)}: {ToSentence(descriptions)}");
        }

        if (partSummaries.Count == 0)
            return;

        args.PushMarkup(Loc.GetString(
            "cmu-medical-examine-wounds-line",
            ("target", target),
            ("parts", ToSemicolonList(partSummaries))));
    }

    private void AddFractureLines(EntityUid body, ExaminedEvent args, EntityUid target)
    {
        var partSummaries = new List<string>();

        foreach (var (partUid, part) in _body.GetBodyChildren(body))
        {
            if (!TryComp<FractureComponent>(partUid, out var fracture)
                || fracture.Severity == FractureSeverity.None)
            {
                continue;
            }

            var stabilized = HasComp<CMUSplintedComponent>(partUid) || HasComp<CMUCastComponent>(partUid);
            partSummaries.Add($"{FormatPartName(part.PartType, part.Symmetry)}: {DescribeFracture(fracture.Severity, stabilized)}");
        }

        if (partSummaries.Count == 0)
            return;

        args.PushMarkup(Loc.GetString(
            "cmu-medical-examine-fractures-line",
            ("target", target),
            ("parts", ToSemicolonList(partSummaries))));
    }

    private static string DescribeWound(Wound wound, WoundSize size, TimeSpan now)
    {
        var sizeText = size switch
        {
            WoundSize.Small => "small",
            WoundSize.Deep => "deep",
            WoundSize.Gaping => "gaping",
            WoundSize.Massive => "massive",
            _ => "deep",
        };

        var kind = wound.Type switch
        {
            WoundType.Burn => "burn",
            WoundType.Surgery => "surgical wound",
            _ => "trauma wound",
        };

        var treated = wound.Treated ? "treated " : string.Empty;
        var bleeding = !wound.Treated
            && wound.Bloodloss > 0f
            && (wound.StopBleedAt is null || now < wound.StopBleedAt.Value)
                ? " (bleeding)"
                : string.Empty;

        return $"a {treated}{sizeText} {kind}{bleeding}";
    }

    private static string DescribeFracture(FractureSeverity severity, bool stabilized)
    {
        var prefix = stabilized ? "stabilized " : string.Empty;
        return severity switch
        {
            FractureSeverity.Hairline => $"a {prefix}hairline fracture",
            FractureSeverity.Simple => $"a {prefix}broken bone",
            FractureSeverity.Compound => $"a {prefix}compound fracture",
            FractureSeverity.Comminuted => $"a {prefix}shattered bone",
            _ => "a broken bone",
        };
    }

    private static string FormatPartName(BodyPartType type, BodyPartSymmetry symmetry)
    {
        var part = type.ToString().ToLowerInvariant();
        return symmetry switch
        {
            BodyPartSymmetry.Left => $"left {part}",
            BodyPartSymmetry.Right => $"right {part}",
            _ => part,
        };
    }

    private static string ToSentence(List<string> parts)
    {
        return parts.Count switch
        {
            0 => string.Empty,
            1 => parts[0],
            2 => $"{parts[0]} and {parts[1]}",
            _ => $"{string.Join(", ", parts.GetRange(0, parts.Count - 1))}, and {parts[^1]}",
        };
    }

    private static string ToSemicolonList(List<string> parts)
    {
        return string.Join("; ", parts);
    }
}
