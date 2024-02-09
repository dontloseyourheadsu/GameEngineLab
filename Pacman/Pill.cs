namespace Pacman
{
    internal class Pill
    {
        static Random rand;

        public static void DrawPill(Graphics g, int x, int y, int cellSize, int cntT)
        {
            if (rand is null)
            {
                rand = new Random();
            }

            if (((cntT + rand.Next(1,5)) % 5) == 0)
            {
                g.FillEllipse(Brushes.Goldenrod, x * cellSize + 8, y * cellSize + 8, cellSize - 16, cellSize - 16);
            }
            else
            {
                g.FillEllipse(Brushes.Gold, x * cellSize + 8, y * cellSize + 8, cellSize - 16, cellSize - 16);
                g.FillEllipse(Brushes.Goldenrod, x * cellSize + 10, y * cellSize + 10, cellSize - 20, cellSize - 20);
            }
        }

        public static void DrawPowerPill(Graphics g, int x, int y, int cellSize, int cntT)
        {
            if (rand is null)
            {
                rand = new Random();
            }

            if (((cntT + rand.Next(1, 10)) % 10) == 0)
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(35,200,180)), (x * cellSize), (y * cellSize), cellSize, cellSize);
                g.FillEllipse(Brushes.Yellow, x * cellSize + 3, y * cellSize + 3, cellSize - 6, cellSize - 6);
                g.FillEllipse(Brushes.Gold, x * cellSize + 5, y * cellSize + 5, cellSize - 10, cellSize - 10);
            }
            else
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(100,200,200,180)), x * cellSize, y * cellSize, cellSize, cellSize);
                g.FillEllipse(Brushes.Orange, x * cellSize + 2, y * cellSize + 2, cellSize - 4, cellSize - 4);
                g.FillEllipse(Brushes.Linen, x * cellSize + 5, y * cellSize + 5, cellSize - 10, cellSize - 10);
            }
        }
    }
}
