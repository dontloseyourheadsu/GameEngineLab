namespace GolfIt
{
    public class Map
    {
        public char[,] map;
        public const char lightTile = 't';
        public const char darkTile = 'T';
        public const char wall = 'w';
        public const char sand = 's';
        public const char ball = 'b';
        public const char goal = 'g';
        public List<char[,]> levels;
        private int cellSize;
        private Brush darkGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#7EA00E"));
        private Brush lightGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#DCD964"));
        private Brush wallBrush = new SolidBrush(ColorTranslator.FromHtml("#213502"));
        private Brush sandBrush = new SolidBrush(ColorTranslator.FromHtml("#E8D8A6"));

        public Map(int width, int height, int cellSize)
        {
            map = new char[width, height];
            this.cellSize = cellSize;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    bool isStairWall = (height - 1 - j) == i;

                    if (isStairWall)
                    {
                        map[i, j] = wall;
                    }
                    else if (i % 2 == 0)
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

            for (int i = 0; i < map.GetLength(0); i++)
            {
                map[i, 12] = wall;
            }
            for (int i = 0; i < map.GetLength(1); i++)
            {
                map[12, i] = wall;
            }
        }

        public Map(int cellSize, int level)
        {
            if (levels is null) CreateMaps();
            this.cellSize = cellSize;
            SetMap(0);
        }

        public void SetMap(int index)
        {
            map = levels[index];
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
                            g.FillRectangle(lightGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                        case darkTile:
                            g.FillRectangle(darkGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                        case wall:
                            g.FillRectangle(wallBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                        case sand:
                            g.FillRectangle(sandBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                            break;
                        case ball:
                        case goal:
                            if (i % 2 == 0)
                            {
                                if (j % 2 == 0)
                                {
                                    g.FillRectangle(lightGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                                }
                                else
                                {
                                    g.FillRectangle(darkGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                                }
                            }
                            else
                            {
                                if (j % 2 == 0)
                                {
                                    g.FillRectangle(darkGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                                }
                                else
                                {
                                    g.FillRectangle(lightGreenBrush, i * cellSize, j * cellSize, cellSize, cellSize);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public bool IsWall(int x, int y)
        {
            int gridX = x / cellSize;
            int gridY = y / cellSize;

            if (gridX < 0 || gridX >= map.GetLength(0) || gridY < 0 || gridY >= map.GetLength(1))
                return false;

            return map[gridX, gridY] == wall;
        }

        public bool IsSand(int x, int y)
        {
            int gridX = x / cellSize;
            int gridY = y / cellSize;

            if (gridX < 0 || gridX >= map.GetLength(0) || gridY < 0 || gridY >= map.GetLength(1))
                return false;

            return map[gridX, gridY] == sand;
        }

        public (int x, int y) GetBallPosition()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == ball)
                    {
                        return (i * cellSize, j * cellSize);
                    }
                }
            }
            return (0, 0);
        }

        public (int x, int y) GetGoalPosition()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == goal)
                    {
                        return (i * cellSize, j * cellSize);
                    }
                }
            }
            return (0, 0);
        }

        private void CreateMaps()
        {
            levels = new List<char[,]>();
            levels.Add(new char[40, 20]
            {
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 'b', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'g', 'T', 't' },
                { 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T' },
                { 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't' },
            });
        }
    }
}
