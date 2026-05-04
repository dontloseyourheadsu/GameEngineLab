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

            var globalScale = theme.GlobalScale;
            var bounds = new Rectangle(
                (int)(transform.Bounds.X * globalScale),
                (int)(transform.Bounds.Y * globalScale),
                (int)(transform.Bounds.Width * globalScale),
                (int)(transform.Bounds.Height * globalScale)
            );
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
                var shadowOffset = (int)(theme.ShadowOffset * globalScale);
                var shadowRect = new Rectangle(bounds.X + shadowOffset, bounds.Y + shadowOffset, bounds.Width, bounds.Height);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(shadowRect.Center.X, shadowRect.Center.Y), new Vector2(shadowRect.Width, shadowRect.Height), theme.ShadowColor);
            }

            // Content Position (offset if pressed)
            var contentRect = bounds;
            if (isPressed && !isPanel)
            {
                var pressedOffset = (int)(theme.PressedContentOffset * globalScale);
                contentRect.X += pressedOffset;
                contentRect.Y += pressedOffset;
            }

            // Draw Background
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(contentRect.Center.X, contentRect.Center.Y), new Vector2(contentRect.Width, contentRect.Height), bgColor);

            // Draw Border
            var thickness = (int)Math.Max(2, 2 * globalScale);
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
            var textXOffset = (int)(5 * globalScale);
            if (world.TryGetComponent<UiCheckboxComponent>(entityId, out var checkbox))
            {
                var boxSize = (int)(20 * globalScale);
                var boxRect = new Rectangle(contentRect.X + (int)(5 * globalScale), contentRect.Y + (contentRect.Height - boxSize) / 2, boxSize, boxSize);
                
                // Draw Checkbox Border
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize, boxSize), theme.BorderColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize - (int)(4 * globalScale), boxSize - (int)(4 * globalScale)), theme.ShadowColor);

                // Draw Inner box if checked
                if (checkbox.Checked)
                {
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(boxRect.Center.X, boxRect.Center.Y), new Vector2(boxSize - (int)(8 * globalScale), boxSize - (int)(8 * globalScale)), theme.HighlightColor);
                }

                textXOffset = (int)(30 * globalScale);
            }

            // Draw Selector
            if (world.TryGetComponent<UiSelectorComponent>(entityId, out var sel))
            {
                // Draw selector box around the text area or a specific indicator
                var indicatorWidth = (int)(30 * globalScale);
                var indicatorRect = new Rectangle(contentRect.Right - indicatorWidth - (int)(5 * globalScale), contentRect.Y + (int)(5 * globalScale), indicatorWidth, contentRect.Height - (int)(10 * globalScale));
                
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(indicatorRect.Center.X, indicatorRect.Center.Y), new Vector2(indicatorRect.Width, indicatorRect.Height), theme.ShadowColor);
                
                // Draw Arrow Down / Up
                var arrowColor = theme.TextColor;
                var arrowSize = (int)(6 * globalScale);
                var arrowPos = new Vector2(indicatorRect.Center.X, indicatorRect.Center.Y);
                if (sel.IsOpen)
                {
                    // Arrow Up
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, arrowPos, new Vector2(arrowSize, (int)(2 * globalScale)), arrowColor);
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, arrowPos - new Vector2(0, (int)(2 * globalScale)), new Vector2(arrowSize - (int)(2 * globalScale), (int)(2 * globalScale)), arrowColor);
                }
                else
                {
                    // Arrow Down
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, arrowPos, new Vector2(arrowSize, (int)(2 * globalScale)), arrowColor);
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, arrowPos + new Vector2(0, (int)(2 * globalScale)), new Vector2(arrowSize - (int)(2 * globalScale), (int)(2 * globalScale)), arrowColor);
                }
            }

            // Draw Slider / Scrollbar
            if (world.TryGetComponent<UiSliderComponent>(entityId, out var slider))
            {
                var thumbSize = (int)(24 * globalScale);
                var trackColor = theme.ShadowColor;
                var thumbColor = theme.HighlightColor;

                // Draw Track
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(contentRect.Center.X, contentRect.Center.Y), new Vector2(contentRect.Width - (int)(4 * globalScale), contentRect.Height - (int)(4 * globalScale)), trackColor);

                if (slider.Vertical)
                {
                    var thumbY = contentRect.Y + (int)(slider.Value * (contentRect.Height - thumbSize)) + thumbSize / 2;
                    // Draw Thumb Shadow
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X + (int)(2 * globalScale), thumbY + (int)(2 * globalScale)), new Vector2(contentRect.Width, thumbSize), theme.ShadowColor);
                    // Draw Thumb
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X, thumbY), new Vector2(contentRect.Width, thumbSize), thumbColor);
                    // Thumb Top Highlight
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(contentRect.Center.X, thumbY - thumbSize/4), new Vector2(contentRect.Width, (int)(2 * globalScale)), theme.TextColor);
                }
                else
                {
                    var thumbX = contentRect.X + (int)(slider.Value * (contentRect.Width - thumbSize)) + thumbSize / 2;
                    // Draw Thumb Shadow
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX + (int)(2 * globalScale), contentRect.Center.Y + (int)(2 * globalScale)), new Vector2(thumbSize, contentRect.Height), theme.ShadowColor);
                    // Draw Thumb
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX, contentRect.Center.Y), new Vector2(thumbSize, contentRect.Height), thumbColor);
                    // Thumb Side Highlight
                    ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                        new Vector2(thumbX - thumbSize/4, contentRect.Center.Y), new Vector2((int)(2 * globalScale), contentRect.Height), theme.TextColor);
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
                        bool hasGlobalFocus = world.TryGetResource<UiFocusResource>(out var focus) && focus != null && focus.FocusedEntity == entityId;
                        displayPath = textInput.Text + (hasGlobalFocus && frameContext.GameTime.TotalGameTime.TotalSeconds % 1.0 < 0.5 ? "|" : "");
                    }
                    else if (world.TryGetComponent<UiSelectorComponent>(entityId, out var selector))
                    {
                        var option = selector.Options.Length > 0 ? selector.Options[selector.SelectedIndex] : "EMPTY";
                        displayPath = $"{uiText.Text}: {option}";
                    }

                    var textScale = uiText.Scale * globalScale;
                    var textSize = font.MeasureString(displayPath) * textScale;
                    var textPos = new Vector2(contentRect.X + textXOffset, contentRect.Y + (contentRect.Height - textSize.Y) / 2f);

                    if (uiText.CenterX) textPos.X = contentRect.X + (contentRect.Width - textSize.X) / 2f;
                    if (uiText.CenterY) textPos.Y = contentRect.Y + (contentRect.Height - textSize.Y) / 2f;

                    var textColor = uiText.Color ?? theme.TextColor;

                    if (uiText.HasShadow)
                    {
                        var shadowOffset = (int)(2 * globalScale);
                        frameContext.SpriteBatch.DrawString(font, displayPath, textPos + new Vector2(shadowOffset, shadowOffset), uiText.ShadowColor, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
                    }

                    frameContext.SpriteBatch.DrawString(font, displayPath, textPos, textColor, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
                }
            }
        }

        // Draw overlays (e.g., Selector dropdowns)
        foreach (var entityId in entities)
        {
            if (world.TryGetComponent<UiSelectorComponent>(entityId, out var sel) && sel.IsOpen)
            {
                world.TryGetComponent<UiTransformComponent>(entityId, out var transform);
                world.TryGetComponent<UiTextComponent>(entityId, out var uiText);
                
                var globalScale = theme.GlobalScale;
                var bounds = new Rectangle(
                    (int)(transform.Bounds.X * globalScale),
                    (int)(transform.Bounds.Y * globalScale),
                    (int)(transform.Bounds.Width * globalScale),
                    (int)(transform.Bounds.Height * globalScale)
                );
                
                var optionHeight = bounds.Height;
                var overlayRect = new Rectangle(bounds.X, bounds.Bottom, bounds.Width, optionHeight * sel.Options.Length);

                // Draw Overlay Background & Shadow
                var shadowOffset = (int)(theme.ShadowOffset * globalScale);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Center.X + shadowOffset, overlayRect.Center.Y + shadowOffset), new Vector2(overlayRect.Width, overlayRect.Height), theme.ShadowColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Center.X, overlayRect.Center.Y), new Vector2(overlayRect.Width, overlayRect.Height), theme.SurfaceColor);
                
                // Draw Border
                var thickness = (int)Math.Max(2, 2 * globalScale);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Center.X, overlayRect.Top), new Vector2(overlayRect.Width, thickness), theme.BorderColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Center.X, overlayRect.Bottom), new Vector2(overlayRect.Width, thickness), theme.BorderColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Left, overlayRect.Center.Y), new Vector2(thickness, overlayRect.Height), theme.BorderColor);
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                    new Vector2(overlayRect.Right, overlayRect.Center.Y), new Vector2(thickness, overlayRect.Height), theme.BorderColor);

                if (theme.Fonts.TryGetValue(uiText.FontName, out var font))
                {
                    var textScale = uiText.Scale * globalScale;
                    for (int i = 0; i < sel.Options.Length; i++)
                    {
                        var itemRect = new Rectangle(bounds.X, bounds.Bottom + i * optionHeight, bounds.Width, optionHeight);
                        bool isHovered = itemRect.Contains(frameContext.CurrentMouse.Position);
                        bool isSelected = i == sel.SelectedIndex;

                        if (isHovered)
                        {
                            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                                new Vector2(itemRect.Center.X, itemRect.Center.Y), new Vector2(itemRect.Width - (int)(4 * globalScale), itemRect.Height - (int)(4 * globalScale)), theme.HighlightColor);
                        }
                        else if (isSelected)
                        {
                            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                                new Vector2(itemRect.Center.X, itemRect.Center.Y), new Vector2(itemRect.Width - (int)(4 * globalScale), itemRect.Height - (int)(4 * globalScale)), Color.Lerp(theme.SurfaceColor, theme.HighlightColor, 0.5f));
                        }

                        var text = sel.Options[i];
                        var textSize = font.MeasureString(text) * textScale;
                        var textPos = new Vector2(itemRect.X + (int)(10 * globalScale), itemRect.Y + (itemRect.Height - textSize.Y) / 2f);
                        
                        frameContext.SpriteBatch.DrawString(font, text, textPos, theme.TextColor, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
                    }
                }
            }
        }
    }
}
