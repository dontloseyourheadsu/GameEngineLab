using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class GameplayOverlaySystem : IGameSystem
{
    public int Order => 120;

    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorNeonRed = new(255, 50, 50);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);

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
        var options = world.GetRequiredResource<OptionsResource>();

        var viewport = frameContext.Viewport;
        float autoScale = Math.Max(1.0f, Math.Min(viewport.Width / 1024f, viewport.Height / 768f));
        float scale = options.UiScale * autoScale;

        DrawTopBar(frameContext, gameplay, collectibles, scale);

        if (gameplay.IsGameOver)
        {
            DrawCenterOverlay(frameContext, "GAME OVER", ColorNeonRed, scale);
        }
        else if (gameplay.IsWin)
        {
            DrawCenterOverlay(frameContext, "YOU WIN!", ColorNeonGreen, scale);
        }
    }

    private static void DrawTopBar(FrameContext frameContext, GameplayStateResource gameplay, CollectiblesResource collectibles, float scale)
    {
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var viewport = frameContext.Viewport;
        
        var barH = (int)(60 * scale);
        var bar = new Rectangle(0, 0, viewport.Width, barH);
        sb.Draw(pixel, bar, new Color(10, 10, 25, 200));
        sb.Draw(pixel, new Rectangle(0, barH - 2, viewport.Width, 2), ColorNeonCyan);

        // Score
        var scoreText = $"SCORE: {gameplay.Score:D6}";
        PixelText.Draw(sb, pixel, scoreText, new Vector2(20 * scale, (barH - 24 * scale) / 2), (int)(2 * scale), ColorNeonCyan);

        // Items Remaining
        var itemsText = $"LEFT: {collectibles.TotalCount}";
        var itemsSize = PixelText.Measure(itemsText, (int)(2 * scale));
        PixelText.Draw(sb, pixel, itemsText, new Vector2(viewport.Width / 2 - itemsSize.X / 2, (barH - 24 * scale) / 2), (int)(2 * scale), ColorNeonYellow);

        // Lives
        var livesText = "LIVES: ";
        var livesTextSize = PixelText.Measure(livesText, (int)(2 * scale));
        var livesX = viewport.Width - (int)(180 * scale);
        PixelText.Draw(sb, pixel, livesText, new Vector2(livesX, (barH - 24 * scale) / 2), (int)(2 * scale), ColorNeonMagenta);

        for (var i = 0; i < gameplay.Lives; i++)
        {
            var lifeSize = (int)(14 * scale);
            var lifeRect = new Rectangle(livesX + livesTextSize.X + i * (int)(20 * scale), (barH - lifeSize) / 2, lifeSize, lifeSize);
            sb.Draw(pixel, lifeRect, ColorNeonYellow);
        }
    }

    private static void DrawCenterOverlay(FrameContext frameContext, string text, Color color, float scale)
    {
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var viewport = frameContext.Viewport;

        var w = (int)(500 * scale);
        var h = (int)(200 * scale);
        var rect = new Rectangle((viewport.Width - w) / 2, (viewport.Height - h) / 2, w, h);
        
        // Shadow/Blur
        sb.Draw(pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), new Color(0, 0, 0, 150));
        
        // Panel
        sb.Draw(pixel, rect, new Color(16, 16, 32));
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 4), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 4, rect.Width, 4), color);

        var tScale = (int)(5 * scale);
        var tSize = PixelText.Measure(text, tScale);
        PixelText.Draw(sb, pixel, text, new Vector2(rect.Center.X - tSize.X / 2, rect.Center.Y - tSize.Y / 2 - (int)(10 * scale)), tScale, color);
        
        var subText = "PRESS TAB FOR MENU";
        var sScale = (int)(1 * scale);
        var sSize = PixelText.Measure(subText, sScale);
        PixelText.Draw(sb, pixel, subText, new Vector2(rect.Center.X - sSize.X / 2, rect.Bottom - (int)(40 * scale)), sScale, Color.Gray);
    }
}
