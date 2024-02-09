using static Pacman.Ghost;

namespace Pacman
{
    internal class Map
    {
        Graphics g;
        Pacman pacman;
        Ghost[] ghosts = new Ghost[4];
        public char[,] level = new char[,] { };
        public int cellSize;
        public int mapSize;
        public bool consumedPowerPill = false;

        public Map(int c, Graphics g, Pacman p, Ghost[] ghosts)
        {
            pacman = p;
            this.g = g;
            cellSize = 30;
            mapSize = 15;
            this.ghosts = ghosts;
            // 15 x 15 map assign to level
            // # is path
            // w is wall
            // p is pacman
            // " is power pill
            // 1 is ghost red
            // 2 is ghost pink
            // 3 is ghost blue
            // 4 is ghost orange
            // e is empty
            level = new char[,]
            {
                {'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w'},
                {'w', 'e', '#', '#', '#', '"', '#', '#', 'w', '"', '#', 'e', 'e', '1', 'w'},
                {'w', '#', '#', '#', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '2', 'w'},
                {'w', '#', 'w', 'w', 'w', '#', 'w', '#', '#', '#', '#', 'w', 'e', '3', 'w'},
                {'w', '#', '#', 'w', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '4', 'w'},
                {'w', '#', '#', '#', '#', '#', '#', '#', 'w', '#', '"', 'w', 'e', 'e', 'w'},
                {'w', 'w', 'w', 'w', '#', '#', '#', '#', 'w', 'w', '#', 'w', 'w', 'w', 'w'},
                {'#', '#', '#', '#', '"', 'w', '#', '#', '#', '#', '#', 'w', '#', '#', '#'},
                {'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', '#', '#', 'w', '#', 'w'},
                {'w', 'w', '#', 'w', 'w', '#', '#', '#', 'w', '#', '#', 'w', '#', '#', 'w'},
                {'w', 'w', '#', 'w', '#', '#', 'w', '#', '#', '#', '#', '#', '#', '#', 'w'},
                {'w', '#', '#', '#', '#', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w'},
                {'w', '#', 'w', 'w', 'w', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w'},
                {'w', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', 'w'},
                {'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w'}
            };

            level[pacman.x, pacman.y] = 'p';
            for (int i = 0; i < ghosts.Length; i++)
            {
                level[ghosts[i].x, ghosts[i].y] = (char)('1' + i);
            }
        }

        public bool PacmanWins()
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    if (level[x, y] == '#' || level[x,y] == '"')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void UpdatePacmanPoints(int x, int y)
        {
            if (level[x, y] == '#')
            {
                pacman.points += Pacman.pillPoint;
            }
            else if (level[x, y] == '"')
            {
                pacman.points += Pacman.powerPillPoint;
                consumedPowerPill = true;
                foreach (var ghost in ghosts)
                {
                    ghost.isScared = true;
                }
            }
            else if (level[x, y] is '1' or '2' or '3' or '4')
            {
                var ghostIndex = (int)level[x, y] - '1';
                ghosts[ghostIndex].KillPacman(pacman, ghosts);
            }
        }

        public void UpdatePacman()
        {
            if (pacman.y != 0 && pacman.y != mapSize - 1 && pacman.x != 0 && pacman.x != mapSize - 1)
            {
                if ((level[pacman.x, pacman.y - 1] != 'w' && pacman.nextDirection == Direction.Up) ||
                    (level[pacman.x, pacman.y + 1] != 'w' && pacman.nextDirection == Direction.Down) ||
                    (level[pacman.x - 1, pacman.y] != 'w' && pacman.nextDirection == Direction.Left) ||
                    (level[pacman.x + 1, pacman.y] != 'w' && pacman.nextDirection == Direction.Right))
                {
                    pacman.direction = pacman.nextDirection;
                }
            }

            pacman.prevX = pacman.x;
            pacman.prevY = pacman.y;
            switch (pacman.direction)
            {
                case Direction.Up:
                    if (pacman.y == 0)
                    {
                        level[pacman.x, pacman.y] = 'e';
                        UpdatePacmanPoints(pacman.x, mapSize - 1);
                        pacman.y = mapSize - 1;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    else if (level[pacman.x, pacman.y - 1] != 'w')
                    {
                        UpdatePacmanPoints(pacman.x, pacman.y - 1);
                        level[pacman.x, pacman.y] = 'e';
                        pacman.y--;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    break;
                case Direction.Down:
                    if (pacman.y == mapSize - 1)
                    {
                        level[pacman.x, pacman.y] = 'e';
                        UpdatePacmanPoints(pacman.x, 0);
                        pacman.y = 0;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    else if (level[pacman.x, pacman.y + 1] != 'w')
                    {
                        UpdatePacmanPoints(pacman.x, pacman.y + 1);
                        level[pacman.x, pacman.y] = 'e';
                        pacman.y++;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    break;
                case Direction.Left:
                    if (pacman.x == 0)
                    {
                        level[pacman.x, pacman.y] = 'e';
                        UpdatePacmanPoints(mapSize - 1, pacman.y);
                        pacman.x = mapSize - 1;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    else if (level[pacman.x - 1, pacman.y] != 'w')
                    {
                        UpdatePacmanPoints(pacman.x - 1, pacman.y);
                        level[pacman.x, pacman.y] = 'e';
                        pacman.x--;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    break;
                case Direction.Right:
                    if (pacman.x == mapSize - 1)
                    {
                        level[pacman.x, pacman.y] = 'e';
                        UpdatePacmanPoints(0, pacman.y);
                        pacman.x = 0;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    else if (level[pacman.x + 1, pacman.y] != 'w')
                    {
                        UpdatePacmanPoints(pacman.x + 1, pacman.y);
                        level[pacman.x, pacman.y] = 'e';
                        pacman.x++;
                        level[pacman.x, pacman.y] = 'p';
                    }
                    break;
            }
        }

        public void UpdateGhost(GhostType type)
        {
            int i = (int)type;

            if (ghosts[i].respawn)
            {
                return;
            }

            ghosts[i].ChooseNextDirection(level, pacman);

            ghosts[i].prevY = ghosts[i].y;
            ghosts[i].prevX = ghosts[i].x;

            switch (ghosts[i].direction)
            {
                case Direction.Up:
                    if (ghosts[i].y == 0)
                    {
                        return;
                    }
                    else if (level[ghosts[i].x, ghosts[i].y - 1] == 'p')
                    {
                        ghosts[i].KillPacman(pacman, ghosts);
                    }
                    else if (!ghosts[i].WillCollide(ghosts[i].x, ghosts[i].y - 1, level))
                    {
                        level[ghosts[i].x, ghosts[i].y] = ghosts[i].eatenBlock;
                        ghosts[i].y--;
                        ghosts[i].eatenBlock = level[ghosts[i].x, ghosts[i].y];
                        level[ghosts[i].x, ghosts[i].y] = (char)('1' + i);
                    }
                    break;
                case Direction.Down:
                    if (ghosts[i].y == mapSize - 1)
                    {
                        return;
                    }
                    else if (level[ghosts[i].x, ghosts[i].y + 1] == 'p')
                    {
                        ghosts[i].KillPacman(pacman, ghosts);
                    }
                    else if (!ghosts[i].WillCollide(ghosts[i].x, ghosts[i].y + 1, level))
                    {
                        level[ghosts[i].x, ghosts[i].y] = ghosts[i].eatenBlock;
                        ghosts[i].y++;
                        ghosts[i].eatenBlock = level[ghosts[i].x, ghosts[i].y];
                        level[ghosts[i].x, ghosts[i].y] = (char)('1' + i);
                    }
                    break;
                case Direction.Left:
                    if (ghosts[i].x == 0)
                    {
                        return;
                    }
                    else if (level[ghosts[i].x - 1, ghosts[i].y] == 'p')
                    {
                        ghosts[i].KillPacman(pacman, ghosts);
                    }
                    else if (!ghosts[i].WillCollide(ghosts[i].x - 1, ghosts[i].y, level))
                    {
                        level[ghosts[i].x, ghosts[i].y] = ghosts[i].eatenBlock;
                        ghosts[i].x--;
                        ghosts[i].eatenBlock = level[ghosts[i].x, ghosts[i].y];
                        level[ghosts[i].x, ghosts[i].y] = (char)('1' + i);
                    }
                    break;
                case Direction.Right:
                    if (ghosts[i].x == mapSize - 1)
                    {
                        return;
                    }
                    else if (level[ghosts[i].x + 1, ghosts[i].y] == 'p')
                    {
                        ghosts[i].KillPacman(pacman, ghosts);
                    }
                    else if (!ghosts[i].WillCollide(ghosts[i].x + 1, ghosts[i].y, level))
                    {
                        level[ghosts[i].x, ghosts[i].y] = ghosts[i].eatenBlock;
                        ghosts[i].x++;
                        ghosts[i].eatenBlock = level[ghosts[i].x, ghosts[i].y];
                        level[ghosts[i].x, ghosts[i].y] = (char)('1' + i);
                    }
                    break;
            }
        }

        public void DrawMap(int cntT,
            PictureBox pacmanPictureBox,
            PictureBox redPb,
            PictureBox pinkPb,
            PictureBox bluePb,
            PictureBox orangePb)
        {
            if (g is null) return;

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    switch (level[x, y])
                    {
                        case '#':
                            Pill.DrawPill(g, x, y, cellSize, cntT);
                            break;
                        case 'w':
                            Brick.DrawBrick(g, x, y, cellSize);
                            break;
                        case 'p':
                            pacman.Animate(cellSize, cntT, pacmanPictureBox, g);
                            break;
                        case '"':
                            Pill.DrawPowerPill(g, x, y, cellSize, cntT);
                            break;
                        case '1':
                            ghosts[0].Animate(cellSize, cntT, g);
                            break;
                        case '2':
                            ghosts[1].Animate(cellSize, cntT, g);
                            break;
                        case '3':
                            ghosts[2].Animate(cellSize, cntT, g);
                            break;
                        case '4':
                            ghosts[3].Animate(cellSize, cntT, g);
                            break;
                        default:
                            g.FillRectangle(Brushes.Transparent, x * cellSize, y * cellSize, cellSize, cellSize);
                            break;
                    }
                }
            }
        }
    }
}
