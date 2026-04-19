using System.Text.RegularExpressions;
using Content.Server._RMC14.Speech.Components;
using Robust.Shared.Random;
using Content.Server.Speech;

namespace Content.Server._RMC14.Speech.EntitySystems;

public sealed class VulpkaninAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    // CCM-Localization-Start
    private static readonly Regex RegexLowerR = new("r+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperR = new("R+", RegexOptions.Compiled);
    
    private static readonly string[] RrrVariants = { "rr", "rrr" };
    private static readonly string[] RrrUpperVariants = { "RR", "RRR" };
    // CCM-Localization-End
    
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VulpkaninAccentComponent, AccentGetEvent>(OnAccent);
    }
    
    private void OnAccent(EntityUid uid, VulpkaninAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;
        
        // CCM-Localization-Start
        message = RegexLowerR.Replace(message, _ => _random.Pick(RrrVariants));
        message = RegexUpperR.Replace(message, _ => _random.Pick(RrrUpperVariants));
        // CCM-Localization-End
        
        args.Message = message;
    }
}