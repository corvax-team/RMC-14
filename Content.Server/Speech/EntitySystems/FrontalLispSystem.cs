using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random; // CCM-Localization

namespace Content.Server.Speech.EntitySystems;

public sealed class FrontalLispSystem : EntitySystem
{
    // @formatter:off
    private static readonly Regex RegexUpperTh = new(@"[T]+[Ss]+|[S]+[Cc]+(?=[IiEeYy]+)|[C]+(?=[IiEeYy]+)|[P][Ss]+|([S]+[Tt]+|[T]+)(?=[Ii]+[Oo]+[Uu]*[Nn]*)|[C]+[Hh]+(?=[Ii]*[Ee]*)|[Z]+|[S]+|[X]+(?=[Ee]+)");
    private static readonly Regex RegexLowerTh = new(@"[t]+[s]+|[s]+[c]+(?=[iey]+)|[c]+(?=[iey]+)|[p][s]+|([s]+[t]+|[t]+)(?=[i]+[o]+[u]*[n]*)|[c]+[h]+(?=[i]*[e]*)|[z]+|[s]+|[x]+(?=[e]+)");
    private static readonly Regex RegexUpperEcks = new(@"[E]+[Xx]+[Cc]*|[X]+");
    private static readonly Regex RegexLowerEcks = new(@"[e]+[x]+[c]*|[x]+");

    // CCM-Localization Start
    private static readonly Regex RegexRusS = new(@"с", RegexOptions.Compiled);
    private static readonly Regex RegexRusSUpper = new(@"С", RegexOptions.Compiled);
    private static readonly Regex RegexRusCh = new(@"ч", RegexOptions.Compiled);
    private static readonly Regex RegexRusChUpper = new(@"Ч", RegexOptions.Compiled);
    private static readonly Regex RegexRusTs = new(@"ц", RegexOptions.Compiled);
    private static readonly Regex RegexRusTsUpper = new(@"Ц", RegexOptions.Compiled);
    private static readonly Regex RegexRusT = new(@"\B[т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])", RegexOptions.Compiled);
    private static readonly Regex RegexRusTUpper = new(@"\B[Т](?![АЕЁИОУЫЭЮЯаеёиоуыэюя])", RegexOptions.Compiled);
    private static readonly Regex RegexRusZ = new(@"з", RegexOptions.Compiled);
    private static readonly Regex RegexRusZUpper = new(@"З", RegexOptions.Compiled);
    // CCM-Localization End
    // @formatter:on

    [Dependency] private readonly IRobustRandom _random = default!; // CCM-Localization

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, FrontalLispComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // handles ts, sc(i|e|y), c(i|e|y), ps, st(io(u|n)), ch(i|e), z, s
        message = RegexUpperTh.Replace(message, "TH");
        message = RegexLowerTh.Replace(message, "th");
        // handles ex(c), x
        message = RegexUpperEcks.Replace(message, "EKTH");
        message = RegexLowerEcks.Replace(message, "ekth");

        // CCM-Localization Start
        // с - ш
        message = RegexRusS.Replace(message, _ => _random.Prob(0.90f) ? "ш" : "с");
        message = RegexRusSUpper.Replace(message, _ => _random.Prob(0.90f) ? "Ш" : "С");
        // ч - ш
        message = RegexRusCh.Replace(message, _ => _random.Prob(0.90f) ? "ш" : "ч");
        message = RegexRusChUpper.Replace(message, _ => _random.Prob(0.90f) ? "Ш" : "Ч");
        // ц - ч
        message = RegexRusTs.Replace(message, _ => _random.Prob(0.90f) ? "ч" : "ц");
        message = RegexRusTsUpper.Replace(message, _ => _random.Prob(0.90f) ? "Ч" : "Ц");
        // т - ч
        message = RegexRusT.Replace(message, _ => _random.Prob(0.90f) ? "ч" : "т");
        message = RegexRusTUpper.Replace(message, _ => _random.Prob(0.90f) ? "Ч" : "Т");
        // з - ж
        message = RegexRusZ.Replace(message, _ => _random.Prob(0.90f) ? "ж" : "з");
        message = RegexRusZUpper.Replace(message, _ => _random.Prob(0.90f) ? "Ж" : "З");
        // CCM-Localization End

        args.Message = message;
    }
}
