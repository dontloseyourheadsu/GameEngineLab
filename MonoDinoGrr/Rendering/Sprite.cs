using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoDinoGrr.Rendering
{
    public class Sprite
    {
        public Rectangle drect, srect;

        public Sprite(Rectangle drect, Rectangle srect)
        {
            this.drect = drect;
            this.srect = srect;
        }

        public virtual void Update()
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offset, Texture2D texture)
        {
            Rectangle dest = new(
                drect.X - (int)offset.X,
                drect.Y - (int)offset.Y,
                drect.Width,
                drect.Height
                );

            spriteBatch.Draw(texture, dest, srect, Color.White);
        }
    }
}
