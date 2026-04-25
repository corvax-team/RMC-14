// CM14 rework: non-RMC edit marker.
using System.Linq;
using System;
using Content.Shared.CCVar;
using Content.Server.Verbs;
using Content.Shared.Examine;
using Content.Shared.Localizations;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Examine
{
    [UsedImplicitly]
    public sealed class ExamineSystem : ExamineSystemShared
    {
        [Dependency] private readonly VerbSystem _verbSystem = default!;
        [Dependency] private readonly INetConfigurationManager _netConfig = default!;
        [Dependency] private readonly ContentLocalizationManager _contentLoc = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<ExamineSystemMessages.RequestExamineInfoMessage>(ExamineInfoRequest);
        }

        public override void SendExamineTooltip(EntityUid player, EntityUid target, FormattedMessage message, bool getVerbs, bool centerAtCursor)
        {
            if (!TryComp<ActorComponent>(player, out var actor))
                return;

            var session = actor.PlayerSession;

            SortedSet<Verb>? verbs = null;
            if (getVerbs)
                verbs = _verbSystem.GetLocalVerbs(target, player, typeof(ExamineVerb));

            var ev = new ExamineSystemMessages.ExamineInfoResponseMessage(
                GetNetEntity(target), 0, message, verbs?.ToList(), centerAtCursor
            );

            RaiseNetworkEvent(ev, session.Channel);
        }

        private void ExamineInfoRequest(ExamineSystemMessages.RequestExamineInfoMessage request, EntitySessionEventArgs eventArgs)
        {
            var player = eventArgs.SenderSession;
            var session = eventArgs.SenderSession;
            var channel = player.Channel;
            var entity = GetEntity(request.NetEntity);

            if (session.AttachedEntity is not {Valid: true} playerEnt
                || !Exists(entity))
            {
                var notFound = new FormattedMessage();
                notFound.AddText(WithChannelCulture(channel, () => Loc.GetString("examine-system-entity-does-not-exist"), request.Locale));
                RaiseNetworkEvent(new ExamineSystemMessages.ExamineInfoResponseMessage(
                    request.NetEntity, request.Id, notFound), channel);
                return;
            }

            if (!CanExamine(playerEnt, entity))
            {
                var outOfRange = new FormattedMessage();
                outOfRange.AddText(WithChannelCulture(channel, () => Loc.GetString("examine-system-cant-see-entity"), request.Locale));
                RaiseNetworkEvent(new ExamineSystemMessages.ExamineInfoResponseMessage(
                    request.NetEntity, request.Id, outOfRange, knowTarget: false), channel);
                return;
            }

            SortedSet<Verb>? verbs = null;
            if (request.GetVerbs)
                verbs = _verbSystem.GetLocalVerbs(entity, playerEnt, typeof(ExamineVerb));

            var text = WithChannelCulture(channel, () => GetExamineText(entity, player.AttachedEntity), request.Locale);
            RaiseNetworkEvent(new ExamineSystemMessages.ExamineInfoResponseMessage(
                request.NetEntity, request.Id, text, verbs?.ToList()), channel);
        }

        private string GetClientLocaleCode(INetChannel channel, string? requestedLocale = null)
        {
            if (!string.IsNullOrWhiteSpace(requestedLocale))
                return requestedLocale;

            var locale = _netConfig.GetClientCVar(channel, CCVars.ClientLocale);
            return string.IsNullOrWhiteSpace(locale) ? "ru-RU" : locale;
        }

        private T WithChannelCulture<T>(INetChannel channel, Func<T> action, string? requestedLocale = null)
        {
            var locale = GetClientLocaleCode(channel, requestedLocale);
            var oldCulture = _contentLoc.CurrentCultureCode;
            if (oldCulture.Equals(locale, StringComparison.OrdinalIgnoreCase))
                return action();

            _contentLoc.SetCulture(locale);
            try
            {
                return action();
            }
            finally
            {
                _contentLoc.SetCulture(oldCulture);
            }
        }
    }
}
