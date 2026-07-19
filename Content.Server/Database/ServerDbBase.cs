using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Server._CCM.Database;
using Content.Server._RMC14.LinkAccount;
using Content.Shared._RMC14.Marines.Roles.Ranks;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.IP;
using Content.Shared._CCM.Achievements;
using Content.Shared._CCM.Sponsorship;
using Content.Shared._CCM.Stats;
using Content.Shared._RMC14.NamedItems;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Database
{
    public abstract class ServerDbBase
    {
        private readonly ISawmill _opsLog;

        public event Action<DatabaseNotification>? OnNotificationReceived;

        /// <param name="opsLog">Sawmill to trace log database operations to.</param>
        public ServerDbBase(ISawmill opsLog)
        {
            _opsLog = opsLog;
        }

        #region Preferences
        public async Task<PlayerPreferences?> GetPlayerPreferencesAsync(
            NetUserId userId,
            CancellationToken cancel = default)
        {
            await using var db = await GetDb(cancel);

            var prefs = await db.DbContext
                .Preference
                .Include(p => p.Profiles).ThenInclude(h => h.Jobs)
                .Include(p => p.Profiles).ThenInclude(h => h.Antags)
                .Include(p => p.Profiles).ThenInclude(h => h.Traits)
                .Include(p => p.Profiles)
                    .ThenInclude(h => h.Loadouts)
                    .ThenInclude(l => l.Groups)
                    .ThenInclude(group => group.Loadouts)
                .Include(p => p.Profiles).ThenInclude(p => p.NamedItems)
                .Include(p => p.Profiles).ThenInclude(h => h.Ranks)
                .Include(p => p.Profiles).ThenInclude(p => p.SquadPreference)
                .AsSplitQuery()
                .SingleOrDefaultAsync(p => p.UserId == userId.UserId, cancel);

            if (prefs is null)
                return null;

            var maxSlot = prefs.Profiles.Max(p => p.Slot) + 1;
            var profiles = new Dictionary<int, ICharacterProfile>(maxSlot);
            foreach (var profile in prefs.Profiles)
            {
                var convertedProfile = ConvertProfiles(profile);

                // Validate species - only allow: Human, Avali, Arachnid, Moth, Felinid, Dwarf
                if (convertedProfile is HumanoidCharacterProfile humanoidProfile)
                {
                    var allowedSpecies = new[] { "Human", "Avali", "Arachnid", "Moth", "Felinid", "Dwarf", "Yautja" };
                    if (!allowedSpecies.Contains(humanoidProfile.Species.Id))
                    {
                        humanoidProfile.Species = "Human";
                        profile.Species = "Human";
                        db.DbContext.Update(profile);
                    }
                }

                profiles[profile.Slot] = convertedProfile;
            }

            var constructionFavorites = new List<ProtoId<ConstructionPrototype>>(prefs.ConstructionFavorites.Count);
            foreach (var favorite in prefs.ConstructionFavorites)
                constructionFavorites.Add(new ProtoId<ConstructionPrototype>(favorite));

            await db.DbContext.SaveChangesAsync(cancel);

            return new PlayerPreferences(profiles, prefs.SelectedCharacterSlot, Color.FromHex(prefs.AdminOOCColor), constructionFavorites);
        }

        public async Task SaveSelectedCharacterIndexAsync(NetUserId userId, int index)
        {
            await using var db = await GetDb();

            await SetSelectedCharacterSlotAsync(userId, index, db.DbContext);

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SaveCharacterSlotAsync(NetUserId userId, ICharacterProfile? profile, int slot)
        {
            // CCM rework lobby - start
            try
            {
                await SaveCharacterSlotInternal(userId, profile, slot);
            }
            catch (DbUpdateConcurrencyException)
            {
                await SaveCharacterSlotInternal(userId, profile, slot);
            }
            // CCM rework lobby - end
        }

        private async Task SaveCharacterSlotInternal(NetUserId userId, ICharacterProfile? profile, int slot)
        {
            // CCM rework lobby - start
            await using var db = await GetDb();

            if (profile is null)
            {
                await DeleteCharacterSlot(db.DbContext, userId, slot);
                await db.DbContext.SaveChangesAsync();
                return;
            }

            if (profile is not HumanoidCharacterProfile humanoid)
            {
                // TODO: Handle other ICharacterProfile implementations properly
                throw new NotImplementedException();
            }

            var oldProfile = db.DbContext.Profile
                .Include(p => p.Preference)
                .Where(p => p.Preference.UserId == userId.UserId)
                .Include(p => p.Jobs)
                .Include(p => p.Antags)
                .Include(p => p.Traits)
                .Include(p => p.Loadouts)
                    .ThenInclude(l => l.Groups)
                    .ThenInclude(group => group.Loadouts)
                .Include(p => p.Ranks)
                .Include(p => p.NamedItems)
                .Include(p => p.SquadPreference)
                .AsSplitQuery()
                .SingleOrDefault(h => h.Slot == slot);

            var newProfile = ConvertProfiles(humanoid, slot, oldProfile);
            if (oldProfile == null)
            {
                var prefs = await db.DbContext
                    .Preference
                    .Include(p => p.Profiles)
                    .ThenInclude(p => p.NamedItems)
                    .Include(p => p.Profiles)
                    .ThenInclude(p => p.SquadPreference)
                    .SingleAsync(p => p.UserId == userId.UserId);

                prefs.Profiles.Add(newProfile);
            }

            await db.DbContext.SaveChangesAsync();
            // CCM rework lobby - end
        }

        private static async Task DeleteCharacterSlot(ServerDbContext db, NetUserId userId, int slot)
        {
            var profile = await db.Profile.Include(p => p.Preference)
                .Where(p => p.Preference.UserId == userId.UserId && p.Slot == slot)
                .SingleOrDefaultAsync();

            if (profile == null)
            {
                return;
            }

            await db.ProfileJobPriorityWeights
                .Where(w => w.PlayerUserId == userId.UserId && w.Slot == slot)
                .ExecuteDeleteAsync();

            db.Profile.Remove(profile);
        }

        public async Task<PlayerPreferences> InitPrefsAsync(NetUserId userId, ICharacterProfile defaultProfile)
        {
            await using var db = await GetDb();

            var profile = ConvertProfiles((HumanoidCharacterProfile) defaultProfile, 0);
            var prefs = new Preference
            {
                UserId = userId.UserId,
                SelectedCharacterSlot = 0,
                AdminOOCColor = Color.Red.ToHex(),
                ConstructionFavorites = [],
            };

            prefs.Profiles.Add(profile);

            db.DbContext.Preference.Add(prefs);

            await db.DbContext.SaveChangesAsync();

            return new PlayerPreferences(new[] { new KeyValuePair<int, ICharacterProfile>(0, defaultProfile) }, 0, Color.FromHex(prefs.AdminOOCColor), []);
        }

        public async Task DeleteSlotAndSetSelectedIndex(NetUserId userId, int deleteSlot, int newSlot)
        {
            await using var db = await GetDb();

            await DeleteCharacterSlot(db.DbContext, userId, deleteSlot);
            await SetSelectedCharacterSlotAsync(userId, newSlot, db.DbContext);

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SaveAdminOOCColorAsync(NetUserId userId, Color color)
        {
            await using var db = await GetDb();
            var prefs = await db.DbContext
                .Preference
                .Include(p => p.Profiles)
                .SingleAsync(p => p.UserId == userId.UserId);
            prefs.AdminOOCColor = color.ToHex();

            await db.DbContext.SaveChangesAsync();

        }

        public async Task SaveConstructionFavoritesAsync(NetUserId userId, List<ProtoId<ConstructionPrototype>> constructionFavorites)
        {
            await using var db = await GetDb();
            var prefs = await db.DbContext.Preference.SingleAsync(p => p.UserId == userId.UserId);

            var favorites = new List<string>(constructionFavorites.Count);
            foreach (var favorite in constructionFavorites)
                favorites.Add(favorite.Id);
            prefs.ConstructionFavorites = favorites;

            await db.DbContext.SaveChangesAsync();
        }

        private static async Task SetSelectedCharacterSlotAsync(NetUserId userId, int newSlot, ServerDbContext db)
        {
            var prefs = await db.Preference.SingleAsync(p => p.UserId == userId.UserId);
            prefs.SelectedCharacterSlot = newSlot;
        }

        private static HumanoidCharacterProfile ConvertProfiles(Profile profile)
        {
            var jobs = profile.Jobs.ToDictionary(j => new ProtoId<JobPrototype>(j.JobName), j => (JobPriority) j.Priority);
            var antags = profile.Antags.Select(a => new ProtoId<AntagPrototype>(a.AntagName));
            var traits = profile.Traits.Select(t => new ProtoId<TraitPrototype>(t.TraitName));
            var ranks = profile.Ranks.ToDictionary(
                r => new ProtoId<JobPrototype>(r.JobName),
                r => (ProtoId<RankPrototype>?) new ProtoId<RankPrototype>(r.RankName));

            var sex = Sex.Male;
            if (Enum.TryParse<Sex>(profile.Sex, true, out var sexVal))
                sex = sexVal;

            var spawnPriority = (SpawnPriorityPreference) profile.SpawnPriority;
            var squadPreference = profile.SquadPreference?.Squad;

            var armorPreference = ArmorPreference.Random;
            if (Enum.TryParse<ArmorPreference>(profile.ArmorPreference, true, out var armorVal))
                armorPreference = armorVal;

            var gender = sex == Sex.Male ? Gender.Male : Gender.Female;
            if (Enum.TryParse<Gender>(profile.Gender, true, out var genderVal))
                gender = genderVal;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var markingsRaw = profile.Markings?.Deserialize<List<string>>();

            List<Marking> markings = new();
            if (markingsRaw != null)
            {
                foreach (var marking in markingsRaw)
                {
                    var parsed = Marking.ParseFromDbString(marking);

                    if (parsed is null) continue;

                    markings.Add(parsed);
                }
            }

            var loadouts = new Dictionary<string, RoleLoadout>();

            foreach (var role in profile.Loadouts)
            {
                var loadout = new RoleLoadout(role.RoleName)
                {
                    EntityName = role.EntityName,
                };

                foreach (var group in role.Groups)
                {
                    var groupLoadouts = loadout.SelectedLoadouts.GetOrNew(group.GroupName);
                    foreach (var profLoadout in group.Loadouts)
                    {
                        groupLoadouts.Add(new Loadout()
                        {
                            Prototype = profLoadout.LoadoutName,
                        });
                    }
                }

                loadouts[role.RoleName] = loadout;
            }

            return new HumanoidCharacterProfile(
                profile.CharacterName,
                profile.FlavorText,
                profile.Species,
                profile.Age,
                sex,
                gender,
                new HumanoidCharacterAppearance
                (
                    profile.HairName,
                    Color.FromHex(profile.HairColor),
                    profile.FacialHairName,
                    Color.FromHex(profile.FacialHairColor),
                    Color.FromHex(profile.EyeColor),
                    Color.FromHex(profile.SkinColor),
                    markings
                ),
                spawnPriority,
                armorPreference,
                ranks,
                squadPreference,
                jobs,
                (PreferenceUnavailableMode)profile.PreferenceUnavailable,
                antags.ToHashSet(),
                traits.ToHashSet(),
                loadouts,
                new SharedRMCNamedItems
                {
                    PrimaryGunName = profile.NamedItems?.PrimaryGunName,
                    SidearmName = profile.NamedItems?.SidearmName,
                    HelmetName = profile.NamedItems?.HelmetName,
                    ArmorName = profile.NamedItems?.ArmorName,
                    SentryName = profile.NamedItems?.SentryName,
                },
                  profile.PlaytimePerks,
                  profile.XenoPrefix,
                  profile.XenoPostfix,
                  false,
                  false,
                  profile.OriginId,
                  profile.ReligionId,
                  profile.CorporateRelationId,
                  profile.BarkVoice,
                  profile.BarkPitch,
                  profile.BarkSpeed,
                  profile.Voice
              );
        }

        private static Profile ConvertProfiles(HumanoidCharacterProfile humanoid, int slot, Profile? profile = null)
        {
            profile ??= new Profile();
            var appearance = (HumanoidCharacterAppearance) humanoid.CharacterAppearance;
            List<string> markingStrings = new();
            foreach (var marking in appearance.Markings)
            {
                markingStrings.Add(marking.ToString());
            }
            var markings = JsonSerializer.SerializeToDocument(markingStrings);

            profile.CharacterName = humanoid.Name;
            profile.FlavorText = humanoid.FlavorText;
            profile.Species = humanoid.Species;
            profile.Age = humanoid.Age;
            profile.Sex = humanoid.Sex.ToString();
            profile.Gender = humanoid.Gender.ToString();
            profile.HairName = appearance.HairStyleId;
            profile.HairColor = appearance.HairColor.ToHex();
            profile.FacialHairName = appearance.FacialHairStyleId;
            profile.FacialHairColor = appearance.FacialHairColor.ToHex();
            profile.EyeColor = appearance.EyeColor.ToHex();
            profile.SkinColor = appearance.SkinColor.ToHex();
            profile.SpawnPriority = (int) humanoid.SpawnPriority;
            profile.ArmorPreference = humanoid.ArmorPreference.ToString();
            profile.SquadPreference = new RMCSquadPreference { Squad = humanoid.SquadPreference };
            profile.Markings = markings;
            profile.Slot = slot;
            profile.PreferenceUnavailable = (DbPreferenceUnavailableMode) humanoid.PreferenceUnavailable;

            profile.Jobs.Clear();
            profile.Jobs.AddRange(
                humanoid.JobPriorities
                    .Where(j => j.Value != JobPriority.Never)
                    .Select(j => new Job {JobName = j.Key, Priority = (DbJobPriority) j.Value})
            );

            profile.Antags.Clear();
            profile.Antags.AddRange(
                humanoid.AntagPreferences
                    .Select(a => new Antag {AntagName = a})
            );

            profile.Traits.Clear();
            profile.Traits.AddRange(
                humanoid.TraitPreferences
                        .Select(t => new Trait {TraitName = t})
            );

            profile.Ranks.Clear();
            profile.Ranks.AddRange(
                humanoid.RankPreferences
                    .Where(r => r.Value != null)
                    .Select(r => new Rank { JobName = r.Key, RankName = r.Value!.Value.Id })
            );

            profile.Loadouts.Clear();

            foreach (var (role, loadouts) in humanoid.Loadouts)
            {
                var dz = new ProfileRoleLoadout()
                {
                    RoleName = role,
                    EntityName = loadouts.EntityName ?? string.Empty,
                };

                foreach (var (group, groupLoadouts) in loadouts.SelectedLoadouts)
                {
                    var profileGroup = new ProfileLoadoutGroup()
                    {
                        GroupName = group,
                    };

                    foreach (var loadout in groupLoadouts)
                    {
                        profileGroup.Loadouts.Add(new ProfileLoadout()
                        {
                            LoadoutName = loadout.Prototype,
                        });
                    }

                    dz.Groups.Add(profileGroup);
                }

                profile.Loadouts.Add(dz);
            }

            profile.NamedItems = new RMCNamedItems
            {
                PrimaryGunName = humanoid.NamedItems.PrimaryGunName,
                SidearmName = humanoid.NamedItems.SidearmName,
                HelmetName = humanoid.NamedItems.HelmetName,
                ArmorName = humanoid.NamedItems.ArmorName,
                SentryName = humanoid.NamedItems.SentryName,
            };

            profile.PlaytimePerks = humanoid.PlaytimePerks;
            profile.XenoPrefix = humanoid.XenoPrefix;
            profile.XenoPostfix = humanoid.XenoPostfix;
            profile.OriginId = humanoid.OriginId;
            profile.ReligionId = humanoid.ReligionId;
            profile.CorporateRelationId = humanoid.CorporateRelationId;
            profile.BarkVoice = humanoid.BarkVoice;
            profile.BarkPitch = humanoid.BarkPitch;
            profile.BarkSpeed = humanoid.BarkSpeed;
            profile.Voice = humanoid.Voice;

            return profile;
        }

        public async Task<List<ProfileJobPriorityWeight>> GetJobPriorityWeights(Guid userId, CancellationToken cancel = default)
        {
            await using var db = await GetDb(cancel);

            return await db.DbContext.ProfileJobPriorityWeights
                .Where(w => w.PlayerUserId == userId)
                .ToListAsync(cancel);
        }

        public async Task UpsertJobPriorityWeights(Guid userId, int slot, IReadOnlyList<JobPriorityWeightUpdate> updates, CancellationToken cancel = default)
        {
            if (updates.Count == 0)
                return;

            await using var db = await GetDb(cancel);

            var jobIds = updates.Select(u => u.JobId).ToArray();
            var existing = await db.DbContext.ProfileJobPriorityWeights
                .Where(w => w.PlayerUserId == userId && w.Slot == slot && jobIds.Contains(w.JobName))
                .ToDictionaryAsync(w => w.JobName, cancel);

            foreach (var update in updates)
            {
                if (existing.TryGetValue(update.JobId, out var row))
                {
                    row.MissedRounds = update.MissedRounds;
                    row.LastAssignedRoundId = update.LastAssignedRoundId;
                }
                else
                {
                    db.DbContext.ProfileJobPriorityWeights.Add(new ProfileJobPriorityWeight
                    {
                        PlayerUserId = userId,
                        Slot = slot,
                        JobName = update.JobId,
                        MissedRounds = update.MissedRounds,
                        LastAssignedRoundId = update.LastAssignedRoundId
                    });
                }
            }

            await db.DbContext.SaveChangesAsync(cancel);
        }
        #endregion

        #region User Ids
        public async Task<NetUserId?> GetAssignedUserIdAsync(string name)
        {
            await using var db = await GetDb();

            var assigned = await db.DbContext.AssignedUserId.SingleOrDefaultAsync(p => p.UserName == name);
            return assigned?.UserId is { } g ? new NetUserId(g) : default(NetUserId?);
        }

        public async Task AssignUserIdAsync(string name, NetUserId netUserId)
        {
            await using var db = await GetDb();

            db.DbContext.AssignedUserId.Add(new AssignedUserId
            {
                UserId = netUserId.UserId,
                UserName = name
            });

            await db.DbContext.SaveChangesAsync();
        }
        #endregion

        #region Bans
        /*
         * BAN STUFF
         */
        /// <summary>
        ///     Looks up a ban by id.
        ///     This will return a pardoned ban as well.
        /// </summary>
        /// <param name="id">The ban id to look for.</param>
        /// <returns>The ban with the given id or null if none exist.</returns>
        public abstract Task<ServerBanDef?> GetServerBanAsync(int id);

        /// <summary>
        ///     Looks up an user's most recent received un-pardoned ban.
        ///     This will NOT return a pardoned ban.
        ///     One of <see cref="address"/> or <see cref="userId"/> need to not be null.
        /// </summary>
        /// <param name="address">The ip address of the user.</param>
        /// <param name="userId">The id of the user.</param>
        /// <param name="hwId">The legacy HWId of the user.</param>
        /// <param name="modernHWIds">The modern HWIDs of the user.</param>
        /// <returns>The user's latest received un-pardoned ban, or null if none exist.</returns>
        public abstract Task<ServerBanDef?> GetServerBanAsync(
            IPAddress? address,
            NetUserId? userId,
            ImmutableArray<byte>? hwId,
            ImmutableArray<ImmutableArray<byte>>? modernHWIds);

        /// <summary>
        ///     Looks up an user's ban history.
        ///     This will return pardoned bans as well.
        ///     One of <see cref="address"/> or <see cref="userId"/> need to not be null.
        /// </summary>
        /// <param name="address">The ip address of the user.</param>
        /// <param name="userId">The id of the user.</param>
        /// <param name="hwId">The legacy HWId of the user.</param>
        /// <param name="modernHWIds">The modern HWIDs of the user.</param>
        /// <param name="includeUnbanned">Include pardoned and expired bans.</param>
        /// <returns>The user's ban history.</returns>
        public abstract Task<List<ServerBanDef>> GetServerBansAsync(
            IPAddress? address,
            NetUserId? userId,
            ImmutableArray<byte>? hwId,
            ImmutableArray<ImmutableArray<byte>>? modernHWIds,
            bool includeUnbanned);

        public abstract Task AddServerBanAsync(ServerBanDef serverBan);
        public abstract Task AddServerUnbanAsync(ServerUnbanDef serverUnban);

        public async Task EditServerBan(int id, string reason, NoteSeverity severity, DateTimeOffset? expiration, Guid editedBy, DateTimeOffset editedAt)
        {
            await using var db = await GetDb();

            var ban = await db.DbContext.Ban.SingleOrDefaultAsync(b => b.Id == id);
            if (ban is null)
                return;
            ban.Severity = severity;
            ban.Reason = reason;
            ban.ExpirationTime = expiration?.UtcDateTime;
            ban.LastEditedById = editedBy;
            ban.LastEditedAt = editedAt.UtcDateTime;
            await db.DbContext.SaveChangesAsync();
        }

        protected static async Task<ServerBanExemptFlags?> GetBanExemptionCore(
            DbGuard db,
            NetUserId? userId,
            CancellationToken cancel = default)
        {
            if (userId == null)
                return null;

            var exemption = await db.DbContext.BanExemption
                .SingleOrDefaultAsync(e => e.UserId == userId.Value.UserId, cancellationToken: cancel);

            return exemption?.Flags;
        }

        public async Task UpdateBanExemption(NetUserId userId, ServerBanExemptFlags flags)
        {
            await using var db = await GetDb();

            if (flags == 0)
            {
                // Delete whatever is there.
                await db.DbContext.BanExemption.Where(u => u.UserId == userId.UserId).ExecuteDeleteAsync();
                return;
            }

            var exemption = await db.DbContext.BanExemption.SingleOrDefaultAsync(u => u.UserId == userId.UserId);
            if (exemption == null)
            {
                exemption = new ServerBanExemption
                {
                    UserId = userId
                };

                db.DbContext.BanExemption.Add(exemption);
            }

            exemption.Flags = flags;
            await db.DbContext.SaveChangesAsync();
        }

        public async Task<ServerBanExemptFlags> GetBanExemption(NetUserId userId, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var flags = await GetBanExemptionCore(db, userId, cancel);
            return flags ?? ServerBanExemptFlags.None;
        }

        #endregion

        #region Role Bans
        /*
         * ROLE BANS
         */
        /// <summary>
        ///     Looks up a role ban by id.
        ///     This will return a pardoned role ban as well.
        /// </summary>
        /// <param name="id">The role ban id to look for.</param>
        /// <returns>The role ban with the given id or null if none exist.</returns>
        public abstract Task<ServerRoleBanDef?> GetServerRoleBanAsync(int id);

        /// <summary>
        ///     Looks up an user's role ban history.
        ///     This will return pardoned role bans based on the <see cref="includeUnbanned"/> bool.
        ///     Requires one of <see cref="address"/>, <see cref="userId"/>, or <see cref="hwId"/> to not be null.
        /// </summary>
        /// <param name="address">The IP address of the user.</param>
        /// <param name="userId">The NetUserId of the user.</param>
        /// <param name="hwId">The Hardware Id of the user.</param>
        /// <param name="modernHWIds">The modern HWIDs of the user.</param>
        /// <param name="includeUnbanned">Whether expired and pardoned bans are included.</param>
        /// <returns>The user's role ban history.</returns>
        public abstract Task<List<ServerRoleBanDef>> GetServerRoleBansAsync(IPAddress? address,
            NetUserId? userId,
            ImmutableArray<byte>? hwId,
            ImmutableArray<ImmutableArray<byte>>? modernHWIds,
            bool includeUnbanned);

        public abstract Task<ServerRoleBanDef> AddServerRoleBanAsync(ServerRoleBanDef serverRoleBan);
        public abstract Task AddServerRoleUnbanAsync(ServerRoleUnbanDef serverRoleUnban);

        public async Task EditServerRoleBan(int id, string reason, NoteSeverity severity, DateTimeOffset? expiration, Guid editedBy, DateTimeOffset editedAt)
        {
            await using var db = await GetDb();
            var roleBanDetails = await db.DbContext.RoleBan
                .Where(b => b.Id == id)
                .Select(b => new { b.BanTime, b.PlayerUserId })
                .SingleOrDefaultAsync();

            if (roleBanDetails == default)
                return;

            await db.DbContext.RoleBan
                .Where(b => b.BanTime == roleBanDetails.BanTime && b.PlayerUserId == roleBanDetails.PlayerUserId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Severity, severity)
                    .SetProperty(b => b.Reason, reason)
                    .SetProperty(b => b.ExpirationTime, expiration.HasValue ? expiration.Value.UtcDateTime : (DateTime?)null)
                    .SetProperty(b => b.LastEditedById, editedBy)
                    .SetProperty(b => b.LastEditedAt, editedAt.UtcDateTime)
                );
        }
        #endregion

        #region Playtime
        public async Task<List<PlayTime>> GetPlayTimes(Guid player, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            return await db.DbContext.PlayTime
                .Where(p => p.PlayerId == player)
                .ToListAsync(cancel);
        }

        public async Task UpdatePlayTimes(IReadOnlyCollection<PlayTimeUpdate> updates)
        {
            await using var db = await GetDb();

            // Ideally I would just be able to send a bunch of UPSERT commands, but EFCore is a pile of garbage.
            // So... In the interest of not making this take forever at high update counts...
            // Bulk-load play time objects for all players involved.
            // This allows us to semi-efficiently load all entities we need in a single DB query.
            // Then we can update & insert without further round-trips to the DB.

            var players = updates.Select(u => u.User.UserId).Distinct().ToArray();
            var dbTimes = (await db.DbContext.PlayTime
                    .Where(p => players.Contains(p.PlayerId))
                    .ToArrayAsync())
                .GroupBy(p => p.PlayerId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(p => p.Tracker, p => p));

            foreach (var (user, tracker, time) in updates)
            {
                if (dbTimes.TryGetValue(user.UserId, out var userTimes)
                    && userTimes.TryGetValue(tracker, out var ent))
                {
                    // Already have a tracker in the database, update it.
                    ent.TimeSpent = time;
                    continue;
                }

                // No tracker, make a new one.
                var playTime = new PlayTime
                {
                    Tracker = tracker,
                    PlayerId = user.UserId,
                    TimeSpent = time
                };

                db.DbContext.PlayTime.Add(playTime);
            }

            await db.DbContext.SaveChangesAsync();
        }

        #endregion

        #region Player Records
        /*
         * PLAYER RECORDS
         */
        public async Task UpdatePlayerRecord(
            NetUserId userId,
            string userName,
            IPAddress address,
            ImmutableTypedHwid? hwId)
        {
            await using var db = await GetDb();

            var record = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == userId.UserId);
            if (record == null)
            {
                db.DbContext.Player.Add(record = new Player
                {
                    FirstSeenTime = DateTime.UtcNow,
                    UserId = userId.UserId,
                });
            }

            record.LastSeenTime = DateTime.UtcNow;
            record.LastSeenAddress = address;
            record.LastSeenUserName = userName;
            record.LastSeenHWId = hwId;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<PlayerRecord?> GetPlayerRecordByUserName(string userName, CancellationToken cancel)
        {
            await using var db = await GetDb();

            // Sort by descending last seen time.
            // So if, due to account renames, we have two people with the same username in the DB,
            // the most recent one is picked.
            var record = await db.DbContext.Player
                .OrderByDescending(p => p.LastSeenTime)
                .FirstOrDefaultAsync(p => p.LastSeenUserName == userName, cancel);

            return record == null ? null : MakePlayerRecord(record);
        }

        public async Task<PlayerRecord?> GetPlayerRecordByUserId(NetUserId userId, CancellationToken cancel)
        {
            await using var db = await GetDb();

            var record = await db.DbContext.Player
                .SingleOrDefaultAsync(p => p.UserId == userId.UserId, cancel);

            return record == null ? null : MakePlayerRecord(record);
        }

        protected async Task<bool> PlayerRecordExists(DbGuard db, NetUserId userId)
        {
            return await db.DbContext.Player.AnyAsync(p => p.UserId == userId);
        }

        [return: NotNullIfNotNull(nameof(player))]
        protected PlayerRecord? MakePlayerRecord(Player? player)
        {
            if (player == null)
                return null;

            return new PlayerRecord(
                new NetUserId(player.UserId),
                new DateTimeOffset(NormalizeDatabaseTime(player.FirstSeenTime)),
                player.LastSeenUserName,
                new DateTimeOffset(NormalizeDatabaseTime(player.LastSeenTime)),
                player.LastSeenAddress,
                player.LastSeenHWId);
        }

        #endregion

        #region Connection Logs
        /*
         * CONNECTION LOG
         */
        public abstract Task<int> AddConnectionLogAsync(NetUserId userId,
            string userName,
            IPAddress address,
            ImmutableTypedHwid? hwId,
            float trust,
            ConnectionDenyReason? denied,
            int serverId);

        public async Task AddServerBanHitsAsync(int connection, IEnumerable<ServerBanDef> bans)
        {
            await using var db = await GetDb();

            foreach (var ban in bans)
            {
                db.DbContext.ServerBanHit.Add(new ServerBanHit
                {
                    ConnectionId = connection, BanId = ban.Id!.Value
                });
            }

            await db.DbContext.SaveChangesAsync();
        }

        #endregion

        #region Admin Ranks
        /*
         * ADMIN RANKS
         */
        public async Task<Admin?> GetAdminDataForAsync(NetUserId userId, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            return await db.DbContext.Admin
                .Include(p => p.Flags)
                .Include(p => p.AdminRank)
                .ThenInclude(p => p!.Flags)
                .AsSplitQuery() // tests fail because of a random warning if you dont have this!
                .SingleOrDefaultAsync(p => p.UserId == userId.UserId, cancel);
        }

        public abstract Task<((Admin, string? lastUserName)[] admins, AdminRank[])>
            GetAllAdminAndRanksAsync(CancellationToken cancel);

        public async Task<AdminRank?> GetAdminRankDataForAsync(int id, CancellationToken cancel = default)
        {
            await using var db = await GetDb(cancel);

            return await db.DbContext.AdminRank
                .Include(r => r.Flags)
                .SingleOrDefaultAsync(r => r.Id == id, cancel);
        }

        public async Task RemoveAdminAsync(NetUserId userId, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var admin = await db.DbContext.Admin.SingleAsync(a => a.UserId == userId.UserId, cancel);
            db.DbContext.Admin.Remove(admin);

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task AddAdminAsync(Admin admin, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            db.DbContext.Admin.Add(admin);

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task UpdateAdminAsync(Admin admin, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var existing = await db.DbContext.Admin.Include(a => a.Flags).SingleAsync(a => a.UserId == admin.UserId, cancel);
            existing.Flags = admin.Flags;
            existing.Title = admin.Title;
            existing.AdminRankId = admin.AdminRankId;
            existing.Deadminned = admin.Deadminned;
            existing.Suspended = admin.Suspended;

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task UpdateAdminDeadminnedAsync(NetUserId userId, bool deadminned, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var adminRecord = db.DbContext.Admin.Where(a => a.UserId == userId);
            await adminRecord.ExecuteUpdateAsync(
                set => set.SetProperty(p => p.Deadminned, deadminned),
                cancellationToken: cancel);

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task RemoveAdminRankAsync(int rankId, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var admin = await db.DbContext.AdminRank.SingleAsync(a => a.Id == rankId, cancel);
            db.DbContext.AdminRank.Remove(admin);

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task AddAdminRankAsync(AdminRank rank, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            db.DbContext.AdminRank.Add(rank);

            await db.DbContext.SaveChangesAsync(cancel);
        }

        public async Task<int> AddNewRound(Server server, params Guid[] playerIds)
        {
            await using var db = await GetDb();

            var players = await db.DbContext.Player
                .Where(player => playerIds.Contains(player.UserId))
                .ToListAsync();

            var round = new Round
            {
                StartDate = DateTime.UtcNow,
                Players = players,
                ServerId = server.Id
            };

            db.DbContext.Round.Add(round);

            await db.DbContext.SaveChangesAsync();

            return round.Id;
        }

        public async Task<Round> GetRound(int id)
        {
            await using var db = await GetDb();

            var round = await db.DbContext.Round
                .Include(round => round.Players)
                .SingleAsync(round => round.Id == id);

            return round;
        }

        public async Task AddRoundPlayers(int id, Guid[] playerIds)
        {
            await using var db = await GetDb();

            // ReSharper disable once SuggestVarOrType_Elsewhere
            Dictionary<Guid, int> players = await db.DbContext.Player
                .Where(player => playerIds.Contains(player.UserId))
                .ToDictionaryAsync(player => player.UserId, player => player.Id);

            foreach (var player in playerIds)
            {
                await db.DbContext.Database.ExecuteSqlAsync($"""
INSERT INTO player_round (players_id, rounds_id) VALUES ({players[player]}, {id}) ON CONFLICT DO NOTHING
""");
            }

            await db.DbContext.SaveChangesAsync();
        }

        [return: NotNullIfNotNull(nameof(round))]
        protected RoundRecord? MakeRoundRecord(Round? round)
        {
            if (round == null)
                return null;

            return new RoundRecord(
                round.Id,
                NormalizeDatabaseTime(round.StartDate),
                MakeServerRecord(round.Server));
        }

        public async Task UpdateAdminRankAsync(AdminRank rank, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);

            var existing = await db.DbContext.AdminRank
                .Include(r => r.Flags)
                .SingleAsync(a => a.Id == rank.Id, cancel);

            existing.Flags = rank.Flags;
            existing.Name = rank.Name;

            await db.DbContext.SaveChangesAsync(cancel);
        }
        #endregion

        #region Admin Logs

        public async Task<(Server, bool existed)> AddOrGetServer(string serverName)
        {
            await using var db = await GetDb();
            var server = await db.DbContext.Server
                .Where(server => server.Name.Equals(serverName))
                .SingleOrDefaultAsync();

            if (server != default)
                return (server, true);

            server = new Server
            {
                Name = serverName
            };

            db.DbContext.Server.Add(server);

            await db.DbContext.SaveChangesAsync();

            return (server, false);
        }

        [return: NotNullIfNotNull(nameof(server))]
        protected ServerRecord? MakeServerRecord(Server? server)
        {
            if (server == null)
                return null;

            return new ServerRecord(server.Id, server.Name);
        }

        public async Task AddAdminLogs(List<AdminLog> logs)
        {
            const int maxRetryAttempts = 5;
            var initialRetryDelay = TimeSpan.FromSeconds(5);

            DebugTools.Assert(logs.All(x => x.RoundId > 0), "Adding logs with invalid round ids.");

            var attempt = 0;
            var retryDelay = initialRetryDelay;

            while (attempt < maxRetryAttempts)
            {
                try
                {
                    await using var db = await GetDb();
                    db.DbContext.AdminLog.AddRange(logs);
                    await db.DbContext.SaveChangesAsync();
                    _opsLog.Debug($"Successfully saved {logs.Count} admin logs.");
                    break;
                }
                catch (Exception ex)
                {
                    attempt += 1;
                    _opsLog.Error($"Attempt {attempt} failed to save logs: {ex}");

                    if (attempt >= maxRetryAttempts)
                    {
                        _opsLog.Error($"Max retry attempts reached. Failed to save {logs.Count} admin logs.");
                        return;
                    }

                    _opsLog.Warning($"Retrying in {retryDelay.TotalSeconds} seconds...");
                    await Task.Delay(retryDelay);

                    retryDelay *= 2;
                }
            }
        }

        protected abstract IQueryable<AdminLog> StartAdminLogsQuery(ServerDbContext db, LogFilter? filter = null);

        private IQueryable<AdminLog> GetAdminLogsQuery(ServerDbContext db, LogFilter? filter = null)
        {
            // Save me from SQLite
            var query = StartAdminLogsQuery(db, filter);

            if (filter == null)
            {
                return query.OrderBy(log => log.Date);
            }

            if (filter.Round != null)
            {
                query = query.Where(log => log.RoundId == filter.Round);
            }

            if (filter.Types != null)
            {
                query = query.Where(log => filter.Types.Contains(log.Type));
            }

            if (filter.Impacts != null)
            {
                query = query.Where(log => filter.Impacts.Contains(log.Impact));
            }

            if (filter.Before != null)
            {
                query = query.Where(log => log.Date < filter.Before);
            }

            if (filter.After != null)
            {
                query = query.Where(log => log.Date > filter.After);
            }

            if (filter.IncludePlayers)
            {
                if (filter.AnyPlayers != null)
                {
                    query = query.Where(log =>
                        log.Players.Any(p => filter.AnyPlayers.Contains(p.PlayerUserId)) ||
                        log.Players.Count == 0 && filter.IncludeNonPlayers);
                }

                if (filter.AllPlayers != null)
                {
                    query = query.Where(log =>
                        log.Players.All(p => filter.AllPlayers.Contains(p.PlayerUserId)) ||
                        log.Players.Count == 0 && filter.IncludeNonPlayers);
                }
            }
            else
            {
                query = query.Where(log => log.Players.Count == 0);
            }

            if (filter.LastLogId != null)
            {
                query = filter.DateOrder switch
                {
                    DateOrder.Ascending => query.Where(log => log.Id > filter.LastLogId),
                    DateOrder.Descending => query.Where(log => log.Id < filter.LastLogId),
                    _ => throw new ArgumentOutOfRangeException(nameof(filter),
                        $"Unknown {nameof(DateOrder)} value {filter.DateOrder}")
                };
            }

            query = filter.DateOrder switch
            {
                DateOrder.Ascending => query.OrderBy(log => log.Date),
                DateOrder.Descending => query.OrderByDescending(log => log.Date),
                _ => throw new ArgumentOutOfRangeException(nameof(filter),
                    $"Unknown {nameof(DateOrder)} value {filter.DateOrder}")
            };

            const int hardLogLimit = 500_000;
            if (filter.Limit != null)
            {
                query = query.Take(Math.Min(filter.Limit.Value, hardLogLimit));
            }
            else
            {
                query = query.Take(hardLogLimit);
            }

            return query;
        }

        public async IAsyncEnumerable<string> GetAdminLogMessages(LogFilter? filter = null)
        {
            await using var db = await GetDb();
            var query = GetAdminLogsQuery(db.DbContext, filter);

            await foreach (var log in query.Select(log => log.Message).AsAsyncEnumerable())
            {
                yield return log;
            }
        }

        public async IAsyncEnumerable<SharedAdminLog> GetAdminLogs(LogFilter? filter = null)
        {
            await using var db = await GetDb();
            var query = GetAdminLogsQuery(db.DbContext, filter);
            query = query.Include(log => log.Players);

            await foreach (var log in query.AsAsyncEnumerable())
            {
                var players = new Guid[log.Players.Count];
                for (var i = 0; i < log.Players.Count; i++)
                {
                    players[i] = log.Players[i].PlayerUserId;
                }

                yield return new SharedAdminLog(log.Id, log.Type, log.Impact, log.Date, log.Message, players);
            }
        }

        public async IAsyncEnumerable<JsonDocument> GetAdminLogsJson(LogFilter? filter = null)
        {
            await using var db = await GetDb();
            var query = GetAdminLogsQuery(db.DbContext, filter);

            await foreach (var json in query.Select(log => log.Json).AsAsyncEnumerable())
            {
                yield return json;
            }
        }

        public async Task<int> CountAdminLogs(int round)
        {
            await using var db = await GetDb();
            return await db.DbContext.AdminLog.CountAsync(log => log.RoundId == round);
        }

        #endregion

        #region Whitelist

        public async Task<bool> GetWhitelistStatusAsync(NetUserId player)
        {
            await using var db = await GetDb();

            return await db.DbContext.Whitelist.AnyAsync(w => w.UserId == player);
        }

        public async Task AddToWhitelistAsync(NetUserId player)
        {
            await using var db = await GetDb();

            db.DbContext.Whitelist.Add(new Whitelist { UserId = player });
            await db.DbContext.SaveChangesAsync();
        }

        public async Task RemoveFromWhitelistAsync(NetUserId player)
        {
            await using var db = await GetDb();
            var entry = await db.DbContext.Whitelist.SingleAsync(w => w.UserId == player);
            db.DbContext.Whitelist.Remove(entry);
            await db.DbContext.SaveChangesAsync();
        }

        public async Task<DateTimeOffset?> GetLastReadRules(NetUserId player)
        {
            await using var db = await GetDb();

            return NormalizeDatabaseTime(await db.DbContext.Player
                .Where(dbPlayer => dbPlayer.UserId == player)
                .Select(dbPlayer => dbPlayer.LastReadRules)
                .SingleOrDefaultAsync());
        }

        public async Task SetLastReadRules(NetUserId player, DateTimeOffset? date)
        {
            await using var db = await GetDb();

            var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == player).SingleOrDefaultAsync();
            if (dbPlayer == null)
            {
                return;
            }

            dbPlayer.LastReadRules = date?.UtcDateTime;
            await db.DbContext.SaveChangesAsync();
        }

        public async Task<bool> GetBlacklistStatusAsync(NetUserId player)
        {
            await using var db = await GetDb();

            return await db.DbContext.Blacklist.AnyAsync(w => w.UserId == player);
        }

        public async Task AddToBlacklistAsync(NetUserId player)
        {
            await using var db = await GetDb();

            db.DbContext.Blacklist.Add(new Blacklist() { UserId = player });
            await db.DbContext.SaveChangesAsync();
        }

        public async Task RemoveFromBlacklistAsync(NetUserId player)
        {
            await using var db = await GetDb();
            var entry = await db.DbContext.Blacklist.SingleAsync(w => w.UserId == player);
            db.DbContext.Blacklist.Remove(entry);
            await db.DbContext.SaveChangesAsync();
        }

        public async Task<bool> GetHiddenBanStatusAsync(
            NetUserId? player,
            IPAddress? address = null,
            ImmutableArray<byte>? hwId = null,
            ImmutableArray<ImmutableArray<byte>>? modernHWIds = null)
        {
            await using var db = await GetDb();
            await EnsureHiddenBanStorage(db.DbContext);

            if (player is not { } userId)
                return false;

            var connection = db.DbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM hidden_ban WHERE user_id = @userId LIMIT 1";
                AddCommandParameter(command, "@userId", userId.UserId.ToString());
                return await command.ExecuteScalarAsync() != null;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task AddHiddenBanAsync(NetUserId player, IPAddress? address = null, ImmutableTypedHwid? hwId = null)
        {
            await using var db = await GetDb();
            await EnsureHiddenBanStorage(db.DbContext);

            var connection = db.DbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO hidden_ban (user_id)
                    VALUES (@userId)
                    ON CONFLICT(user_id) DO NOTHING
                    """;

                AddCommandParameter(command, "@userId", player.UserId.ToString());

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task RemoveHiddenBanAsync(NetUserId player)
        {
            await using var db = await GetDb();
            await EnsureHiddenBanStorage(db.DbContext);

            var connection = db.DbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM hidden_ban WHERE user_id = @userId";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@userId";
                parameter.Value = player.UserId.ToString();
                command.Parameters.Add(parameter);

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        #endregion

        #region Uploaded Resources Logs

        public async Task AddUploadedResourceLogAsync(NetUserId user, DateTimeOffset date, string path, byte[] data)
        {
            await using var db = await GetDb();

            db.DbContext.UploadedResourceLog.Add(new UploadedResourceLog() { UserId = user, Date = date.UtcDateTime, Path = path, Data = data });
            await db.DbContext.SaveChangesAsync();
        }

        public async Task PurgeUploadedResourceLogAsync(int days)
        {
            await using var db = await GetDb();

            var date = DateTime.UtcNow.Subtract(TimeSpan.FromDays(days));

            await foreach (var log in db.DbContext.UploadedResourceLog
                               .Where(l => date > l.Date)
                               .AsAsyncEnumerable())
            {
                db.DbContext.UploadedResourceLog.Remove(log);
            }

            await db.DbContext.SaveChangesAsync();
        }

        #endregion

        #region Admin Notes

        public virtual async Task<int> AddAdminNote(AdminNote note)
        {
            await using var db = await GetDb();
            db.DbContext.AdminNotes.Add(note);
            await db.DbContext.SaveChangesAsync();
            return note.Id;
        }

        public virtual async Task<int> AddAdminWatchlist(AdminWatchlist watchlist)
        {
            await using var db = await GetDb();
            db.DbContext.AdminWatchlists.Add(watchlist);
            await db.DbContext.SaveChangesAsync();
            return watchlist.Id;
        }

        public virtual async Task<int> AddAdminMessage(AdminMessage message)
        {
            await using var db = await GetDb();
            db.DbContext.AdminMessages.Add(message);
            await db.DbContext.SaveChangesAsync();
            return message.Id;
        }

        public async Task<AdminNoteRecord?> GetAdminNote(int id)
        {
            await using var db = await GetDb();
            var entity = await db.DbContext.AdminNotes
                .Where(note => note.Id == id)
                .Include(note => note.Round)
                .ThenInclude(r => r!.Server)
                .Include(note => note.CreatedBy)
                .Include(note => note.LastEditedBy)
                .Include(note => note.DeletedBy)
                .Include(note => note.Player)
                .SingleOrDefaultAsync();

            return entity == null ? null : MakeAdminNoteRecord(entity);
        }

        private AdminNoteRecord MakeAdminNoteRecord(AdminNote entity)
        {
            return new AdminNoteRecord(
                entity.Id,
                MakeRoundRecord(entity.Round),
                MakePlayerRecord(entity.Player),
                entity.PlaytimeAtNote,
                entity.Message,
                entity.Severity,
                MakePlayerRecord(entity.CreatedBy),
                NormalizeDatabaseTime(entity.CreatedAt),
                MakePlayerRecord(entity.LastEditedBy),
                NormalizeDatabaseTime(entity.LastEditedAt),
                NormalizeDatabaseTime(entity.ExpirationTime),
                entity.Deleted,
                MakePlayerRecord(entity.DeletedBy),
                NormalizeDatabaseTime(entity.DeletedAt),
                entity.Secret);
        }

        public async Task<AdminWatchlistRecord?> GetAdminWatchlist(int id)
        {
            await using var db = await GetDb();
            var entity = await db.DbContext.AdminWatchlists
                .Where(note => note.Id == id)
                .Include(note => note.Round)
                .ThenInclude(r => r!.Server)
                .Include(note => note.CreatedBy)
                .Include(note => note.LastEditedBy)
                .Include(note => note.DeletedBy)
                .Include(note => note.Player)
                .SingleOrDefaultAsync();

            return entity == null ? null : MakeAdminWatchlistRecord(entity);
        }

        public async Task<AdminMessageRecord?> GetAdminMessage(int id)
        {
            await using var db = await GetDb();
            var entity = await db.DbContext.AdminMessages
                .Where(note => note.Id == id)
                .Include(note => note.Round)
                .ThenInclude(r => r!.Server)
                .Include(note => note.CreatedBy)
                .Include(note => note.LastEditedBy)
                .Include(note => note.DeletedBy)
                .Include(note => note.Player)
                .SingleOrDefaultAsync();

            return entity == null ? null : MakeAdminMessageRecord(entity);
        }

        private AdminMessageRecord MakeAdminMessageRecord(AdminMessage entity)
        {
            return new AdminMessageRecord(
                entity.Id,
                MakeRoundRecord(entity.Round),
                MakePlayerRecord(entity.Player),
                entity.PlaytimeAtNote,
                entity.Message,
                MakePlayerRecord(entity.CreatedBy),
                NormalizeDatabaseTime(entity.CreatedAt),
                MakePlayerRecord(entity.LastEditedBy),
                NormalizeDatabaseTime(entity.LastEditedAt),
                NormalizeDatabaseTime(entity.ExpirationTime),
                entity.Deleted,
                MakePlayerRecord(entity.DeletedBy),
                NormalizeDatabaseTime(entity.DeletedAt),
                entity.Seen,
                entity.Dismissed);
        }

        public async Task<ServerBanNoteRecord?> GetServerBanAsNoteAsync(int id)
        {
            await using var db = await GetDb();

            var ban = await db.DbContext.Ban
                .Include(ban => ban.Unban)
                .Include(ban => ban.Round)
                .ThenInclude(r => r!.Server)
                .Include(ban => ban.CreatedBy)
                .Include(ban => ban.LastEditedBy)
                .Include(ban => ban.Unban)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (ban is null)
                return null;

            var player = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == ban.PlayerUserId);
            return new ServerBanNoteRecord(
                ban.Id,
                MakeRoundRecord(ban.Round),
                MakePlayerRecord(player),
                ban.PlaytimeAtNote,
                ban.Reason,
                ban.Severity,
                MakePlayerRecord(ban.CreatedBy),
                ban.BanTime,
                MakePlayerRecord(ban.LastEditedBy),
                ban.LastEditedAt,
                ban.ExpirationTime,
                ban.Hidden,
                MakePlayerRecord(ban.Unban?.UnbanningAdmin == null
                    ? null
                    : await db.DbContext.Player.SingleOrDefaultAsync(p =>
                        p.UserId == ban.Unban.UnbanningAdmin.Value)),
                ban.Unban?.UnbanTime);
        }

        public async Task<ServerRoleBanNoteRecord?> GetServerRoleBanAsNoteAsync(int id)
        {
            await using var db = await GetDb();

            var ban = await db.DbContext.RoleBan
                .Include(ban => ban.Unban)
                .Include(ban => ban.Round)
                .ThenInclude(r => r!.Server)
                .Include(ban => ban.CreatedBy)
                .Include(ban => ban.LastEditedBy)
                .Include(ban => ban.Unban)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (ban is null)
                return null;

            var player = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == ban.PlayerUserId);
            var unbanningAdmin =
                ban.Unban is null
                ? null
                : await db.DbContext.Player.SingleOrDefaultAsync(b => b.UserId == ban.Unban.UnbanningAdmin);

            return new ServerRoleBanNoteRecord(
                ban.Id,
                MakeRoundRecord(ban.Round),
                MakePlayerRecord(player),
                ban.PlaytimeAtNote,
                ban.Reason,
                ban.Severity,
                MakePlayerRecord(ban.CreatedBy),
                ban.BanTime,
                MakePlayerRecord(ban.LastEditedBy),
                ban.LastEditedAt,
                ban.ExpirationTime,
                ban.Hidden,
                new [] { ban.RoleId.Replace(BanManager.JobPrefix, null) },
                MakePlayerRecord(unbanningAdmin),
                ban.Unban?.UnbanTime);
        }

        public async Task<List<IAdminRemarksRecord>> GetAllAdminRemarks(Guid player)
        {
            await using var db = await GetDb();
            List<IAdminRemarksRecord> notes = new();
            notes.AddRange(
                (await (from note in db.DbContext.AdminNotes
                        where note.PlayerUserId == player &&
                              !note.Deleted &&
                              (note.ExpirationTime == null || DateTime.UtcNow < note.ExpirationTime)
                        select note)
                    .Include(note => note.Round)
                    .ThenInclude(r => r!.Server)
                    .Include(note => note.CreatedBy)
                    .Include(note => note.LastEditedBy)
                    .Include(note => note.Player)
                    .ToListAsync()).Select(MakeAdminNoteRecord));
            notes.AddRange(await GetActiveWatchlistsImpl(db, player));
            notes.AddRange(await GetMessagesImpl(db, player));
            notes.AddRange(await GetServerBansAsNotesForUser(db, player));
            notes.AddRange(await GetGroupedServerRoleBansAsNotesForUser(db, player));
            return notes;
        }
        public async Task EditAdminNote(int id, string message, NoteSeverity severity, bool secret, Guid editedBy, DateTimeOffset editedAt, DateTimeOffset? expiryTime)
        {
            await using var db = await GetDb();

            var note = await db.DbContext.AdminNotes.Where(note => note.Id == id).SingleAsync();
            note.Message = message;
            note.Severity = severity;
            note.Secret = secret;
            note.LastEditedById = editedBy;
            note.LastEditedAt = editedAt.UtcDateTime;
            note.ExpirationTime = expiryTime?.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task EditAdminWatchlist(int id, string message, Guid editedBy, DateTimeOffset editedAt, DateTimeOffset? expiryTime)
        {
            await using var db = await GetDb();

            var note = await db.DbContext.AdminWatchlists.Where(note => note.Id == id).SingleAsync();
            note.Message = message;
            note.LastEditedById = editedBy;
            note.LastEditedAt = editedAt.UtcDateTime;
            note.ExpirationTime = expiryTime?.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task EditAdminMessage(int id, string message, Guid editedBy, DateTimeOffset editedAt, DateTimeOffset? expiryTime)
        {
            await using var db = await GetDb();

            var note = await db.DbContext.AdminMessages.Where(note => note.Id == id).SingleAsync();
            note.Message = message;
            note.LastEditedById = editedBy;
            note.LastEditedAt = editedAt.UtcDateTime;
            note.ExpirationTime = expiryTime?.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task DeleteAdminNote(int id, Guid deletedBy, DateTimeOffset deletedAt)
        {
            await using var db = await GetDb();

            var note = await db.DbContext.AdminNotes.Where(note => note.Id == id).SingleAsync();

            note.Deleted = true;
            note.DeletedById = deletedBy;
            note.DeletedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task DeleteAdminWatchlist(int id, Guid deletedBy, DateTimeOffset deletedAt)
        {
            await using var db = await GetDb();

            var watchlist = await db.DbContext.AdminWatchlists.Where(note => note.Id == id).SingleAsync();

            watchlist.Deleted = true;
            watchlist.DeletedById = deletedBy;
            watchlist.DeletedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task DeleteAdminMessage(int id, Guid deletedBy, DateTimeOffset deletedAt)
        {
            await using var db = await GetDb();

            var message = await db.DbContext.AdminMessages.Where(note => note.Id == id).SingleAsync();

            message.Deleted = true;
            message.DeletedById = deletedBy;
            message.DeletedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task HideServerBanFromNotes(int id, Guid deletedBy, DateTimeOffset deletedAt)
        {
            await using var db = await GetDb();

            var ban = await db.DbContext.Ban.Where(ban => ban.Id == id).SingleAsync();

            ban.Hidden = true;
            ban.LastEditedById = deletedBy;
            ban.LastEditedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task HideServerRoleBanFromNotes(int id, Guid deletedBy, DateTimeOffset deletedAt)
        {
            await using var db = await GetDb();

            var roleBan = await db.DbContext.RoleBan.Where(roleBan => roleBan.Id == id).SingleAsync();

            roleBan.Hidden = true;
            roleBan.LastEditedById = deletedBy;
            roleBan.LastEditedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<List<IAdminRemarksRecord>> GetVisibleAdminRemarks(Guid player)
        {
            await using var db = await GetDb();
            List<IAdminRemarksRecord> notesCol = new();
            notesCol.AddRange(
                (await (from note in db.DbContext.AdminNotes
                        where note.PlayerUserId == player &&
                              !note.Secret &&
                              !note.Deleted &&
                              (note.ExpirationTime == null || DateTime.UtcNow < note.ExpirationTime)
                        select note)
                    .Include(note => note.Round)
                    .ThenInclude(r => r!.Server)
                    .Include(note => note.CreatedBy)
                    .Include(note => note.Player)
                    .ToListAsync()).Select(MakeAdminNoteRecord));
            notesCol.AddRange(await GetMessagesImpl(db, player));
            notesCol.AddRange(await GetServerBansAsNotesForUser(db, player));
            notesCol.AddRange(await GetGroupedServerRoleBansAsNotesForUser(db, player));
            return notesCol;
        }

        public async Task<List<AdminWatchlistRecord>> GetActiveWatchlists(Guid player)
        {
            await using var db = await GetDb();
            return await GetActiveWatchlistsImpl(db, player);
        }

        protected async Task<List<AdminWatchlistRecord>> GetActiveWatchlistsImpl(DbGuard db, Guid player)
        {
            var entities = await (from watchlist in db.DbContext.AdminWatchlists
                          where watchlist.PlayerUserId == player &&
                                !watchlist.Deleted &&
                                (watchlist.ExpirationTime == null || DateTime.UtcNow < watchlist.ExpirationTime)
                          select watchlist)
                .Include(note => note.Round)
                .ThenInclude(r => r!.Server)
                .Include(note => note.CreatedBy)
                .Include(note => note.LastEditedBy)
                .Include(note => note.Player)
                .ToListAsync();

            return entities.Select(MakeAdminWatchlistRecord).ToList();
        }

        private AdminWatchlistRecord MakeAdminWatchlistRecord(AdminWatchlist entity)
        {
            return new AdminWatchlistRecord(entity.Id, MakeRoundRecord(entity.Round), MakePlayerRecord(entity.Player), entity.PlaytimeAtNote, entity.Message, MakePlayerRecord(entity.CreatedBy), NormalizeDatabaseTime(entity.CreatedAt), MakePlayerRecord(entity.LastEditedBy), NormalizeDatabaseTime(entity.LastEditedAt), NormalizeDatabaseTime(entity.ExpirationTime), entity.Deleted, MakePlayerRecord(entity.DeletedBy), NormalizeDatabaseTime(entity.DeletedAt));
        }

        public async Task<List<AdminMessageRecord>> GetMessages(Guid player)
        {
            await using var db = await GetDb();
            return await GetMessagesImpl(db, player);
        }

        protected async Task<List<AdminMessageRecord>> GetMessagesImpl(DbGuard db, Guid player)
        {
            var entities = await (from message in db.DbContext.AdminMessages
                        where message.PlayerUserId == player && !message.Deleted &&
                              (message.ExpirationTime == null || DateTime.UtcNow < message.ExpirationTime)
                        select message).Include(note => note.Round)
                    .ThenInclude(r => r!.Server)
                    .Include(note => note.CreatedBy)
                    .Include(note => note.LastEditedBy)
                    .Include(note => note.Player)
                    .ToListAsync();

            return entities.Select(MakeAdminMessageRecord).ToList();
        }

        public async Task MarkMessageAsSeen(int id, bool dismissedToo)
        {
            await using var db = await GetDb();
            var message = await db.DbContext.AdminMessages.SingleAsync(m => m.Id == id);
            message.Seen = true;
            if (dismissedToo)
                message.Dismissed = true;
            await db.DbContext.SaveChangesAsync();
        }

        // These two are here because they get converted into notes later
        protected async Task<List<ServerBanNoteRecord>> GetServerBansAsNotesForUser(DbGuard db, Guid user)
        {
            // You can't group queries, as player will not always exist. When it doesn't, the
            // whole query returns nothing
            var player = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == user);
            var bans = await db.DbContext.Ban
                .Where(ban => ban.PlayerUserId == user && !ban.Hidden)
                .Include(ban => ban.Unban)
                .Include(ban => ban.Round)
                .ThenInclude(r => r!.Server)
                .Include(ban => ban.CreatedBy)
                .Include(ban => ban.LastEditedBy)
                .Include(ban => ban.Unban)
                .ToArrayAsync();

            var banNotes = new List<ServerBanNoteRecord>();
            foreach (var ban in bans)
            {
                var banNote = new ServerBanNoteRecord(
                    ban.Id,
                    MakeRoundRecord(ban.Round),
                    MakePlayerRecord(player),
                    ban.PlaytimeAtNote,
                    ban.Reason,
                    ban.Severity,
                    MakePlayerRecord(ban.CreatedBy),
                    NormalizeDatabaseTime(ban.BanTime),
                    MakePlayerRecord(ban.LastEditedBy),
                    NormalizeDatabaseTime(ban.LastEditedAt),
                    NormalizeDatabaseTime(ban.ExpirationTime),
                    ban.Hidden,
                    MakePlayerRecord(ban.Unban?.UnbanningAdmin == null
                        ? null
                        : await db.DbContext.Player.SingleOrDefaultAsync(
                            p => p.UserId == ban.Unban.UnbanningAdmin.Value)),
                    NormalizeDatabaseTime(ban.Unban?.UnbanTime));

                banNotes.Add(banNote);
            }

            return banNotes;
        }

        protected async Task<List<ServerRoleBanNoteRecord>> GetGroupedServerRoleBansAsNotesForUser(DbGuard db, Guid user)
        {
            // Server side query
            var bansQuery = await db.DbContext.RoleBan
                .Where(ban => ban.PlayerUserId == user && !ban.Hidden)
                .Include(ban => ban.Unban)
                .Include(ban => ban.Round)
                .ThenInclude(r => r!.Server)
                .Include(ban => ban.CreatedBy)
                .Include(ban => ban.LastEditedBy)
                .Include(ban => ban.Unban)
                .ToArrayAsync();

            // Client side query, as EF can't do groups yet
            var bansEnumerable = bansQuery
                    .GroupBy(ban => new { ban.BanTime, CreatedBy = (Player?)ban.CreatedBy, ban.Reason, Unbanned = ban.Unban == null })
                    .Select(banGroup => banGroup)
                    .ToArray();

            List<ServerRoleBanNoteRecord> bans = new();
            var player = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == user);
            foreach (var banGroup in bansEnumerable)
            {
                var firstBan = banGroup.First();
                Player? unbanningAdmin = null;

                if (firstBan.Unban?.UnbanningAdmin is not null)
                    unbanningAdmin = await db.DbContext.Player.SingleOrDefaultAsync(p => p.UserId == firstBan.Unban.UnbanningAdmin.Value);

                bans.Add(new ServerRoleBanNoteRecord(
                    firstBan.Id,
                    MakeRoundRecord(firstBan.Round),
                    MakePlayerRecord(player),
                    firstBan.PlaytimeAtNote,
                    firstBan.Reason,
                    firstBan.Severity,
                    MakePlayerRecord(firstBan.CreatedBy),
                    NormalizeDatabaseTime(firstBan.BanTime),
                    MakePlayerRecord(firstBan.LastEditedBy),
                    NormalizeDatabaseTime(firstBan.LastEditedAt),
                    NormalizeDatabaseTime(firstBan.ExpirationTime),
                    firstBan.Hidden,
                    banGroup.Select(ban => ban.RoleId.Replace(BanManager.JobPrefix, null)).ToArray(),
                    MakePlayerRecord(unbanningAdmin),
                    NormalizeDatabaseTime(firstBan.Unban?.UnbanTime)));
            }

            return bans;
        }

        #endregion

        #region Job Whitelists

        public async Task<bool> AddJobWhitelist(Guid player, ProtoId<JobPrototype> job)
        {
            await using var db = await GetDb();
            var exists = await db.DbContext.RoleWhitelists
                .Where(w => w.PlayerUserId == player)
                .Where(w => w.RoleId == job.Id)
                .AnyAsync();

            if (exists)
                return false;

            var whitelist = new RoleWhitelist
            {
                PlayerUserId = player,
                RoleId = job
            };
            db.DbContext.RoleWhitelists.Add(whitelist);
            await db.DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetJobWhitelists(Guid player, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);
            return await db.DbContext.RoleWhitelists
                .Where(w => w.PlayerUserId == player)
                .Select(w => w.RoleId)
                .ToListAsync(cancellationToken: cancel);
        }

        public async Task<bool> IsJobWhitelisted(Guid player, ProtoId<JobPrototype> job, CancellationToken cancel = default)
        {
            await using var db = await GetDb(cancel);
            return await db.DbContext.RoleWhitelists
                .Where(w => w.PlayerUserId == player)
                .Where(w => w.RoleId == job.Id)
                .AnyAsync(cancel);
        }

        public async Task<bool> RemoveJobWhitelist(Guid player, ProtoId<JobPrototype> job)
        {
            await using var db = await GetDb();
            var entry = await db.DbContext.RoleWhitelists
                .Where(w => w.PlayerUserId == player)
                .Where(w => w.RoleId == job.Id)
                .SingleOrDefaultAsync();

            if (entry == null)
                return false;

            db.DbContext.RoleWhitelists.Remove(entry);
            await db.DbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        # region IPIntel

        public async Task<bool> UpsertIPIntelCache(DateTime time, IPAddress ip, float score)
        {
            while (true)
            {
                try
                {
                    await using var db = await GetDb();

                    var existing = await db.DbContext.IPIntelCache
                        .Where(w => ip.Equals(w.Address))
                        .SingleOrDefaultAsync();

                    if (existing == null)
                    {
                        var newCache = new IPIntelCache
                        {
                            Time = time,
                            Address = ip,
                            Score = score,
                        };
                        db.DbContext.IPIntelCache.Add(newCache);
                    }
                    else
                    {
                        existing.Time = time;
                        existing.Score = score;
                    }

                    await Task.Delay(5000);

                    await db.DbContext.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    _opsLog.Warning("IPIntel UPSERT failed with a db exception... retrying.");
                }
            }
        }

        public async Task<IPIntelCache?> GetIPIntelCache(IPAddress ip)
        {
            await using var db = await GetDb();

            return await db.DbContext.IPIntelCache
                .SingleOrDefaultAsync(w => ip.Equals(w.Address));
        }

        public async Task<bool> CleanIPIntelCache(TimeSpan range)
        {
            await using var db = await GetDb();

            // Calculating this here cause otherwise sqlite whines.
            var cutoffTime = DateTime.UtcNow.Subtract(range);

            await db.DbContext.IPIntelCache
                .Where(w => w.Time <= cutoffTime)
                .ExecuteDeleteAsync();

            await db.DbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region RMC14

        public async Task<Guid?> GetLinkingCode(Guid player)
        {
            await using var db = await GetDb();
            var linking = await db.DbContext.RMCLinkingCodes.FirstOrDefaultAsync(l => l.PlayerId == player);
            return linking?.Code;
        }

        public async Task SetLinkingCode(Guid player, Guid code)
        {
            await using var db = await GetDb();
            var linking = await db.DbContext.RMCLinkingCodes.FirstOrDefaultAsync(l => l.PlayerId == player);
            if (linking == null)
            {
                linking = new RMCLinkingCodes { PlayerId = player };
                db.DbContext.RMCLinkingCodes.Add(linking);
            }

            linking.Code = code;
            linking.CreationTime = DateTime.UtcNow;
            await db.DbContext.SaveChangesAsync();
        }

        public async Task<bool> HasLinkedAccount(Guid player, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);
            return await db.DbContext.RMCLinkedAccounts.AnyAsync(l => l.PlayerId == player, cancel);

        }

        public async Task<RMCPatron?> GetPatron(Guid player, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);
            var patron = await db.DbContext.RMCPatrons
                .Include(p => p.Tier)
                .Include(p => p.LobbyMessage)
                .Include(p => p.RoundEndMarineShoutout)
                .Include(p => p.RoundEndXenoShoutout)
                .FirstOrDefaultAsync(p => p.PlayerId == player, cancellationToken: cancel);
            return patron;
        }

        public async Task<List<RMCPatron>> GetAllPatrons()
        {
            await using var db = await GetDb();
            return await db.DbContext.RMCPatrons
                .Include(p => p.Player)
                .Include(p => p.Tier)
                .ToListAsync();
        }

        public async Task SetGhostColor(Guid player, System.Drawing.Color? color)
        {
            await using var db = await GetDb();
            var patron = await db.DbContext.RMCPatrons.FirstOrDefaultAsync(p => p.PlayerId == player);
            if (patron == null)
                return;

            patron.GhostColor = color?.ToArgb();
            await db.DbContext.SaveChangesAsync();
        }

        public async Task SetLobbyMessage(Guid player, string message)
        {
            await using var db = await GetDb();
            var msg = await db.DbContext.RMCPatronLobbyMessages
                .Include(l => l.Patron)
                .FirstOrDefaultAsync(p => p.PatronId == player);
            msg ??= db.DbContext.RMCPatronLobbyMessages
                .Add(new RMCPatronLobbyMessage
                {
                    PatronId = player,
                    Message = message,
                })
                .Entity;
            msg.Message = message;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SetMarineShoutout(Guid player, string name)
        {
            await using var db = await GetDb();
            var msg = await db.DbContext.RMCPatronRoundEndMarineShoutouts
                .Include(s => s.Patron)
                .FirstOrDefaultAsync(p => p.PatronId == player);
            msg ??= db.DbContext.RMCPatronRoundEndMarineShoutouts
                .Add(new RMCPatronRoundEndMarineShoutout()
                {
                    PatronId = player,
                    Name = name,
                })
                .Entity;
            msg.Name = name;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SetXenoShoutout(Guid player, string name)
        {
            await using var db = await GetDb();
            var msg = await db.DbContext.RMCPatronRoundEndXenoShoutouts
                .Include(s => s.Patron)
                .FirstOrDefaultAsync(p => p.PatronId == player);
            msg ??= db.DbContext.RMCPatronRoundEndXenoShoutouts
                .Add(new RMCPatronRoundEndXenoShoutout()
                {
                    PatronId = player,
                    Name = name,
                })
                .Entity;
            msg.Name = name;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<(string Message, string User)?> GetRandomLobbyMessage()
        {
            // TODO RMC14 the random row is evaluated outside the DB, if we have that many patrons I guess we have better problems!
            await using var db = await GetDb();
            var messages = await db.DbContext.RMCPatronLobbyMessages
                .Include(p => p.Patron)
                .ThenInclude(p => p.Player)
                .Where(p => p.Patron.Tier.LobbyMessage)
                .Where(p => !string.IsNullOrWhiteSpace(p.Message))
                .Select(p => new { p.Message, p.Patron.Player.LastSeenUserName })
                .ToListAsync();

            if (messages.Count == 0)
                return null;

            var random = messages[Random.Shared.Next(messages.Count)];
            return (random.Message, random.LastSeenUserName);
        }

        public async Task<(RoundEndShoutout? Marine, RoundEndShoutout? Xeno)> GetRandomShoutout()
        {
            // TODO RMC14 the random row is evaluated outside the DB, if we have that many patrons I guess we have better problems!
            await using var db = await GetDb();
            var marines = await db.DbContext.RMCPatronRoundEndMarineShoutouts
                .Include(p => p.Patron)
                .ThenInclude(p => p.Player)
                .Where(p => p.Patron.Tier.RoundEndShoutout)
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToListAsync();

            var xenos = await db.DbContext.RMCPatronRoundEndXenoShoutouts
                .Include(p => p.Patron)
                .ThenInclude(p => p.Player)
                .Where(p => p.Patron.Tier.RoundEndShoutout)
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToListAsync();

            var marine = marines.Count == 0 ? null : marines[Random.Shared.Next(marines.Count)];
            RoundEndShoutout? marineShoutout = marine == null
                ? null
                : new RoundEndShoutout(marine.Patron.Player.LastSeenUserName, marine.Name);

            var xeno = xenos.Count == 0 ? null : xenos[Random.Shared.Next(xenos.Count)];
            RoundEndShoutout? xenoShoutout = xeno == null
                ? null
                : new RoundEndShoutout(xeno.Patron.Player.LastSeenUserName, xeno.Name);

            return (marineShoutout, xenoShoutout);
        }

        public async Task<List<string>> GetExcludedRoleTimers(Guid player, CancellationToken cancel)
        {
            await using var db = await GetDb(cancel);
            return await db.DbContext.RMCRoleTimerExcludes
                .Where(r => r.PlayerId == player)
                .Select(r => r.Tracker)
                .ToListAsync(cancel);
        }

        public async Task<bool> ExcludeRoleTimer(Guid player, string tracker)
        {
            await using var db = await GetDb();
            var alreadyExcluded = await db.DbContext.RMCRoleTimerExcludes
                .AnyAsync(r => r.PlayerId == player && r.Tracker == tracker);
            if (alreadyExcluded)
                return false;

            db.DbContext.RMCRoleTimerExcludes.Add(new RMCRoleTimerExclude
            {
                PlayerId = player,
                Tracker = tracker,
            });
            await db.DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleTimerExclusion(Guid player, string tracker)
        {
            await using var db = await GetDb();
            var exclusion = await db.DbContext.RMCRoleTimerExcludes
                .FirstOrDefaultAsync(r => r.PlayerId == player && r.Tracker == tracker);
            if (exclusion == null)
                return false;

            db.DbContext.RMCRoleTimerExcludes.Remove(exclusion);
            await db.DbContext.SaveChangesAsync();
            return true;
        }

        public async Task AddCommendation(Guid giver,
            Guid receiver,
            string giverName,
            string receiverName,
            string name,
            string text,
            CommendationType type,
            int round)
        {
            await using var db = await GetDb();
            db.DbContext.RMCCommendations.Add(new RMCCommendation
            {
                GiverId = giver,
                ReceiverId = receiver,
                GiverName = giverName,
                ReceiverName = receiverName,
                Name = name,
                Text = text,
                Type = type,
                RoundId = round,
            });

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<List<RMCCommendation>> GetCommendationsReceived(Guid player, CommendationType? filterType = null, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            if (filterType.HasValue)
                query = query.Where(c => c.Type == filterType.Value);

            return await query
                .Where(c => c.ReceiverId == player)
                .ToListAsync();
        }

        public async Task<List<RMCCommendation>> GetCommendationsGiven(Guid player, CommendationType? filterType = null, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            if (filterType.HasValue)
                query = query.Where(c => c.Type == filterType.Value);

            return await query
                .Where(c => c.GiverId == player)
                .ToListAsync();
        }

        public async Task<List<RMCCommendation>> GetLastCommendations(int count, CommendationType? filterType = null, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            if (filterType.HasValue)
                query = query.Where(c => c.Type == filterType.Value);

            return await query
                .OrderByDescending(c => c.Id)
                .Take(count)
                .ToListAsync();
        }

        public async Task<RMCCommendation?> GetCommendationById(int commendationId, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            return await query
                .FirstOrDefaultAsync(c => c.Id == commendationId);
        }

        public async Task<List<RMCCommendation>> GetCommendationsByRound(int roundId, CommendationType? filterType = null, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            if (filterType.HasValue)
                query = query.Where(c => c.Type == filterType.Value);

            return await query
                .Where(c => c.RoundId == roundId)
                .ToListAsync();
        }

        public async Task<RMCCommendation?> DeleteCommendationById(int commendationId, Guid deletedBy, DateTimeOffset deletedAt, bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            var commendation = await query
                .FirstOrDefaultAsync(c => c.Id == commendationId);

            if (commendation == null)
                return null;

            commendation.Deleted = true;
            commendation.DeletedById = deletedBy;
            commendation.DeletedAt = deletedAt.UtcDateTime;

            await db.DbContext.SaveChangesAsync();
            return commendation;
        }

        public async Task<List<RMCCommendation>> DeleteCommendationsByRound(
            int roundId,
            CommendationType type,
            Guid deletedBy,
            DateTimeOffset deletedAt,
            Guid? giverId = null,
            Guid? receiverId = null,
            bool includePlayers = false)
        {
            await using var db = await GetDb();
            var query = db.DbContext.RMCCommendations
                .Where(c => !c.Deleted)
                .AsQueryable();

            if (includePlayers)
            {
                query = query
                    .Include(c => c.Giver)
                    .Include(c => c.Receiver);
            }

            query = query.Where(c => c.RoundId == roundId && c.Type == type);

            if (giverId.HasValue)
                query = query.Where(c => c.GiverId == giverId.Value);

            if (receiverId.HasValue)
                query = query.Where(c => c.ReceiverId == receiverId.Value);

            var commendations = await query.ToListAsync();

            if (commendations.Count == 0)
                return commendations;

            foreach (var commendation in commendations)
            {
                commendation.Deleted = true;
                commendation.DeletedById = deletedBy;
                commendation.DeletedAt = deletedAt.UtcDateTime;
            }

            await db.DbContext.SaveChangesAsync();
            return commendations;
        }

        public async Task IncreaseInfects(Guid player)
        {
            await using var db = await GetDb();
            var stats = await db.DbContext.RMCPlayerStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            stats ??= db.DbContext.RMCPlayerStats
                .Add(new RMCPlayerStats { PlayerId = player })
                .Entity;

            stats.ParasiteInfects++;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<CCMPlayerStatsSnapshot> GetCCMPlayerStats(Guid player)
        {
            await using var db = await GetDb();
            var stats = await db.DbContext.CCMPlayerStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            return ToCCMStatsSnapshot(stats);
        }

        public async Task<CCMPlayerAchievementStatsSnapshot> GetCCMPlayerAchievementStats(Guid player)
        {
            await using var db = await GetDb();
            var stats = await db.DbContext.CCMPlayerAchievementStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            return ToCCMAchievementStatsSnapshot(stats);
        }

        public async Task<CCMCustomizationSnapshot> GetCCMCustomization(Guid player)
        {
            await using var db = await GetDb();
            await EnsureCCMCustomizationCompatibility(db.DbContext);
            var customization = await db.DbContext.CCMPlayerCustomization
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            return ToCCMCustomizationSnapshot(customization);
        }

        public async Task AdjustCCMPlayerAchievementStats(
            Guid player,
            int friendlyFireDamageDelta = 0,
            int requisitionOrdersDelta = 0,
            int xenoEvolutionsDelta = 0,
            int officerWinsDelta = 0,
            int queenKillsDelta = 0,
            int queenWinsDelta = 0,
            int queenKillParticipationsDelta = 0)
        {
            await using var db = await GetDb();

            var stats = await db.DbContext.CCMPlayerAchievementStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            stats ??= db.DbContext.CCMPlayerAchievementStats
                .Add(new CCMPlayerAchievementStats { PlayerId = player })
                .Entity;

            stats.FriendlyFireDamage = Math.Max(0, stats.FriendlyFireDamage + friendlyFireDamageDelta);
            stats.RequisitionOrders = Math.Max(0, stats.RequisitionOrders + requisitionOrdersDelta);
            stats.XenoEvolutions = Math.Max(0, stats.XenoEvolutions + xenoEvolutionsDelta);
            stats.OfficerWins = Math.Max(0, stats.OfficerWins + officerWinsDelta);
            stats.QueenKills = Math.Max(0, stats.QueenKills + queenKillsDelta);
            stats.QueenWins = Math.Max(0, stats.QueenWins + queenWinsDelta);
            stats.QueenKillParticipations = Math.Max(0, stats.QueenKillParticipations + queenKillParticipationsDelta);

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SetCCMUnlockedAchievementIds(Guid player, string unlockedAchievementIds)
        {
            await using var db = await GetDb();

            var stats = await db.DbContext.CCMPlayerAchievementStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            stats ??= db.DbContext.CCMPlayerAchievementStats
                .Add(new CCMPlayerAchievementStats { PlayerId = player })
                .Entity;

            stats.UnlockedAchievementIds = unlockedAchievementIds;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task SaveCCMCustomization(Guid player, CCMCustomizationSnapshot snapshot)
        {
            await using var db = await GetDb();
            await EnsureCCMCustomizationCompatibility(db.DbContext);

            var customization = await db.DbContext.CCMPlayerCustomization
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            customization ??= db.DbContext.CCMPlayerCustomization
                .Add(new CCMPlayerCustomization { PlayerId = player })
                .Entity;

            foreach (var selection in snapshot.Selections)
            {
                switch (selection.SlotId)
                {
                    case "xeno_defender":
                        customization.XenoDefenderSkinId = selection.ValueId;
                        break;
                    case "xeno_drone":
                        customization.XenoDroneSkinId = selection.ValueId;
                        break;
                    case "xeno_queen":
                        customization.XenoQueenSkinId = selection.ValueId;
                        break;
                    case "xeno_runner":
                        customization.XenoRunnerSkinId = selection.ValueId;
                        break;
                    case "xeno_sentinel":
                        customization.XenoSentinelSkinId = selection.ValueId;
                        break;
                    case "ghost":
                        customization.GhostSkinId = selection.ValueId;
                        break;
                    case "weapon_spray":
                        customization.WeaponSprayId = selection.ValueId;
                        break;
                    case "armor_palette":
                        customization.ArmorPaletteId = selection.ValueId;
                        break;
                    case "armor_variant":
                        customization.ArmorVariantId = selection.ValueId;
                        break;
                    case "armor_paint":
                        customization.ArmorPaintId = selection.ValueId;
                        break;
                }
            }

            customization.SelectedOocTagId = snapshot.SelectedOocTagId;
            customization.CustomOocTagText = snapshot.CustomOocTagText;
            customization.SelectedOocColorId = snapshot.SelectedOocColorId;
            customization.SelectedLoocColorId = snapshot.SelectedLoocColorId;

            await db.DbContext.SaveChangesAsync();
        }

        private static async Task EnsureCCMCustomizationCompatibility(DbContext dbContext)
        {
            const string addArmorVariantSqlite =
                "ALTER TABLE ccm_player_customization ADD COLUMN armor_variant_id TEXT NOT NULL DEFAULT ''";
            const string addArmorVariantSqliteAlt =
                "ALTER TABLE ccm_player_customization ADD COLUMN armor_variant_id TEXT DEFAULT ''";
            const string addArmorVariantPostgres =
                "ALTER TABLE ccm_player_customization ADD COLUMN IF NOT EXISTS armor_variant_id text NOT NULL DEFAULT ''";
            const string backfillArmorVariantSqlite =
                "UPDATE ccm_player_customization SET armor_variant_id = '' WHERE armor_variant_id IS NULL";

            try
            {
                var provider = dbContext.Database.ProviderName ?? string.Empty;
                if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    // Avoid spamming failing ALTER TABLE on every access. Check first, and retry a few times
                    // in case the sqlite DB is temporarily locked.
                    for (var attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            if (await HasTableColumn(dbContext.Database.GetDbConnection(), "ccm_player_customization", "armor_variant_id"))
                                return;

                            try
                            {
                                await dbContext.Database.ExecuteSqlRawAsync(addArmorVariantSqlite);
                                return;
                            }
                            catch (SqliteException e) when (e.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase))
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(150 * (attempt + 1)));
                                continue;
                            }
                            catch (SqliteException e) when (e.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase))
                            {
                                // If the table truly doesn't exist, migrations should create it.
                                return;
                            }
                            catch (SqliteException e) when (e.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
                            {
                                return;
                            }
                            catch (SqliteException)
                            {
                                // Fallback for older/broken sqlite variants: add nullable + backfill.
                                await dbContext.Database.ExecuteSqlRawAsync(addArmorVariantSqliteAlt);
                                await dbContext.Database.ExecuteSqlRawAsync(backfillArmorVariantSqlite);
                                return;
                            }
                        }
                        catch (SqliteException e) when (e.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase))
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(150 * (attempt + 1)));
                        }
                    }

                    return;
                }

                if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                    provider.Contains("Postgre", StringComparison.OrdinalIgnoreCase))
                {
                    await dbContext.Database.ExecuteSqlRawAsync(addArmorVariantPostgres);
                }
            }
            catch (SqliteException e) when (e.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (Exception e) when (e.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
            }
        }

        private static async Task EnsureCCMLeaderboardResetStorage(DbContext dbContext)
        {
            const string createTable = @"
CREATE TABLE IF NOT EXISTS ccm_leaderboard_reset (
    category INTEGER NOT NULL,
    timeframe INTEGER NOT NULL,
    period_year INTEGER NOT NULL,
    period_month INTEGER NOT NULL,
    player_id TEXT NOT NULL,
    baseline_score INTEGER NOT NULL,
    PRIMARY KEY (category, timeframe, period_year, period_month, player_id)
)";

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(createTable);
            }
            catch (SqliteException e) when (e.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(200);
                await dbContext.Database.ExecuteSqlRawAsync(createTable);
            }
        }

        private static async Task EnsureHiddenBanStorage(DbContext dbContext)
        {
            const string createTable = @"
CREATE TABLE IF NOT EXISTS hidden_ban (
    user_id TEXT PRIMARY KEY
)";

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(createTable);
            }
            catch (SqliteException e) when (e.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(200);
                await dbContext.Database.ExecuteSqlRawAsync(createTable);
            }

        }

        private static async Task EnsureTableColumn(
            DbContext dbContext,
            DbConnection connection,
            string tableName,
            string columnName,
            string columnType)
        {
            if (await HasTableColumn(connection, tableName, columnName))
                return;

            var alterTable = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}";

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync(alterTable);
            }
            catch (SqliteException e) when (e.Message.Contains("database is locked", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Delay(200);
                await dbContext.Database.ExecuteSqlRawAsync(alterTable);
            }
        }

        private static void AddCommandParameter(DbCommand command, string name, object? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static IPAddress? NormalizeHiddenBanAddress(IPAddress? address)
        {
            if (address == null)
                return null;

            return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
        }

        private static async Task<bool> HasTableColumn(DbConnection connection, string tableName, string columnName)
        {
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                var connectionType = connection.GetType().FullName ?? string.Empty;
                var isNpgsql = connectionType.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                               connectionType.Contains("Postgre", StringComparison.OrdinalIgnoreCase);

                if (isNpgsql)
                {
                    await using var postgresCommand = connection.CreateCommand();
                    postgresCommand.CommandText = @"
                        SELECT EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = @table
                              AND column_name = @column
                        )";

                    AddCommandParameter(postgresCommand, "@table", tableName);
                    AddCommandParameter(postgresCommand, "@column", columnName);
                    return await postgresCommand.ExecuteScalarAsync() is true;
                }

                await using (var existsCommand = connection.CreateCommand())
                {
                    existsCommand.CommandText =
                        "SELECT 1 FROM sqlite_master WHERE type='table' AND name=@table LIMIT 1";
                    AddCommandParameter(existsCommand, "@table", tableName);

                    var exists = await existsCommand.ExecuteScalarAsync();
                    if (exists == null)
                        return false;
                }

                await using var pragmaCommand = connection.CreateCommand();
                pragmaCommand.CommandText = $"PRAGMA table_info('{tableName}')";
                await using var reader = await pragmaCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task SaveCCMRoundStats(
            Guid player,
            int year,
            int month,
            int roundsPlayed,
            int roundsWon,
            int roundsLost,
            int roundSecondsPlayed,
            int totalDamageDealt,
            int totalKills,
            int victoryPoints,
            int impactPoints,
            int revives,
            int healingDone,
            int structuresBuilt,
            int deaths,
            int shotsFired,
            int marineRoundsPlayed,
            int marineRoundsWon,
            int marineRoundsLost,
            int marineDamageDealt,
            int marineKills,
            int marineVictoryPoints,
            int marineImpactPoints,
            int marineRevives,
            int marineHealingDone,
            int marineStructuresBuilt,
            int marineDeaths,
            int marineShotsFired,
            int xenoRoundsPlayed,
            int xenoRoundsWon,
            int xenoRoundsLost,
            int xenoDamageDealt,
            int xenoKills,
            int xenoVictoryPoints,
            int xenoImpactPoints,
            int xenoHealingDone,
            int xenoStructuresBuilt,
            int xenoDeaths,
            int xenoShotsFired)
        {
            await using var db = await GetDb();

            var stats = await db.DbContext.CCMPlayerStats
                .FirstOrDefaultAsync(s => s.PlayerId == player);

            stats ??= db.DbContext.CCMPlayerStats
                .Add(new CCMPlayerStats { PlayerId = player })
                .Entity;

            stats.RoundsPlayed += roundsPlayed;
            stats.RoundsWon += roundsWon;
            stats.RoundsLost += roundsLost;
            stats.RoundSecondsPlayed += roundSecondsPlayed;
            stats.TotalDamageDealt += totalDamageDealt;
            stats.TotalKills += totalKills;
            stats.VictoryPoints += victoryPoints;
            stats.ImpactPoints += impactPoints;
            stats.Revives += revives;
            stats.HealingDone += healingDone;
            stats.StructuresBuilt += structuresBuilt;
            stats.Deaths += deaths;
            stats.ShotsFired += shotsFired;
            stats.MarineRoundsPlayed += marineRoundsPlayed;
            stats.MarineRoundsWon += marineRoundsWon;
            stats.MarineRoundsLost += marineRoundsLost;
            stats.MarineDamageDealt += marineDamageDealt;
            stats.MarineKills += marineKills;
            stats.MarineVictoryPoints += marineVictoryPoints;
            stats.MarineImpactPoints += marineImpactPoints;
            stats.MarineRevives += marineRevives;
            stats.MarineHealingDone += marineHealingDone;
            stats.MarineStructuresBuilt += marineStructuresBuilt;
            stats.MarineDeaths += marineDeaths;
            stats.MarineShotsFired += marineShotsFired;
            stats.XenoRoundsPlayed += xenoRoundsPlayed;
            stats.XenoRoundsWon += xenoRoundsWon;
            stats.XenoRoundsLost += xenoRoundsLost;
            stats.XenoDamageDealt += xenoDamageDealt;
            stats.XenoKills += xenoKills;
            stats.XenoVictoryPoints += xenoVictoryPoints;
            stats.XenoImpactPoints += xenoImpactPoints;
            stats.XenoHealingDone += xenoHealingDone;
            stats.XenoStructuresBuilt += xenoStructuresBuilt;
            stats.XenoDeaths += xenoDeaths;
            stats.XenoShotsFired += xenoShotsFired;

            var monthly = await db.DbContext.CCMPlayerMonthlyStats
                .FirstOrDefaultAsync(s => s.PlayerId == player && s.Year == year && s.Month == month);

            monthly ??= db.DbContext.CCMPlayerMonthlyStats
                .Add(new CCMPlayerMonthlyStats
                {
                    PlayerId = player,
                    Year = year,
                    Month = month,
                })
                .Entity;

            monthly.RoundsPlayed += roundsPlayed;
            monthly.RoundsWon += roundsWon;
            monthly.RoundsLost += roundsLost;
            monthly.RoundSecondsPlayed += roundSecondsPlayed;
            monthly.TotalDamageDealt += totalDamageDealt;
            monthly.TotalKills += totalKills;
            monthly.VictoryPoints += victoryPoints;
            monthly.ImpactPoints += impactPoints;
            monthly.Revives += revives;
            monthly.HealingDone += healingDone;
            monthly.StructuresBuilt += structuresBuilt;
            monthly.Deaths += deaths;
            monthly.ShotsFired += shotsFired;
            monthly.MarineRoundsPlayed += marineRoundsPlayed;
            monthly.MarineRoundsWon += marineRoundsWon;
            monthly.MarineRoundsLost += marineRoundsLost;
            monthly.MarineDamageDealt += marineDamageDealt;
            monthly.MarineKills += marineKills;
            monthly.MarineVictoryPoints += marineVictoryPoints;
            monthly.MarineImpactPoints += marineImpactPoints;
            monthly.MarineRevives += marineRevives;
            monthly.MarineHealingDone += marineHealingDone;
            monthly.MarineStructuresBuilt += marineStructuresBuilt;
            monthly.MarineDeaths += marineDeaths;
            monthly.MarineShotsFired += marineShotsFired;
            monthly.XenoRoundsPlayed += xenoRoundsPlayed;
            monthly.XenoRoundsWon += xenoRoundsWon;
            monthly.XenoRoundsLost += xenoRoundsLost;
            monthly.XenoDamageDealt += xenoDamageDealt;
            monthly.XenoKills += xenoKills;
            monthly.XenoVictoryPoints += xenoVictoryPoints;
            monthly.XenoImpactPoints += xenoImpactPoints;
            monthly.XenoHealingDone += xenoHealingDone;
            monthly.XenoStructuresBuilt += xenoStructuresBuilt;
            monthly.XenoDeaths += xenoDeaths;
            monthly.XenoShotsFired += xenoShotsFired;

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<CCMLeaderboardPage> GetCCMLeaderboard(
            Guid viewer,
            CCMLeaderboardCategory category,
            CCMLeaderboardTimeframe timeframe,
            int page,
            int pageSize)
        {
            await using var db = await GetDb();

            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);
            var (periodYear, periodMonth) = GetCCMLeaderboardPeriod(timeframe);

            IQueryable<LeaderboardRow> rawQuery = timeframe == CCMLeaderboardTimeframe.CurrentMonth
                ? GetMonthlyLeaderboardQuery(db, periodYear, periodMonth, category)
                : GetAllTimeLeaderboardQuery(db, category);

            var rawRows = await rawQuery.ToListAsync();
            var baselines = await GetCCMLeaderboardResetBaselines(db.DbContext, category, timeframe, periodYear, periodMonth);

            var rows = rawRows
                .Select(row => new LeaderboardRow
                {
                    PlayerId = row.PlayerId,
                    Ckey = row.Ckey,
                    Score = Math.Max(0, row.Score - baselines.GetValueOrDefault(row.PlayerId)),
                })
                .Where(row => row.Score > 0)
                .OrderByDescending(row => row.Score)
                .ThenBy(row => row.Ckey, StringComparer.Ordinal)
                .ToList();

            var totalEntries = rows.Count;
            var totalPages = Math.Max(1, (int) Math.Ceiling(totalEntries / (float) pageSize));
            page = Math.Min(page, totalPages);

            var pageRows = rows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var entries = pageRows
                .Select((row, index) => new CCMLeaderboardEntry(
                    (page - 1) * pageSize + index + 1,
                    row.Ckey,
                    row.Score,
                    row.PlayerId == viewer))
                .ToArray();

            CCMLeaderboardEntry? viewerEntry = null;
            var viewerRow = rows.FirstOrDefault(r => r.PlayerId == viewer);
            if (viewerRow != null)
            {
                var higherCount = rows.Count(r => r.Score > viewerRow.Score);
                var viewerRank = higherCount + 1;
                var pageStart = (page - 1) * pageSize + 1;
                var pageEnd = pageStart + pageSize - 1;
                if (viewerRank < pageStart || viewerRank > pageEnd)
                    viewerEntry = new CCMLeaderboardEntry(viewerRank, viewerRow.Ckey, viewerRow.Score, true);
            }

            return new CCMLeaderboardPage(category, timeframe, page, totalPages, entries, viewerEntry);
        }

        public async Task<int> ResetCCMLeaderboard(
            CCMLeaderboardCategory category,
            CCMLeaderboardTimeframe timeframe)
        {
            await using var db = await GetDb();
            await EnsureCCMLeaderboardResetStorage(db.DbContext);

            var (periodYear, periodMonth) = GetCCMLeaderboardPeriod(timeframe);

            IQueryable<LeaderboardRow> rawQuery = timeframe == CCMLeaderboardTimeframe.CurrentMonth
                ? GetMonthlyLeaderboardQuery(db, periodYear, periodMonth, category)
                : GetAllTimeLeaderboardQuery(db, category);

            var rows = await rawQuery
                .Where(row => row.Score > 0)
                .ToListAsync();

            var connection = db.DbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var transaction = await connection.BeginTransactionAsync();

                await using (var deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.CommandText = """
                        DELETE FROM ccm_leaderboard_reset
                        WHERE category = @category
                          AND timeframe = @timeframe
                          AND period_year = @periodYear
                          AND period_month = @periodMonth
                        """;
                    AddCommandParameter(deleteCommand, "@category", (int) category);
                    AddCommandParameter(deleteCommand, "@timeframe", (int) timeframe);
                    AddCommandParameter(deleteCommand, "@periodYear", periodYear);
                    AddCommandParameter(deleteCommand, "@periodMonth", periodMonth);
                    await deleteCommand.ExecuteNonQueryAsync();
                }

                foreach (var row in rows)
                {
                    await using var insertCommand = connection.CreateCommand();
                    insertCommand.Transaction = transaction;
                    insertCommand.CommandText = """
                        INSERT INTO ccm_leaderboard_reset
                            (category, timeframe, period_year, period_month, player_id, baseline_score)
                        VALUES
                            (@category, @timeframe, @periodYear, @periodMonth, @playerId, @baselineScore)
                        """;
                    AddCommandParameter(insertCommand, "@category", (int) category);
                    AddCommandParameter(insertCommand, "@timeframe", (int) timeframe);
                    AddCommandParameter(insertCommand, "@periodYear", periodYear);
                    AddCommandParameter(insertCommand, "@periodMonth", periodMonth);
                    AddCommandParameter(insertCommand, "@playerId", row.PlayerId.ToString());
                    AddCommandParameter(insertCommand, "@baselineScore", row.Score);
                    await insertCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return rows.Count;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static IQueryable<LeaderboardRow> GetAllTimeLeaderboardQuery(DbGuard db, CCMLeaderboardCategory category)
        {
            return category switch
            {
                CCMLeaderboardCategory.OverallVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.VictoryPoints,
                    }),
                    db),
                CCMLeaderboardCategory.OverallKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.TotalKills,
                    }),
                    db),
                CCMLeaderboardCategory.MarineVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.MarineVictoryPoints,
                    }),
                    db),
                CCMLeaderboardCategory.MarineImpact => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.MarineImpactPoints,
                    }),
                    db),
                CCMLeaderboardCategory.MarineKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.MarineKills,
                    }),
                    db),
                CCMLeaderboardCategory.XenoVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.XenoVictoryPoints,
                    }),
                    db),
                CCMLeaderboardCategory.XenoImpact => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.XenoImpactPoints,
                    }),
                    db),
                CCMLeaderboardCategory.XenoKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerStats.Select(s => new LeaderboardScoreProjection
                    {
                        PlayerId = s.PlayerId,
                        Score = s.XenoKills,
                    }),
                    db),
                _ => Enumerable.Empty<LeaderboardRow>().AsQueryable(),
            };
        }

        private static IQueryable<LeaderboardRow> GetMonthlyLeaderboardQuery(DbGuard db, int year, int month, CCMLeaderboardCategory category)
        {
            return category switch
            {
                CCMLeaderboardCategory.OverallVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.VictoryPoints,
                        }),
                    db),
                CCMLeaderboardCategory.OverallKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.TotalKills,
                        }),
                    db),
                CCMLeaderboardCategory.MarineVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.MarineVictoryPoints,
                        }),
                    db),
                CCMLeaderboardCategory.MarineImpact => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.MarineImpactPoints,
                        }),
                    db),
                CCMLeaderboardCategory.MarineKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.MarineKills,
                        }),
                    db),
                CCMLeaderboardCategory.XenoVictoryPoints => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.XenoVictoryPoints,
                        }),
                    db),
                CCMLeaderboardCategory.XenoImpact => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.XenoImpactPoints,
                        }),
                    db),
                CCMLeaderboardCategory.XenoKills => ProjectLeaderboard(
                    db.DbContext.CCMPlayerMonthlyStats
                        .Where(s => s.Year == year && s.Month == month)
                        .Select(s => new LeaderboardScoreProjection
                        {
                            PlayerId = s.PlayerId,
                            Score = s.XenoKills,
                        }),
                    db),
                _ => Enumerable.Empty<LeaderboardRow>().AsQueryable(),
            };
        }

        private static IQueryable<LeaderboardRow> ProjectLeaderboard(IQueryable<LeaderboardScoreProjection> scores, DbGuard db)
        {
            return scores.Join(
                db.DbContext.Player,
                stats => stats.PlayerId,
                player => player.UserId,
                (stats, player) => new LeaderboardRow
                {
                    PlayerId = stats.PlayerId,
                    Ckey = player.LastSeenUserName,
                    Score = stats.Score,
                });
        }

        private static async Task<Dictionary<Guid, int>> GetCCMLeaderboardResetBaselines(
            DbContext dbContext,
            CCMLeaderboardCategory category,
            CCMLeaderboardTimeframe timeframe,
            int periodYear,
            int periodMonth)
        {
            await EnsureCCMLeaderboardResetStorage(dbContext);

            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT player_id, baseline_score
                    FROM ccm_leaderboard_reset
                    WHERE category = @category
                      AND timeframe = @timeframe
                      AND period_year = @periodYear
                      AND period_month = @periodMonth
                    """;
                AddCommandParameter(command, "@category", (int) category);
                AddCommandParameter(command, "@timeframe", (int) timeframe);
                AddCommandParameter(command, "@periodYear", periodYear);
                AddCommandParameter(command, "@periodMonth", periodMonth);

                var baselines = new Dictionary<Guid, int>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1))
                        continue;

                    var playerText = reader.GetString(0);
                    if (!Guid.TryParse(playerText, out var playerId))
                        continue;

                    baselines[playerId] = reader.GetInt32(1);
                }

                return baselines;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        private static (int Year, int Month) GetCCMLeaderboardPeriod(CCMLeaderboardTimeframe timeframe)
        {
            if (timeframe == CCMLeaderboardTimeframe.CurrentMonth)
            {
                var now = DateTime.UtcNow;
                return (now.Year, now.Month);
            }

            return (0, 0);
        }

        private static CCMPlayerStatsSnapshot ToCCMStatsSnapshot(CCMPlayerStats? stats)
        {
            if (stats == null)
                return new CCMPlayerStatsSnapshot(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            return new CCMPlayerStatsSnapshot(
                stats.RoundsPlayed,
                stats.RoundsWon,
                stats.RoundsLost,
                stats.RoundSecondsPlayed,
                stats.TotalDamageDealt,
                stats.TotalKills,
                stats.VictoryPoints,
                stats.ImpactPoints,
                stats.Revives,
                stats.HealingDone,
                stats.StructuresBuilt,
                stats.Deaths,
                stats.ShotsFired,
                stats.MarineRoundsPlayed,
                stats.MarineRoundsWon,
                stats.MarineRoundsLost,
                stats.MarineDamageDealt,
                stats.MarineKills,
                stats.MarineVictoryPoints,
                stats.MarineImpactPoints,
                stats.MarineRevives,
                stats.MarineHealingDone,
                stats.MarineStructuresBuilt,
                stats.MarineDeaths,
                stats.MarineShotsFired,
                stats.XenoRoundsPlayed,
                stats.XenoRoundsWon,
                stats.XenoRoundsLost,
                stats.XenoDamageDealt,
                stats.XenoKills,
                stats.XenoVictoryPoints,
                stats.XenoImpactPoints,
                stats.XenoHealingDone,
                stats.XenoStructuresBuilt,
                stats.XenoDeaths,
                stats.XenoShotsFired);
        }

        private static CCMPlayerAchievementStatsSnapshot ToCCMAchievementStatsSnapshot(CCMPlayerAchievementStats? stats)
        {
            if (stats == null)
                return new CCMPlayerAchievementStatsSnapshot(0, 0, 0, 0, 0, 0, 0, Array.Empty<string>());

            var unlocked = string.IsNullOrWhiteSpace(stats.UnlockedAchievementIds)
                ? Array.Empty<string>()
                : stats.UnlockedAchievementIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return new CCMPlayerAchievementStatsSnapshot(
                stats.FriendlyFireDamage,
                stats.RequisitionOrders,
                stats.XenoEvolutions,
                stats.OfficerWins,
                stats.QueenKills,
                stats.QueenWins,
                stats.QueenKillParticipations,
                unlocked);
        }

        private static CCMCustomizationSnapshot ToCCMCustomizationSnapshot(CCMPlayerCustomization? customization)
        {
            if (customization == null)
            {
                return new CCMCustomizationSnapshot(Array.Empty<CCMCustomizationSelectionData>());
            }

            return new CCMCustomizationSnapshot(
                new[]
                {
                    new CCMCustomizationSelectionData("xeno_defender", customization.XenoDefenderSkinId),
                    new CCMCustomizationSelectionData("xeno_drone", customization.XenoDroneSkinId),
                    new CCMCustomizationSelectionData("xeno_queen", customization.XenoQueenSkinId),
                    new CCMCustomizationSelectionData("xeno_runner", customization.XenoRunnerSkinId),
                    new CCMCustomizationSelectionData("xeno_sentinel", customization.XenoSentinelSkinId),
                    new CCMCustomizationSelectionData("ghost", customization.GhostSkinId),
                    new CCMCustomizationSelectionData("weapon_spray", customization.WeaponSprayId),
                    new CCMCustomizationSelectionData("armor_palette", customization.ArmorPaletteId),
                    new CCMCustomizationSelectionData("armor_variant", customization.ArmorVariantId),
                    new CCMCustomizationSelectionData("armor_paint", customization.ArmorPaintId),
                },
                customization.SelectedOocTagId,
                customization.CustomOocTagText,
                customization.SelectedOocColorId,
                customization.SelectedLoocColorId);
        }

        private static int GetLeaderboardScore(CCMPlayerStats stats, CCMLeaderboardCategory category)
        {
            return category switch
            {
                CCMLeaderboardCategory.OverallVictoryPoints => stats.VictoryPoints,
                CCMLeaderboardCategory.OverallKills => stats.TotalKills,
                CCMLeaderboardCategory.MarineVictoryPoints => stats.MarineVictoryPoints,
                CCMLeaderboardCategory.MarineImpact => stats.MarineImpactPoints,
                CCMLeaderboardCategory.MarineKills => stats.MarineKills,
                CCMLeaderboardCategory.XenoVictoryPoints => stats.XenoVictoryPoints,
                CCMLeaderboardCategory.XenoImpact => stats.XenoImpactPoints,
                CCMLeaderboardCategory.XenoKills => stats.XenoKills,
                _ => 0,
            };
        }

        private static int GetLeaderboardScore(CCMPlayerMonthlyStats stats, CCMLeaderboardCategory category)
        {
            return category switch
            {
                CCMLeaderboardCategory.OverallVictoryPoints => stats.VictoryPoints,
                CCMLeaderboardCategory.OverallKills => stats.TotalKills,
                CCMLeaderboardCategory.MarineVictoryPoints => stats.MarineVictoryPoints,
                CCMLeaderboardCategory.MarineImpact => stats.MarineImpactPoints,
                CCMLeaderboardCategory.MarineKills => stats.MarineKills,
                CCMLeaderboardCategory.XenoVictoryPoints => stats.XenoVictoryPoints,
                CCMLeaderboardCategory.XenoImpact => stats.XenoImpactPoints,
                CCMLeaderboardCategory.XenoKills => stats.XenoKills,
                _ => 0,
            };
        }

        private sealed class LeaderboardRow
        {
            public Guid PlayerId { get; init; }
            public string Ckey { get; init; } = string.Empty;
            public int Score { get; init; }
        }

        private sealed class LeaderboardScoreProjection
        {
            public Guid PlayerId { get; init; }
            public int Score { get; init; }
        }

        public async Task<(int MarineWins, int XenoWins)> GetCCMRoundWinStats()
        {
            await using var db = await GetDb();
            var stats = await db.DbContext.CCMRoundWinStats
                .FirstOrDefaultAsync(s => s.Id == 1);

            stats ??= await TryImportLegacyCCMRoundWinStats(db);

            return stats == null
                ? (0, 0)
                : (stats.MarineWins, stats.XenoWins);
        }

        public async Task<(int MarineWins, int XenoWins)> AdjustCCMRoundWinStats(int marineDelta, int xenoDelta)
        {
            await using var db = await GetDb();
            var stats = await db.DbContext.CCMRoundWinStats
                .FirstOrDefaultAsync(s => s.Id == 1);

            stats ??= await TryImportLegacyCCMRoundWinStats(db);

            stats ??= db.DbContext.CCMRoundWinStats
                .Add(new CCMRoundWinStats
                {
                    Id = 1,
                })
                .Entity;

            stats.MarineWins = Math.Max(0, stats.MarineWins + marineDelta);
            stats.XenoWins = Math.Max(0, stats.XenoWins + xenoDelta);

            await db.DbContext.SaveChangesAsync();

            return (stats.MarineWins, stats.XenoWins);
        }

        private static async Task<CCMRoundWinStats?> TryImportLegacyCCMRoundWinStats(DbGuard db)
        {
            try
            {
                var connection = db.DbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                var provider = db.DbContext.Database.ProviderName ?? string.Empty;
                if (!await HasDatabaseTable(connection, provider, "rmc_round_win_stats"))
                    return null;

                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT marine_wins, xeno_wins FROM rmc_round_win_stats LIMIT 1";

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return null;

                var stats = db.DbContext.CCMRoundWinStats
                    .Add(new CCMRoundWinStats
                    {
                        Id = 1,
                        MarineWins = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        XenoWins = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    })
                    .Entity;

                await db.DbContext.SaveChangesAsync();
                return stats;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> HasDatabaseTable(DbConnection connection, string provider, string tableName)
        {
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                if (provider.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    command.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name=@table LIMIT 1";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "table";
                    parameter.Value = tableName;
                    command.Parameters.Add(parameter);

                    return await command.ExecuteScalarAsync() != null;
                }

                if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                    provider.Contains("Postgre", StringComparison.OrdinalIgnoreCase))
                {
                    command.CommandText = @"
                        SELECT EXISTS (
                            SELECT 1
                            FROM information_schema.tables
                            WHERE table_schema = 'public'
                              AND table_name = @table
                        )";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "table";
                    parameter.Value = tableName;
                    command.Parameters.Add(parameter);

                    return await command.ExecuteScalarAsync() is true;
                }

                return false;
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }
        }

        public async Task<Dictionary<string, List<string>>?> GetActionOrder(Guid player)
        {
            await using var db = await GetDb();
            return await db.DbContext.RMCPlayerActionOrder
                .Where(a => a.PlayerId == player)
                .ToDictionaryAsync(a => a.Id, a => a.Actions);
        }

        public async Task SetActionOrder(Guid player, string id, List<string> actions)
        {
            await using var db = await GetDb();
            var order = await db.DbContext.RMCPlayerActionOrder
                .FirstOrDefaultAsync(a => a.PlayerId == player && a.Id == id);

            order ??= db.DbContext.RMCPlayerActionOrder
                .Add(new RMCPlayerActionOrder
                {
                    PlayerId = player,
                    Id = id,
                })
                .Entity;

            order.Actions = new List<string>(actions);

            await db.DbContext.SaveChangesAsync();
        }

        public async Task AddChatBan(int? round, NetUserId target, (IPAddress, int)? addressRange, ImmutableTypedHwid? hwid, TimeSpan? duration, ChatType type, NetUserId admin, string reason)
        {
            await using var db = await GetDb();

            var time = DateTimeOffset.UtcNow.UtcDateTime;
            db.DbContext.RMCPlayerChatBans.Add(new RMCChatBans
            {
                RoundId = round,
                PlayerId = target,
                Address = addressRange.ToNpgsqlInet(),
                HWId = hwid,
                Type = type,
                BanningAdminId = admin,
                Reason = reason,
                BannedAt = time,
                ExpiresAt = duration == null ? null : time.Add(duration.Value),
            });

            await db.DbContext.SaveChangesAsync();
        }

        public async Task<List<RMCChatBans>> GetAllChatBans(Guid player)
        {
            await using var db = await GetDb();
            return await db.DbContext.RMCPlayerChatBans
                .Include(b => b.UnbanningAdmin)
                .Where(c => c.PlayerId == player)
                .ToListAsync();
        }

        public async Task<List<RMCChatBans>> GetActiveChatBans(Guid player)
        {
            await using var db = await GetDb();
            return await db.DbContext.RMCPlayerChatBans
                .Include(b => b.UnbanningAdmin)
                .Where(c => c.PlayerId == player)
                .Where(c => c.UnbannedAt == null && (c.ExpiresAt == null || c.ExpiresAt.Value > DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<Guid?> TryPardonChatBan(int id, Guid? admin)
        {
            await using var db = await GetDb();
            var ban = await db.DbContext.RMCPlayerChatBans.FirstOrDefaultAsync(c => c.Id == id);
            if (ban == null || ban.UnbanningAdminId != null)
                return null;

            ban.UnbanningAdminId = admin;
            ban.UnbannedAt = DateTimeOffset.UtcNow.UtcDateTime;
            await db.DbContext.SaveChangesAsync();
            return ban.PlayerId;
        }

        #endregion

        public abstract Task SendNotification(DatabaseNotification notification);

        // SQLite returns DateTime as Kind=Unspecified, Npgsql actually knows for sure it's Kind=Utc.
        // Normalize DateTimes here so they're always Utc. Thanks.
        protected abstract DateTime NormalizeDatabaseTime(DateTime time);

        [return: NotNullIfNotNull(nameof(time))]
        protected DateTime? NormalizeDatabaseTime(DateTime? time)
        {
            return time != null ? NormalizeDatabaseTime(time.Value) : time;
        }

        public async Task<bool> HasPendingModelChanges()
        {
            await using var db = await GetDb();
            return db.DbContext.Database.HasPendingModelChanges();
        }

        protected abstract Task<DbGuard> GetDb(
            CancellationToken cancel = default,
            [CallerMemberName] string? name = null);

        protected void LogDbOp(string? name)
        {
            _opsLog.Verbose($"Running DB operation: {name ?? "unknown"}");
        }

        protected abstract class DbGuard : IAsyncDisposable
        {
            public abstract ServerDbContext DbContext { get; }

            public abstract ValueTask DisposeAsync();
        }

        protected void NotificationReceived(DatabaseNotification notification)
        {
            OnNotificationReceived?.Invoke(notification);
        }

        public virtual void Shutdown()
        {

        }
    }
}

// # CCM priority rework
