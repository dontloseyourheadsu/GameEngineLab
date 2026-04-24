using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.Core.Features.Ecs.Resources;

public sealed class FrameContext
{
    public FrameContext(
        GameTime gameTime,
        KeyboardState currentKeyboard,
        KeyboardState previousKeyboard,
        MouseState currentMouse,
        MouseState previousMouse,
        SpriteBatch? spriteBatch = null,
        Texture2D? debugPixel = null)
    {
        GameTime = gameTime;
        CurrentKeyboard = currentKeyboard;
        PreviousKeyboard = previousKeyboard;
        CurrentMouse = currentMouse;
        PreviousMouse = previousMouse;
        SpriteBatch = spriteBatch;
        DebugPixel = debugPixel;
    }

    public GameTime GameTime { get; }

    public KeyboardState CurrentKeyboard { get; }

    public KeyboardState PreviousKeyboard { get; }

    public MouseState CurrentMouse { get; }

    public MouseState PreviousMouse { get; }

    public SpriteBatch? SpriteBatch { get; }

    public Texture2D? DebugPixel { get; }

    public float DeltaSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;
}
