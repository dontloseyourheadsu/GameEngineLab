using System.Numerics;

namespace DinoGrr.Rendering
{
    public class Camera
    {
        public Player player { get; set; }
        public Size ViewportSize { get; set; }

        public Camera(Player p, Size viewportSize)
        {
            player = p;
            ViewportSize = viewportSize;
        }

        public Rectangle GetVisibleArea()
        {
            return new Rectangle((int)player.Position.X, (int)player.Position.Y, ViewportSize.Width, ViewportSize.Height);
        }

        public bool IsVisible(Point position)
        {
            var visibleArea = GetVisibleArea();
            return visibleArea.Contains(position);
        }

        public Point TranslateToView(Point worldPosition)
        {
            return new Point(worldPosition.X - (int)player.Position.X, worldPosition.Y - (int)player.Position.Y);
        }
    }
}
