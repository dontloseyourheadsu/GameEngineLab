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
            return new Rectangle((int)player.CameraPosition.X, (int)player.CameraPosition.Y, ViewportSize.Width, ViewportSize.Height);
        }

        public bool IsVisible(Point position)
        {
            var visibleArea = GetVisibleArea();
            return visibleArea.Contains(position);
        }

        public Point TranslateToView(Point worldPosition)
        {
            return new Point(worldPosition.X - (int)player.CameraPosition.X, worldPosition.Y - (int)player.CameraPosition.Y);
        }
    }
}
