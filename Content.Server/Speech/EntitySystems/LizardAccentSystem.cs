using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random; // CCM-Localization

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!; // CCM-Localization

    private static readonly Regex RegexLowerS = new("s+");
    private static readonly Regex RegexUpperS = new("S+");
    private static readonly Regex RegexInternalX = new(@"(\w)x");
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)");
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)");

    // CCM-Localization-Start
    private static readonly Regex RegexRusLowerS = new("с+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperS = new("С+", RegexOptions.Compiled);
    private static readonly Regex RegexRusLowerZ = new("з+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperZ = new("З+", RegexOptions.Compiled);
    private static readonly Regex RegexRusLowerSh = new("ш+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperSh = new("Ш+", RegexOptions.Compiled);
    private static readonly Regex RegexRusLowerCh = new("ч+", RegexOptions.Compiled);
    private static readonly Regex RegexRusUpperCh = new("Ч+", RegexOptions.Compiled);

    private static readonly string[] SssVariants = ["сс", "ссс"];
    private static readonly string[] SssUpperVariants = ["СС", "ССС"];
    private static readonly string[] ShhhVariants = ["шш", "шшш"];
    private static readonly string[] ShhhUpperVariants = ["ШШ", "ШШШ"];
    private static readonly string[] SchVariants = ["щщ", "щщш"];
    private static readonly string[] SchUpperVariants = ["ЩЩ", "ЩЩШ"];
    // CCM-Localization-End

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");

        // CCM-Localization-Start
        // с => ссс
        message = RegexRusLowerS.Replace(message, _ => _random.Pick(SssVariants));
        // С => CCC
        message = RegexRusUpperS.Replace(message, _ => _random.Pick(SssUpperVariants));
        // з => ссс
        message = RegexRusLowerZ.Replace(message, _ => _random.Pick(SssVariants));
        // З => CCC
        message = RegexRusUpperZ.Replace(message, _ => _random.Pick(SssUpperVariants));
        // ш => шшш
        message = RegexRusLowerSh.Replace(message, _ => _random.Pick(ShhhVariants));
        // Ш => ШШШ
        message = RegexRusUpperSh.Replace(message, _ => _random.Pick(ShhhUpperVariants));
        // ч => щщщ
        message = RegexRusLowerCh.Replace(message, _ => _random.Pick(SchVariants));
        // Ч => ЩЩЩ
        message = RegexRusUpperCh.Replace(message, _ => _random.Pick(SchUpperVariants));
        // CCM-Localization-End
        args.Message = message;
    }
}
