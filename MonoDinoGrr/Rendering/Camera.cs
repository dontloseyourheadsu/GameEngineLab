using Microsoft.Xna.Framework;
using MonoDinoGrr.WorldObjects;

namespace MonoDinoGrr.Rendering
{
    public class Camera
    {
        public Player player { get; set; }
        public Rectangle ViewportSize { get; set; }
        public int Scale { get; set; }

        public Camera(Player p, Rectangle viewportSize, int scale)
        {
            player = p;
            ViewportSize = viewportSize;
            Scale = scale;
        }

        public Rectangle GetVisibleArea()
        {
            return new Rectangle((int)player.CameraPosition.X, (int)player.CameraPosition.Y+300, ViewportSize.Width, ViewportSize.Height);
        }

        public bool IsVisible(Point position)
        {
            var visibleArea = GetVisibleArea();
            return visibleArea.Contains(position);
        }

        public bool IsVisible(Vector2 position)
        {
            var visibleArea = GetVisibleArea();
            return visibleArea.Contains(position);
        }

        public Point TranslateToView(Point worldPosition)
        {
            return new Point(worldPosition.X - (int)player.CameraPosition.X, worldPosition.Y - (int)player.CameraPosition.Y + 300);
        }

        public Vector2 TranslateToView(Vector2 worldPosition)
        {
            return new Vector2(worldPosition.X - player.CameraPosition.X, worldPosition.Y - player.CameraPosition.Y + 300);
        }

        public Point TranslateToOrigin(Point viewPosition)
        {
            return new Point(viewPosition.X + (int)player.CameraPosition.X, viewPosition.Y + (int)player.CameraPosition.Y - 300);
        }

        public Vector2 TranslateToOrigin(Vector2 viewPosition)
        {
            return new Vector2(viewPosition.X + player.CameraPosition.X, viewPosition.Y + player.CameraPosition.Y - 300);
        }
    }
}

