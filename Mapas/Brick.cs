using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapas
{
    internal class Brick
    {
        public static void DrawBrick(Graphics g, int x, int y, int cellSize)
        {
            g.FillRectangle(Brushes.DarkBlue, x * cellSize, y * cellSize, cellSize, cellSize);
            g.FillRectangle(Brushes.DarkCyan, (x * cellSize) + 4, y * cellSize + 4, cellSize - 8, cellSize - 8);

            g.DrawLine(Pens.Black, (x * cellSize), y * cellSize, (x * cellSize) + cellSize, (y * cellSize) + cellSize - 1);

            g.DrawLine(Pens.DimGray, x * cellSize, y * cellSize, (x * cellSize) + cellSize / 2, (y * cellSize) + cellSize / 2);
            g.DrawLine(Pens.Black, x * cellSize, y * cellSize + cellSize, x * cellSize + cellSize, y * cellSize);
        }
    }
}
