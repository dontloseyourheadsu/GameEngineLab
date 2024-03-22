namespace GolfIt
{
    public class Map
    {
        public char[,] map;
        public const char lightTile = 't';
        public const char darkTile = 'T';
        public const char wall = 'w';
        public const char sand = 's';
        public const char triangle = 'r';
        public List<char[,]> levels;
        private List<(int, int)> ballLevelPositions;
        private List<(int, int)> goalLevelPositions;
        public List<List<(int, int)>> obstaclePositions;
        public List<List<char>> obstacles;
        public List<List<MovingFloor>> movingFloors;
        private int cellSize;
        private Brush darkGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#7EA00E"));
        private Brush lightGreenBrush = new SolidBrush(ColorTranslator.FromHtml("#DCD964"));
        private Brush wallBrush = new SolidBrush(ColorTranslator.FromHtml("#213502"));
        private Brush sandBrush = new SolidBrush(ColorTranslator.FromHtml("#E8D8A6"));
        private int level = 0;

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
            this.cellSize = cellSize;
            this.level = level;
            if (levels is null) CreateMaps(level);
            SetMap(level);
        }

        public void SetMap(int index)
        {
            map = levels[index];
        }

        public void Render(Graphics g, int cntT)
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
            return (ballLevelPositions[level].Item1 * cellSize, ballLevelPositions[level].Item2 * cellSize);
        }

        public (int x, int y) GetGoalPosition()
        {
            return (goalLevelPositions[level].Item2 * cellSize, goalLevelPositions[level].Item1 * cellSize);
        }

        public List<(int, int)> GetObstaclePositions()
        {
            return obstaclePositions[level];
        }

        private void CreateMaps(int mapNumber)
        {
            obstaclePositions = new List<List<(int, int)>>();
            obstacles = new List<List<char>>();
            levels = new List<char[,]>();
            ballLevelPositions = new List<(int, int)>();
            goalLevelPositions = new List<(int, int)>();
            movingFloors = new List<List<MovingFloor>>();

                    //============================================ LEVEL 1

                    obstaclePositions.Add(new List<(int, int)>());
                    obstacles.Add(new List<char>());
                    movingFloors.Add(new List<MovingFloor>());
                    levels.Add(new char[40, 20]
                    {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                    });
                    ballLevelPositions.Add((17, 3));
                    goalLevelPositions.Add((16, 31));
              
                    //============================================ LEVEL 2

                    obstaclePositions.Add(new List<(int, int)>());
                    obstacles.Add(new List<char>());
                    movingFloors.Add(new List<MovingFloor>());
                    levels.Add(new char[40, 20]
                    {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                    });
                    ballLevelPositions.Add((14, 13));
                    goalLevelPositions.Add((3, 21));
                    obstaclePositions[1].Add((21, 10));
                    obstacles[1].Add('r');
                
                    //============================================ LEVEL 3

                    obstaclePositions.Add(new List<(int, int)>());
                    obstacles.Add(new List<char>());
                    movingFloors.Add(new List<MovingFloor>());
                    levels.Add(new char[40, 20]
                    {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                    });
            ballLevelPositions.Add((15, 9));
            goalLevelPositions.Add((9, 27));


            movingFloors[2].Add(new MovingFloor(new Vector(19, 7), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(21, 7), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(23, 7), Direction.Down, cellSize));

            movingFloors[2].Add(new MovingFloor(new Vector(18, 8), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(20, 8), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(22, 8), Direction.Down, cellSize));

            movingFloors[2].Add(new MovingFloor(new Vector(19, 9), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(21, 9), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(23, 9), Direction.Down, cellSize));

            movingFloors[2].Add(new MovingFloor(new Vector(18, 10), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(20, 10), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(22, 10), Direction.Down, cellSize));

            movingFloors[2].Add(new MovingFloor(new Vector(19, 11), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(21, 11), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(23, 11), Direction.Down, cellSize));

            movingFloors[2].Add(new MovingFloor(new Vector(18, 12), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(20, 12), Direction.Down, cellSize));
            movingFloors[2].Add(new MovingFloor(new Vector(22, 12), Direction.Down, cellSize));


            //============================================ LEVEL 4

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'w', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((22, 17));
            goalLevelPositions.Add((6, 25));

            //============================================ LEVEL 5

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((10, 6));
            goalLevelPositions.Add((9, 28));

            //============================================ LEVEL 6

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((7, 2));
            goalLevelPositions.Add((10, 23));

            obstaclePositions[5].Add((18, 10));
            obstacles[5].Add('r');
            obstaclePositions[5].Add((28, 10));
            obstacles[5].Add('r');

            //============================================ LEVEL 7

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((9, 15));
            goalLevelPositions.Add((4, 16));

            obstaclePositions[6].Add((18, 10));
            obstacles[6].Add('r');
            obstaclePositions[6].Add((28, 10));
            obstacles[6].Add('r');

            movingFloors[6].Add(new MovingFloor(new Vector(16, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(18, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(20, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(22, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(24, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(26, 18), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(28, 18), Direction.Up, cellSize));
            
            movingFloors[6].Add(new MovingFloor(new Vector(15, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(17, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(19, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(21, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(23, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(25, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(27, 17), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(29, 17), Direction.Up, cellSize));

            movingFloors[6].Add(new MovingFloor(new Vector(16, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(18, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(20, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(22, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(24, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(26, 16), Direction.Up, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(28, 16), Direction.Up, cellSize));

            movingFloors[6].Add(new MovingFloor(new Vector(19, 1), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(21, 1), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(23, 1), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(25, 1), Direction.Down, cellSize));

            movingFloors[6].Add(new MovingFloor(new Vector(20, 2), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(22, 2), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(24, 2), Direction.Down, cellSize));
            movingFloors[6].Add(new MovingFloor(new Vector(26, 2), Direction.Down, cellSize));

            //============================================ LEVEL 8

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 's', 's', 's', 's', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 's', 's', 's', 's', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((10, 11));
            goalLevelPositions.Add((11, 23));

            movingFloors[7].Add(new MovingFloor(new Vector(14, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(16, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(18, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(20, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(22, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(24, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(26, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(28, 18), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(30, 18), Direction.Up, cellSize));

            movingFloors[7].Add(new MovingFloor(new Vector(13, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(15, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(17, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(19, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(21, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(23, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(25, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(27, 17), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(29, 17), Direction.Up, cellSize));

            movingFloors[7].Add(new MovingFloor(new Vector(14, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(16, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(18, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(20, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(22, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(24, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(26, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(28, 16), Direction.Up, cellSize));
            movingFloors[7].Add(new MovingFloor(new Vector(30, 16), Direction.Up, cellSize));

            //============================================ LEVEL 9

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 's', 's', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 's', 's', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w' },
                { 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((4, 3));
            goalLevelPositions.Add((10, 23));

            movingFloors[8].Add(new MovingFloor(new Vector(14, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(16, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(18, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(20, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(22, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(24, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(26, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(28, 18), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(30, 18), Direction.Up, cellSize));
                         
            movingFloors[8].Add(new MovingFloor(new Vector(13, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(15, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(17, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(19, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(21, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(23, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(25, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(27, 17), Direction.Up, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(29, 17), Direction.Up, cellSize));
                                                                              
            movingFloors[8].Add(new MovingFloor(new Vector(13, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(15, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(17, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(19, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(21, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(23, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(25, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(27, 1), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(29, 1), Direction.Down, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(14, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(16, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(18, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(20, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(22, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(24, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(26, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(28, 2), Direction.Down, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(30, 2), Direction.Down, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(15, 13), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(15, 11), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(15, 9), Direction.Left, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(16, 12), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(16, 10), Direction.Left, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(17, 13), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(17, 11), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(17, 9), Direction.Left, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(28, 14), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(28, 12), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(28, 10), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(28, 8), Direction.Left, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(29, 13), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(29, 11), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(29, 9), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(29, 7), Direction.Left, cellSize));

            movingFloors[8].Add(new MovingFloor(new Vector(30, 14), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(30, 12), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(30, 10), Direction.Left, cellSize));
            movingFloors[8].Add(new MovingFloor(new Vector(30, 8), Direction.Left, cellSize));

            //============================================ LEVEL 10

            obstaclePositions.Add(new List<(int, int)>());
            obstacles.Add(new List<char>());
            movingFloors.Add(new List<MovingFloor>());
            levels.Add(new char[40, 20]
            {
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
                { 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w' },
            });
            ballLevelPositions.Add((22, 17));
            goalLevelPositions.Add((6, 25));
        }
    }
}
