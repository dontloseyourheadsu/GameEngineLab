using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapas
{
    internal class Map
    {
        public char[,] level = new char[,] { };
        public int pacmanX = 1;
        public int pacmanY = 1;

        public Direction pacmanDirection = Direction.Down;

        public Map(int mapSize)
        {
            // 15 x 15 map assign to level
            // # is path
            // w is wall
            // p is pacman
            level = new char[,]
            {
                {'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'},
                {'w', '#', '#', '#', '#', '"', '#', '#', '#', '"', '#', '#', '#', '#', 'w'},
                {'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w'},
                {'w', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', 'w'},
                {'w', '#', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w'},
                {'w', '#', '#', '#', '"', '#', '#', '#', '#', '#', '"', '#', '#', '#', 'w'},
                {'w', 'w', 'w', 'w', '#', 'w', '#', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w'},
                {'w', '#', '#', '#', '#', 'w', '#', '#', '#', 'w', '#', '#', '#', '#', 'w'},
                {'w', '#', 'w', 'w', '#', '#', 'w', 'w', '#', '#', '#', 'w', 'w', '#', 'w'},
                {'w', '#', '#', 'w', '#', 'w', 'w', 'w', 'w', '#', '#', '#', 'w', '#', 'w'},
                {'w', 'w', '#', '#', '#', '#', '#', 'w', '#', '#', '#', '#', '#', 'w', 'w'},
                {'w', '#', '#', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', '#', '#', '#', 'w'},
                {'w', '#', 'w', 'w', 'w', '#', '#', '#', '#', '#', 'w', 'w', 'w', '#', 'w'},
                {'w', '#', '#', '#', '#', '#', 'w', 'w', 'w', '#', '#', '#', '#', '#', 'w'},
                {'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'}
            };

            level[pacmanX, pacmanY] = 'p';
        }

        public void UpdatePacman()
        {
            switch (pacmanDirection)
            {
                case Direction.Up:
                    if (level[pacmanX, pacmanY - 1] != 'w')
                    {
                        level[pacmanX, pacmanY] = 'e';
                        pacmanY--;
                        level[pacmanX, pacmanY] = 'p';
                    }
                    break;
                case Direction.Down:
                    if (level[pacmanX, pacmanY + 1] != 'w')
                    {
                        level[pacmanX, pacmanY] = 'e';
                        pacmanY++;
                        level[pacmanX, pacmanY] = 'p';
                    }
                    break;
                case Direction.Left:
                    if (level[pacmanX - 1, pacmanY] != 'w')
                    {
                        level[pacmanX, pacmanY] = 'e';
                        pacmanX--;
                        level[pacmanX, pacmanY] = 'p';
                    }
                    break;
                case Direction.Right:
                    if (level[pacmanX + 1, pacmanY] != 'w')
                    {
                        level[pacmanX, pacmanY] = 'e';
                        pacmanX++;
                        level[pacmanX, pacmanY] = 'p';
                    }
                    break;
            }
        }
    }
}
