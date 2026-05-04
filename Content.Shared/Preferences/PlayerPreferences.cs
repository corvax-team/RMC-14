using System.Linq;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Contains all player characters and the index of the currently selected character.
    ///     Serialized both over the network and to disk.
    /// </summary>
    [Serializable]
    [NetSerializable]
    public sealed class PlayerPreferences
    {
        private Dictionary<int, ICharacterProfile> _characters;

        public PlayerPreferences(IEnumerable<KeyValuePair<int, ICharacterProfile>> characters, int selectedCharacterIndex, Color adminOOCColor, List<ProtoId<ConstructionPrototype>> constructionFavorites)
        {
            _characters = new Dictionary<int, ICharacterProfile>(characters);
            EnsureAtLeastOneCharacter();
            SelectedCharacterIndex = ResolveSelectedCharacterIndex(selectedCharacterIndex);
            AdminOOCColor = adminOOCColor;
            ConstructionFavorites = constructionFavorites;
        }

        /// <summary>
        ///     All player characters.
        /// </summary>
        public IReadOnlyDictionary<int, ICharacterProfile> Characters => _characters;

        public ICharacterProfile GetProfile(int index)
        {
            return TryGetProfile(index, out var profile) && profile != null
                ? profile
                : ResolveSelectedCharacter();
        }

        public bool TryGetProfile(int index, out ICharacterProfile? profile)
        {
            return _characters.TryGetValue(index, out profile);
        }

        /// <summary>
        ///     Index of the currently selected character.
        /// </summary>
        public int SelectedCharacterIndex { get; }

        /// <summary>
        ///     The currently selected character.
        /// </summary>
        public ICharacterProfile SelectedCharacter => ResolveSelectedCharacter();

        public bool TryGetSelectedCharacter(out ICharacterProfile? profile)
        {
            return TryGetProfile(SelectedCharacterIndex, out profile);
        }

        private ICharacterProfile ResolveSelectedCharacter()
        {
            if (TryGetSelectedCharacter(out var profile) &&
                profile != null)
            {
                return profile;
            }

            EnsureAtLeastOneCharacter();

            foreach (var pair in _characters.OrderBy(pair => pair.Key))
            {
                return pair.Value;
            }

            return new HumanoidCharacterProfile();
        }

        private int ResolveSelectedCharacterIndex(int selectedCharacterIndex)
        {
            if (_characters.ContainsKey(selectedCharacterIndex))
                return selectedCharacterIndex;

            foreach (var pair in _characters.OrderBy(pair => pair.Key))
            {
                return pair.Key;
            }

            return 0;
        }

        private void EnsureAtLeastOneCharacter()
        {
            if (_characters.Count > 0)
                return;

            _characters[0] = new HumanoidCharacterProfile();
        }

        public Color AdminOOCColor { get; set; }

        /// <summary>
        ///    List of favorite items in the construction menu.
        /// </summary>
        public List<ProtoId<ConstructionPrototype>> ConstructionFavorites { get; set; } = [];

        public int IndexOfCharacter(ICharacterProfile profile)
        {
            return _characters.FirstOrNull(p => p.Value == profile)?.Key ?? -1;
        }

        public bool TryIndexOfCharacter(ICharacterProfile profile, out int index)
        {
            return (index = IndexOfCharacter(profile)) != -1;
        }
    }
}
