namespace GolfIt
{
    public class Map
    {
        public char[,] map;
        public const char lightTile = 't';
        public const char darkTile = 'T';
        public char[,] level1;
        private int cellSize;
        
        public Map(int width, int height, int cellSize)
        { 
            map = new char[width, height];
            this.cellSize = cellSize;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (i % 2 == 0)
                    {
                        if (j % 2 == 0)
                        {
                            map[i, j] = lightTile;
                        }
                        else
                        {
                            map[i, j] = darkTile;
                        }
                    }
                    else
                    {
                        if (j % 2 == 0)
                        {
                            map[i, j] = darkTile;
                        }
                        else
                        {
                            map[i, j] = lightTile;
                        }
                    }
                }
            }
        }

        public Map(int cellSize)
        {
            if (level1 is null) return;
            map = level1;
            this.cellSize = cellSize;
        }

        public void Render(Graphics g)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    switch (map[i, j])
                    {
                        case lightTile:
                            g.FillRectangle(Brushes.LightGreen, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                        case darkTile:
                            g.FillRectangle(Brushes.DarkGreen, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                    }
                }
            }
        }
    }
}
