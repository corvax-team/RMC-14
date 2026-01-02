
namespace Content.Shared._Forge.Sponsors;

public sealed class SponsorData
{
    public static readonly Dictionary<string, SponsorLevel> RolesMap = new()
    {
        { "1145599037878325249", SponsorLevel.Level1 }, // Бустер
        { "1224015397679005787", SponsorLevel.Level2 }, // Морпех
        { "1179165691111288963", SponsorLevel.Level3 }, // Преторианец
        { "1224015459792453713", SponsorLevel.Level4 }, // Санитар
        { "1179166010650144768", SponsorLevel.Level5 }, // Руни
        { "1224015729838395532", SponsorLevel.Level6 }, // Специалист
        { "1179166168817356831", SponsorLevel.Level7 }, // Королева улья
        { "1428440206666633266", SponsorLevel.Level8 }, // Разум улья
    };

    public static readonly Dictionary<SponsorLevel, string> SponsorColor = new()
    {
        { SponsorLevel.Level1, "#ff6ee7ff" },
        { SponsorLevel.Level2, "#9bbe2c" },
        { SponsorLevel.Level3, "#206694" },
        { SponsorLevel.Level4, "#1b4351" },
        { SponsorLevel.Level5, "#b83232" },
        { SponsorLevel.Level6, "#11ecbc" },
        { SponsorLevel.Level7, "#71368a" },
        { SponsorLevel.Level8, "#ad1457" }
    };

    public static readonly Dictionary<SponsorLevel, string> SponsorGhost = new()
    {
        { SponsorLevel.Level3, "SponsorGhostPretor" },
        { SponsorLevel.Level5, "SponsorGhostRuni" },
        { SponsorLevel.Level7, "SponsorGhostQueen" },
        { SponsorLevel.Level6, "MobObserver" }
    };

    public static SponsorLevel ParseRoles(List<string> roles)
    {
        var highestRole = SponsorLevel.None;
        foreach (var role in roles)
        {
            if (RolesMap.ContainsKey(role))
                if ((byte) RolesMap[role] > (byte) highestRole)
                    highestRole = RolesMap[role];
        }

        return highestRole;
    }
}

public enum SponsorLevel : byte
{
    None = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7,
    Level8 = 8
}
