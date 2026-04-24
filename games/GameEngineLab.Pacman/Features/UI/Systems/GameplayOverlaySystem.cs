using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class GameplayOverlaySystem : IGameSystem
{
    public int Order => 120;

    public void Update(World world, FrameContext frameContext)
    {
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay)
        {
            return;
        }

        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var collectibles = world.GetRequiredResource<CollectiblesResource>();

        DrawTopBar(frameContext, gameplay, collectibles);

        if (gameplay.IsGameOver)
        {
            DrawCenterOverlay(frameContext, new Color(150, 24, 24, 180));
        }
        else if (gameplay.IsWin)
        {
            DrawCenterOverlay(frameContext, new Color(24, 130, 56, 180));
        }
    }

    private static void DrawTopBar(FrameContext frameContext, GameplayStateResource gameplay, CollectiblesResource collectibles)
    {
        var bar = new Rectangle(0, 0, 1024, 34);
        frameContext.SpriteBatch!.Draw(frameContext.DebugPixel!, bar, new Color(6, 10, 20, 220));

        for (var i = 0; i < gameplay.Lives; i++)
        {
            var lifeRect = new Rectangle(12 + (i * 16), 10, 10, 10);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, lifeRect, new Color(255, 214, 74));
        }

        var scoreWidth = Math.Clamp(gameplay.Score / 4, 0, 300);
        var scoreRect = new Rectangle(120, 12, scoreWidth, 6);
        frameContext.SpriteBatch.Draw(frameContext.DebugPixel, scoreRect, new Color(93, 197, 255));

        var remaining = collectibles.TotalCount;
        var remainingWidth = Math.Clamp(remaining * 2, 0, 300);
        var remainingRect = new Rectangle(120, 22, remainingWidth, 4);
        frameContext.SpriteBatch.Draw(frameContext.DebugPixel, remainingRect, new Color(220, 230, 255));
    }

    private static void DrawCenterOverlay(FrameContext frameContext, Color color)
    {
        var rect = new Rectangle(300, 180, 420, 180);
        frameContext.SpriteBatch!.Draw(frameContext.DebugPixel!, rect, color);
        var accent = new Rectangle(rect.X + 20, rect.Y + 20, rect.Width - 40, rect.Height - 40);
        frameContext.SpriteBatch.Draw(frameContext.DebugPixel, accent, new Color(255, 255, 255, 30));
    }
}
