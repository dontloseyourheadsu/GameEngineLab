namespace Pacman
{
    internal class Brick
    {
        public static void DrawBrick(Graphics g, int x, int y, int cellSize)
        {
            g.FillRectangle(Brushes.RosyBrown, x * cellSize, y * cellSize, cellSize, cellSize);
            g.FillRectangle(Brushes.SaddleBrown, (x * cellSize) + 4, y * cellSize + 4, cellSize - 8, cellSize - 8);
        }
    }
}
