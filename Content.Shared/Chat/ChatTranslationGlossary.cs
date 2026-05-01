// CM14 rework: non-RMC edit marker.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Content.Shared.Chat;

public static class ChatTranslationGlossary
{
    private const string TokenPrefix = "QZXTERM";
    private const string TokenSuffix = "ZXQ";

    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex TrailingPunctuationRegex = new(@"(?<punct>[.!?]+)$", RegexOptions.Compiled);
    private static readonly Regex EmojiTokenRegex = new(
        @"(?<!\\):[A-Za-z][A-Za-z0-9_]{0,31}:|(?<!\\)\[[A-Za-z][A-Za-z0-9_]{0,31}\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RichMarkupTagRegex = new(
        @"\[(\/)?[a-z]+(?:[ =][^\]]+)?\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly GlossaryPair[] PhraseGlossary =
    [
        // Movement / combat calls.
        new("go away", "отойди"),
        new("get away", "отойди"),
        new("move away", "отойди"),
        new("back off", "отойди"),
        new("step back", "отойдите"),
        new("move back", "отходите"),
        new("leave me alone", "оставь меня в покое"),
        new("come here", "иди сюда"),
        new("come to me", "иди ко мне"),
        new("follow me", "за мной"),
        new("stay here", "оставайся здесь"),
        new("hold here", "держим тут"),
        new("hold position", "держите позицию"),
        new("fall back", "отступаем"),
        new("retreat", "отступаем"),
        new("push up", "продвигаемся"),
        new("push", "давим"),
        new("move out", "выдвигаемся"),
        new("run away", "беги отсюда"),
        new("stop", "стой"),
        new("wait", "подожди"),
        new("wait here", "жди здесь"),
        new("one sec", "секунду"),
        new("give me a sec", "дай секунду"),
        new("hold on", "подожди"),
        new("hold up", "стой"),
        new("cover me", "прикрой меня"),
        new("get down", "пригнись"),
        new("spread out", "рассредоточиться"),
        new("regroup", "собраться"),
        new("rally up", "собраться"),
        new("fall in", "стройся"),
        new("stack up", "стекаемся"),
        new("breach", "ломаемся внутрь"),
        new("breach it", "врываемся"),
        new("open fire", "огонь"),
        new("cease fire", "прекратить огонь"),
        new("hold fire", "не стрелять"),
        new("suppressing", "подавляю"),
        new("flank left", "обход слева"),
        new("flank right", "обход справа"),
        new("watch left", "смотри влево"),
        new("watch right", "смотри вправо"),
        new("on me", "ко мне"),
        new("with me", "за мной"),
        new("clear", "чисто"),
        new("danger close", "опасно близко"),
        new("contact", "контакт"),
        new("contact front", "контакт спереди"),
        new("contact north", "контакт на севере"),
        new("contact south", "контакт на юге"),
        new("contact east", "контакт на востоке"),
        new("contact west", "контакт на западе"),
        new("go north", "иди на север"),
        new("go south", "иди на юг"),
        new("go east", "иди на восток"),
        new("go west", "иди на запад"),
        new("north", "север"),
        new("south", "юг"),
        new("east", "восток"),
        new("west", "запад"),

        // Medical / logistics.
        new("help", "помогите"),
        new("help me", "помогите мне"),
        new("need medic", "нужен медик"),
        new("need a medic", "нужен медик"),
        new("need doctor", "нужен врач"),
        new("need surgery", "нужна операция"),
        new("need evac", "нужна эвакуация"),
        new("need medevac", "нужен медэвак"),
        new("heal me", "полечи меня"),
        new("i need healing", "мне нужна помощь медика"),
        new("need ammo", "нужны патроны"),
        new("need mags", "нужны магазины"),
        new("need supplies", "нужны припасы"),
        new("need materials", "нужны материалы"),
        new("need cades", "нужны баррикады"),
        new("need welding fuel", "нужно сварочное топливо"),
        new("reload", "перезарядка"),
        new("out of ammo", "нет патронов"),
        new("low ammo", "мало патронов"),
        new("drag me", "тащи меня"),
        new("revive me", "подними меня"),
        new("defib me", "дефибни меня"),
        new("i am dead", "я мертв"),
        new("i am crit", "я в крите"),
        new("i am down", "я лежу"),
        new("i am unconscious", "я без сознания"),
        new("ssd", "ссд"),

        // Short chat slang.
        new("omw", "иду"),
        new("brb", "сейчас вернусь"),
        new("afk", "афк"),
        new("idk", "не знаю"),
        new("imo", "по-моему"),
        new("imho", "по-моему"),
        new("nvm", "забей"),
        new("np", "не за что"),
        new("no problem", "без проблем"),
        new("thx", "спасибо"),
        new("ty", "спасибо"),
        new("pls", "пожалуйста"),
        new("pls help", "помогите пожалуйста"),
        new("plz", "пожалуйста"),
        new("bro", "бро"),
        new("bruh", "брух"),
        new("fr", "серьезно"),
        new("for real", "серьезно"),
        new("wtb", "нужно"),
        new("lfg", "ищу группу"),
        new("cya", "увидимся"),
        new("gn", "спокойной ночи"),
        new("lol", "лол"),
        new("lmao", "лмао"),
        new("wtf", "какого хрена"),
        new("ffs", "да блин"),
        new("gg", "гг"),
        new("good game", "хорошая игра"),
        new("gj", "хорошая работа"),
        new("good job", "хорошая работа"),
        new("nice", "неплохо"),
        new("lets go", "погнали"),
        new("go go go", "вперед вперед вперед"),
        new("roger", "принял"),
        new("copy", "принял"),
        new("wilco", "выполняю"),
        new("thanks", "спасибо"),
        new("thank you", "спасибо"),
        new("sorry", "извини"),
        new("my bad", "мой косяк"),
        new("yes", "да"),
        new("no", "нет"),
        new("ok", "окей"),
        new("okay", "окей"),

        // Generic insults / profanity. These are kept as direct chat translations, not moderation.
        new("fuck", "блядь"),
        new("shit", "дерьмо"),
        new("fuck off", "отвали"),
        new("piss off", "отвали"),
        new("shut up", "заткнись"),
        new("shut the fuck up", "заткнись нахуй"),
        new("go fuck yourself", "иди нахуй"),
        new("fuck you", "пошел нахуй"),
        new("what the fuck", "какого хуя"),
        new("idiot", "идиот"),
        new("moron", "дебил"),
        new("dumbass", "тупица"),
        new("asshole", "мудак"),
        new("bitch", "сука"),
        new("bastard", "ублюдок"),
        new("piece of shit", "кусок дерьма"),
        new("holy shit", "нихуя себе"),
        new("bullshit", "хуйня"),
        new("you suck", "ты отстой"),
        new("stupid", "тупой"),
    ];

    private static readonly GlossaryPair[] ProtectedGlossary =
    [
        // Phrase chunks that LibreTranslate often renders too literally in SS14 chat.
        new("go away", "отойди"),
        new("get away", "отойди"),
        new("move away", "отойди"),
        new("back off", "отойди"),
        new("fall back", "отступаем"),
        new("danger close", "опасно близко"),
        new("cover me", "прикрой меня"),
        new("hold position", "держите позицию"),
        new("need medic", "нужен медик"),
        new("need a medic", "нужен медик"),
        new("need ammo", "нужны патроны"),
        new("out of ammo", "нет патронов"),
        new("low ammo", "мало патронов"),
        new("need surgery", "нужна операция"),
        new("need evac", "нужна эвакуация"),
        new("need medevac", "нужен медэвак"),
        new("drag me", "тащи меня"),
        new("revive me", "подними меня"),
        new("defib me", "дефибни меня"),
        new("i am dead", "я мертв"),
        new("i am crit", "я в крите"),
        new("i am down", "я лежу"),
        new("i am unconscious", "я без сознания"),
        new("fuck off", "отвали"),
        new("piss off", "отвали"),
        new("shut up", "заткнись"),
        new("shut the fuck up", "заткнись нахуй"),
        new("go fuck yourself", "иди нахуй"),
        new("fuck you", "пошел нахуй"),
        new("what the fuck", "какого хуя"),
        new("piece of shit", "кусок дерьма"),
        new("holy shit", "нихуя себе"),
        new("bullshit", "хуйня"),

        // TGMC/RMC locations and command terms.
        new("Almayer", "Альмайер"),
        new("USS Almayer", "USS Альмайер"),
        new("Alamo", "Аламо"),
        new("Normandy", "Нормандия"),
        new("Sulaco", "Сулако"),
        new("medbay", "медбей"),
        new("medical", "медотсек"),
        new("brig", "бриг"),
        new("CIC", "CIC"),
        new("comms", "связь"),
        new("tcomms", "телекомы"),
        new("engineering", "инженерка"),
        new("hangar", "ангар"),
        new("briefing", "брифинг"),
        new("requisitions", "реква"),
        new("req", "реква"),
        new("req line", "линия реквы"),
        new("hydroponics", "гидропоника"),
        new("hydro", "гидра"),
        new("dropship", "дропшип"),
        new("dropship one", "дропшип 1"),
        new("dropship two", "дропшип 2"),
        new("lifeboat", "спасательная шлюпка"),
        new("FOB", "FOB"),
        new("LZ", "LZ"),
        new("CAS", "CAS"),
        new("OB", "OB"),
        new("DEFCON", "DEFCON"),
        new("ASRS", "ASRS"),
        new("JTAC", "JTAC"),
        new("medevac", "медэвак"),
        new("ETA", "ETA"),

        // Marine roles and abbreviations.
        new("marine", "морпех"),
        new("marines", "морпехи"),
        new("squad", "отряд"),
        new("squad leader", "сквад-лидер"),
        new("fireteam leader", "командир звена"),
        new("Alpha squad", "отряд Альфа"),
        new("Bravo squad", "отряд Браво"),
        new("Charlie squad", "отряд Чарли"),
        new("Delta squad", "отряд Дельта"),
        new("PFC", "PFC"),
        new("SL", "SL"),
        new("FTL", "FTL"),
        new("SG", "SG"),
        new("smartgunner", "смартганнер"),
        new("spec", "спек"),
        new("specialist", "специалист"),
        new("engineer", "инженер"),
        new("combat technician", "техник"),
        new("corpsman", "медик"),
        new("medic", "медик"),
        new("doctor", "врач"),
        new("researcher", "исследователь"),
        new("synthetic", "синтет"),
        new("synth", "синтет"),
        new("commander", "командир"),
        new("commanding officer", "CO"),
        new("executive officer", "XO"),
        new("staff officer", "SO"),
        new("requisitions officer", "RO"),
        new("military police", "MP"),
        new("provost", "провост"),
        new("pilot", "пилот"),
        new("intel", "разведка"),
        new("CO", "CO"),
        new("XO", "XO"),
        new("SO", "SO"),
        new("RO", "RO"),
        new("MP", "MP"),
        new("CMP", "CMP"),
        new("CE", "CE"),
        new("CMO", "CMO"),
        new("PO", "PO"),
        new("SEA", "SEA"),

        // Xeno / enemy terms.
        new("xeno", "ксено"),
        new("xenos", "ксено"),
        new("alien", "чужой"),
        new("aliens", "чужие"),
        new("hive", "улей"),
        new("queen", "королева"),
        new("queen dead", "королева мертва"),
        new("larva", "личинка"),
        new("drone", "дрон"),
        new("hivelord", "хайвлорд"),
        new("carrier", "карриер"),
        new("burrower", "бурровер"),
        new("warrior", "варриор"),
        new("defender", "дефендер"),
        new("crusher", "крашер"),
        new("ravager", "равагер"),
        new("praetorian", "преторианец"),
        new("boiler", "бойлер"),
        new("spitter", "спиттер"),
        new("runner", "раннер"),
        new("lurker", "луркер"),
        new("sentinel", "сентинель"),
        new("widow", "видова"),
        new("screech", "скрич"),
        new("boiler gas", "газ бойлера"),
        new("egg", "яйцо"),
        new("eggs", "яйца"),
        new("tunnel", "тоннель"),
        new("weeds", "сорняки"),
        new("weeds up", "сорняки стоят"),
        new("weeds down", "сорняки снесены"),
        new("resin", "смола"),
        new("resin door", "смоляная дверь"),
        new("resin wall", "смоляная стена"),

        // Weapons, meds, supplies.
        new("defib", "дефиб"),
        new("defibrillator", "дефибриллятор"),
        new("stasis bag", "стазис-мешок"),
        new("roller bed", "каталка"),
        new("splint", "шина"),
        new("kelotane", "келотан"),
        new("bicardine", "бикардин"),
        new("tramadol", "трамадол"),
        new("MRE", "MRE"),
        new("pulse rifle", "импульсная винтовка"),
        new("shotgun", "дробовик"),
        new("flamer", "огнемет"),
        new("flamethrower", "огнемет"),
        new("grenade", "граната"),
        new("C4", "C4"),
        new("claymore", "клеймор"),
        new("motion detector", "датчик движения"),
        new("welder", "сварка"),
        new("barricade", "баррикада"),
        new("barricades", "баррикады"),
        new("cade", "баррикада"),
        new("cades", "баррикады"),
        new("sentry", "турель"),
        new("APC", "APC"),
        new("AP", "AP"),
        new("HE", "HE"),
        new("HEFA", "HEFA"),
        new("NVG", "NVG"),

        // Common combat chat chunks.
        new("hold here", "держим тут"),
        new("hold this", "держим это место"),
        new("hold north", "держим север"),
        new("hold south", "держим юг"),
        new("hold east", "держим восток"),
        new("hold west", "держим запад"),
        new("push north", "давим на север"),
        new("push south", "давим на юг"),
        new("push east", "давим на восток"),
        new("push west", "давим на запад"),
        new("fall back to fob", "отходим к FOB"),
        new("back to fob", "назад к FOB"),
        new("left flank", "левый фланг"),
        new("right flank", "правый фланг"),
        new("frontline", "передовая"),
        new("backline", "тыл"),
        new("choke point", "узкий проход"),
        new("main hall", "главный коридор"),

        // Single generic insults that are useful to preserve in mixed chat lines.
        new("idiot", "идиот"),
        new("moron", "дебил"),
        new("dumbass", "тупица"),
        new("asshole", "мудак"),
        new("bitch", "сука"),
        new("bastard", "ублюдок"),
        new("stupid", "тупой"),
    ];

    public static bool TryTranslateDirect(string text, string target, out string translated)
    {
        translated = string.Empty;

        if (!TryNormalizeLanguage(target, out var normalizedTarget))
            return false;

        var normalized = NormalizePhrase(text, out var punctuation);
        if (normalized.Length == 0)
            return false;

        foreach (var entry in PhraseGlossary)
        {
            var source = normalizedTarget == "ru" ? entry.En : entry.Ru;
            if (!string.Equals(normalized, NormalizePhrase(source, out _), StringComparison.Ordinal))
                continue;

            var value = normalizedTarget == "ru" ? entry.Ru : entry.En;
            translated = ApplyTrailingPunctuation(value, punctuation);
            return true;
        }

        return false;
    }

    public static PreparedTranslation PrepareForTranslation(string text, string target)
    {
        if (!TryNormalizeLanguage(target, out var normalizedTarget))
            return new PreparedTranslation(text, []);

        var prepared = text;
        var protectedTerms = new List<ProtectedTerm>();
        prepared = ProtectEmojiTokens(prepared, protectedTerms);
        prepared = ProtectMarkupTokens(prepared, protectedTerms);

        foreach (var entry in ProtectedGlossary.OrderByDescending(e => e.En.Length))
        {
            var source = normalizedTarget == "ru" ? entry.En : entry.Ru;
            var replacement = normalizedTarget == "ru" ? entry.Ru : entry.En;
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(replacement))
                continue;

            var regex = BuildTermRegex(source);
            prepared = regex.Replace(prepared, match =>
            {
                var token = $"{TokenPrefix}{protectedTerms.Count}{TokenSuffix}";
                protectedTerms.Add(new ProtectedTerm(token, replacement));
                return token;
            });
        }

        return new PreparedTranslation(prepared, protectedTerms);
    }

    private static string ProtectEmojiTokens(string text, List<ProtectedTerm> protectedTerms)
    {
        return EmojiTokenRegex.Replace(text, match =>
        {
            var token = $"{TokenPrefix}{protectedTerms.Count}{TokenSuffix}";
            protectedTerms.Add(new ProtectedTerm(token, match.Value));
            return token;
        });
    }

    private static string ProtectMarkupTokens(string text, List<ProtectedTerm> protectedTerms)
    {
        return RichMarkupTagRegex.Replace(text, match =>
        {
            var token = $"{TokenPrefix}{protectedTerms.Count}{TokenSuffix}";
            protectedTerms.Add(new ProtectedTerm(token, match.Value));
            return token;
        });
    }

    public static string ApplyPostProcessing(string originalText, string translated, string target, PreparedTranslation prepared)
    {
        if (!TryNormalizeLanguage(target, out var normalizedTarget))
            return translated;

        var result = translated.Trim();
        foreach (var term in prepared.ProtectedTerms)
        {
            result = new Regex(
                Regex.Escape(term.Token),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Replace(result, _ => term.Replacement);
        }

        if (normalizedTarget == "ru")
            result = ApplyRussianPostProcessing(originalText, result);

        return result;
    }

    private static string ApplyRussianPostProcessing(string originalText, string translated)
    {
        var result = translated;
        if (ContainsWord(originalText, "medbay"))
        {
            result = ReplaceIgnoreCase(result, "медбай", "медбей");
            result = ReplaceIgnoreCase(result, "медицинский залив", "медбей");
            result = ReplaceIgnoreCase(result, "медицинский отсек", "медбей");
        }

        if (ContainsWord(originalText, "marine") || ContainsWord(originalText, "marines"))
        {
            result = ReplaceIgnoreCase(result, "морской пехотинец", "морпех");
            result = ReplaceIgnoreCase(result, "морские пехотинцы", "морпехи");
        }

        if (ContainsWord(originalText, "dropship"))
            result = ReplaceIgnoreCase(result, "десантный корабль", "дропшип");

        return result;
    }

    private static Regex BuildTermRegex(string source)
    {
        var escaped = Regex.Escape(source.Trim()).Replace(@"\ ", @"\s+");
        return new Regex(
            $@"(?<![\p{{L}}\p{{N}}]){escaped}(?![\p{{L}}\p{{N}}])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string NormalizePhrase(string text, out string punctuation)
    {
        text = text.Trim();
        punctuation = string.Empty;

        var match = TrailingPunctuationRegex.Match(text);
        if (match.Success)
        {
            punctuation = match.Groups["punct"].Value;
            text = text[..match.Index];
        }

        text = text.Trim().Trim('"', '\'', '`', '“', '”', '‘', '’');
        text = text.Replace('’', '\'').Replace('`', '\'');
        return WhitespaceRegex.Replace(text.ToLowerInvariant(), " ").Trim();
    }

    private static string ApplyTrailingPunctuation(string text, string punctuation)
    {
        if (string.IsNullOrEmpty(punctuation))
            return text;

        return $"{text}{punctuation}";
    }

    private static bool ContainsWord(string text, string word)
    {
        return BuildTermRegex(word).IsMatch(text);
    }

    private static string ReplaceIgnoreCase(string text, string oldValue, string newValue)
    {
        return new Regex(
            Regex.Escape(oldValue),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Replace(text, _ => newValue);
    }

    private static bool TryNormalizeLanguage(string target, out string normalizedTarget)
    {
        normalizedTarget = target.Trim().ToLowerInvariant() switch
        {
            "ru" or "ru-ru" => "ru",
            "en" or "en-us" or "en-gb" => "en",
            _ => string.Empty,
        };

        return normalizedTarget.Length != 0;
    }

    private readonly record struct GlossaryPair(string En, string Ru);

    public readonly record struct PreparedTranslation(string Text, IReadOnlyList<ProtectedTerm> ProtectedTerms);

    public readonly record struct ProtectedTerm(string Token, string Replacement);
}
