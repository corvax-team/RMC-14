using System.Linq;
using Content.Shared._RMC14.GhostColor;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._RMC14.GhostColor;

public sealed class GhostColorSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly ResPath DefaultGhostRsiPath = new("Mobs/Ghosts/ghost_human.rsi");

    public override void Update(float frameTime)
    {
        var defaultColor = Color.FromHex("#FFFFFF88");
        var colors = EntityQueryEnumerator<GhostColorComponent, SpriteComponent>();
        while (colors.MoveNext(out var uid, out var color, out var sprite))
        {
            sprite.Color = color.Color ?? defaultColor;
            UpdateGhostSkin(uid, sprite, color);
        }
    }

    private void UpdateGhostSkin(EntityUid uid, SpriteComponent sprite, GhostColorComponent component)
    {
        if (!sprite.AllLayers.Any())
            return;

        var targetPath = string.IsNullOrWhiteSpace(component.RsiPath)
            ? DefaultGhostRsiPath
            : new ResPath(component.RsiPath);

        if (sprite[0].ActualRsi?.Path == targetPath)
            return;

        _sprite.LayerSetRsi((uid, sprite), 0, targetPath, "animated");
    }
}
