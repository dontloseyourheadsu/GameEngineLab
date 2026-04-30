using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace GameEngineLab.Core.Features.UI.Systems;

public sealed class UiRenderSystem : IGameSystem
{
    public int Order => 200; // Run late to draw over everything

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;
        if (!world.TryGetResource<UiThemeResource>(out var theme) || theme == null) return;

        var entities = world.GetEntitiesWith<UiTransformComponent>()
            .OrderBy(e => world.TryGetComponent<UiTransformComponent>(e, out var t) ? t.ZIndex : 0)
            .ToList();

        foreach (var entityId in entities)
        {
            world.TryGetComponent<UiTransformComponent>(entityId, out var transform);
            world.TryGetComponent<UiStateComponent>(entityId, out var uiState);
            bool isPressed = uiState.State == UiState.Pressed;
            bool isHovered = uiState.State == UiState.Hovered;
            bool isPanel = world.HasComponent<UiPanelComponent>(entityId);

            var bounds = transform.Bounds;
            var bgColor = theme.SurfaceColor;
            var borderColor = theme.BorderColor;

            if (isHovered && !isPanel)
            {
                bgColor = Color.Lerp(theme.SurfaceColor, theme.HighlightColor, 0.3f);
            }
            else if (isPressed && !isPanel)
            {
                bgColor = theme.HighlightColor;
            }

            // Draw Shadow (if not pressed or if it's a panel)
            if (!isPressed || isPanel)
            {
                var shadowRect = new Rectangle(bounds.X + theme.ShadowOffset, bounds.Y + theme.ShadowOffset, bounds.Width, bounds.Height);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(shadowRect.Center.X, shadowRect.Center.Y), new Vector2(shadowRect.Width, shadowRect.Height), theme.ShadowColor);
            }

            // Content Position (offset if pressed)
            var contentRect = bounds;
            if (isPressed && !isPanel)
            {
                contentRect.X += theme.PressedContentOffset;
                contentRect.Y += theme.PressedContentOffset;
            }

            // Draw Background
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Center.X, contentRect.Center.Y), new Vector2(contentRect.Width, contentRect.Height), bgColor);

            // Draw Border
            var thickness = 2;
            var bColor = isPanel ? theme.SurfaceColor : theme.BorderColor; // Cards use lighter border in HTML
            if (world.HasComponent<UiButtonComponent>(entityId)) bColor = theme.BorderColor;

            // Top
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Center.X, contentRect.Top + thickness / 2f), new Vector2(contentRect.Width, thickness), bColor);
            // Bottom
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Center.X, contentRect.Bottom - thickness / 2f), new Vector2(contentRect.Width, thickness), bColor);
            // Left
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Left + thickness / 2f, contentRect.Center.Y), new Vector2(thickness, contentRect.Height), bColor);
            // Right
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Right - thickness / 2f, contentRect.Center.Y), new Vector2(thickness, contentRect.Height), bColor);

            // Draw Checkbox
            var textXOffset = 5;
            if (world.TryGetComponent<UiCheckboxComponent>(entityId, out var checkbox))
            {
                var boxSize = 20;
                var boxRect = new Rectangle(contentRect.X + 5, contentRect.Y + (contentRect.Height - boxSize) / 2, boxSize, boxSize);
                
                // Draw Checkbox Border
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize, boxSize), theme.BorderColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize - 4, boxSize - 4), theme.ShadowColor);

                // Draw Inner box if checked
                if (checkbox.Checked)
                {
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize - 8, boxSize - 8), theme.HighlightColor);
                }

                textXOffset = 30;
            }

            // Draw Slider / Scrollbar
            if (world.TryGetComponent<UiSliderComponent>(entityId, out var slider))
            {
                var thumbSize = 24; // Increased from 16
                var trackColor = theme.ShadowColor;
                var thumbColor = theme.HighlightColor;

                // Draw Track
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(contentRect.Center.X, contentRect.Center.Y), new Vector2(contentRect.Width - 4, contentRect.Height - 4), trackColor);

                if (slider.Vertical)
                {
                    var thumbY = contentRect.Y + (int)(slider.Value * (contentRect.Height - thumbSize)) + thumbSize / 2;
                    // Draw Thumb Shadow
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X + 2, thumbY + 2), new Vector2(contentRect.Width, thumbSize), theme.ShadowColor);
                    // Draw Thumb
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X, thumbY), new Vector2(contentRect.Width, thumbSize), thumbColor);
                    // Thumb Top Highlight
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X, thumbY - thumbSize/4), new Vector2(contentRect.Width, 2), theme.TextColor);
                }
                else
                {
                    var thumbX = contentRect.X + (int)(slider.Value * (contentRect.Width - thumbSize)) + thumbSize / 2;
                    // Draw Thumb Shadow
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX + 2, contentRect.Center.Y + 2), new Vector2(thumbSize, contentRect.Height), theme.ShadowColor);
                    // Draw Thumb
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX, contentRect.Center.Y), new Vector2(thumbSize, contentRect.Height), thumbColor);
                    // Thumb Side Highlight
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX - thumbSize/4, contentRect.Center.Y), new Vector2(2, contentRect.Height), theme.TextColor);
                }
            }

            // Draw Text
            if (world.TryGetComponent<UiTextComponent>(entityId, out var uiText))
            {
                if (theme.Fonts.TryGetValue(uiText.FontName, out var font))
                {
                    var displayPath = uiText.Text;
                    if (world.TryGetComponent<UiTextInputComponent>(entityId, out var textInput))
                    {
                        bool hasGlobalFocus = world.TryGetResource<UiFocusResource>(out var focus) && focus.FocusedEntity == entityId;
                        displayPath = textInput.Text + (hasGlobalFocus && frameContext.GameTime.TotalGameTime.TotalSeconds % 1.0 < 0.5 ? "|" : "");
                    }

                    var textSize = font.MeasureString(displayPath) * uiText.Scale;
                    var textPos = new Vector2(contentRect.X + textXOffset, contentRect.Y + (contentRect.Height - textSize.Y) / 2f);

                    if (uiText.CenterX) textPos.X = contentRect.X + (contentRect.Width - textSize.X) / 2f;
                    if (uiText.CenterY) textPos.Y = contentRect.Y + (contentRect.Height - textSize.Y) / 2f;

                    var textColor = uiText.Color ?? theme.TextColor;

                    if (uiText.HasShadow)
                    {
                        frameContext.SpriteBatch.DrawString(font, displayPath, textPos + new Vector2(2, 2), uiText.ShadowColor, 0, Vector2.Zero, uiText.Scale, SpriteEffects.None, 0);
                    }

                    frameContext.SpriteBatch.DrawString(font, displayPath, textPos, textColor, 0, Vector2.Zero, uiText.Scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
