using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random; // CCM-Localization

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!; // CCM-Localization

    private static readonly Regex RegexLowerBuzz = new Regex("z{1,3}");
    private static readonly Regex RegexUpperBuzz = new Regex("Z{1,3}");

    // CCM-Localization-Start
    private static readonly Regex RegexRusLowerZh = new("ж+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperZh = new("Ж+", RegexOptions.Compiled);
    private static readonly Regex RegexRusLowerZ = new("з+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperZ = new("З+", RegexOptions.Compiled);

    private static readonly string[] ZhVariants = ["жж", "жжж"];
    private static readonly string[] ZhUpperVariants = ["ЖЖ", "ЖЖЖ"];
    private static readonly string[] ZzzVariants = ["зз", "ззз"];
    private static readonly string[] ZzzUpperVariants = ["ЗЗ", "ЗЗЗ"];
    // CCM-Localization-End

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // buzzz
        message = RegexLowerBuzz.Replace(message, "zzz");
        // buZZZ
        message = RegexUpperBuzz.Replace(message, "ZZZ");

        // CCM-Localization-Start
        // ж => жжж
        message = RegexRusLowerZh.Replace(message, _ => _random.Pick(ZhVariants));
        // Ж => ЖЖЖ
        message = RegexRusUpperZh.Replace(message, _ => _random.Pick(ZhUpperVariants));
        // з => ззз
        message = RegexRusLowerZ.Replace(message, _ => _random.Pick(ZzzVariants));
        // З => ЗЗЗ
        message = RegexRusUpperZ.Replace(message, _ => _random.Pick(ZzzUpperVariants));
        // CCM-Localization-End

        args.Message = message;
    }
}
