using System.Numerics;

namespace DinoGrr.Rendering
{
    public class Camera
    {
        public Player player { get; set; }
        public Size ViewportSize { get; set; }

        public Camera(Player position, Size viewportSize)
        {
            player = position;
            ViewportSize = viewportSize;
        }

        public Rectangle GetVisibleArea()
        {
            return new Rectangle((int)player.Position.X, (int)player.Position.Y, ViewportSize.Width, ViewportSize.Height);
        }
    }

}
