using System.Runtime.InteropServices;

namespace Pacman
{
    internal class Ghost
    {
        private Random rand = new Random();
        public int speed = 25;
        public int initialX;
        public int initialY;
        public int x;
        public int y;
        public float prevX;
        public float prevY;
        public Direction direction = Direction.None;
        private AnimationType animation = AnimationType.None;
        public bool respawn = false;
        public int respawnSpeed = 350;
        public int respawnCntT = 1;
        public bool isScared = false;
        public char eatenBlock;
        public GhostType ghostType;
        public int scaredTime = 700;

        public Ghost(int x, int y, GhostType ghostType)
        {
            this.x = x;
            this.y = y;
            this.prevX = x;
            this.prevY = y;
            this.initialX = x;
            this.initialY = y;
            this.ghostType = ghostType;
        }

        private Direction SeesPacman(Span<Direction> directions, char[,] level)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                var currentX = x;
                var currentY = y;
                switch (directions[i])
                {
                    case Direction.Up:
                        while (currentY > 0 && level[currentX, currentY - 1] != 'w')
                        {
                            if (level[currentX, currentY - 1] == 'p')
                            {
                                return Direction.Up;
                            }
                            currentY--;
                        }
                        break;
                    case Direction.Down:
                        while (currentY < level.GetLength(1) - 1 && level[currentX, currentY + 1] != 'w')
                        {
                            if (level[currentX, currentY + 1] == 'p')
                            {
                                return Direction.Down;
                            }
                            currentY++;
                        }
                        break;
                    case Direction.Left:
                        while (currentX > 0 && level[currentX - 1, currentY] != 'w')
                        {
                            if (level[currentX - 1, currentY] == 'p')
                            {
                                return Direction.Left;
                            }
                            currentX--;
                        }
                        break;
                    case Direction.Right:
                        while (currentX < level.GetLength(0) - 1 && level[currentX + 1, currentY] != 'w')
                        {
                            if (level[currentX + 1, currentY] == 'p')
                            {
                                return Direction.Right;
                            }
                            currentX++;
                        }
                        break;
                }
            }

            return Direction.None;
        }

        public bool WillCollide(int x, int y, char[,] level)
        {
            if (level[x, y] is 'w' or '1' or '2' or '3' or '4' or '7' or '8' or '9' or '0')
            {
                return true;
            }
            return false;
        }

        public void KillPacman(Pacman pacman, Ghost[] ghosts)
        {
            if (isScared)
            {
                respawn = true;
                pacman.points += Pacman.ghostPoint;
                return;
            }

            pacman.lives--;
            pacman.isDead = true;

            foreach (var ghost in ghosts)
            {
                ghost.respawn = true;
            }
        }

        public void ChooseNextDirection(char[,] level, Pacman pacman)
        {
            Span<Direction> directions = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];

            var seesPacman = SeesPacman(directions, level);
            if (seesPacman != Direction.None && !isScared)
            {
                direction = seesPacman;
                return;
            }

            Direction oppositeDirection = GetOppositeDirection(direction);
            List<Direction> possibleDirections = new List<Direction>();

            foreach (var dir in directions)
            {
                if (dir != oppositeDirection && CanMoveInDirection(dir, level))
                {
                    possibleDirections.Add(dir);
                }
            }

            if (possibleDirections.Count == 0)
            {
                possibleDirections.Add(oppositeDirection);
            }

            if (possibleDirections.Contains(direction) && rand.NextDouble() > 0.2)
            {
                return;
            }

            int index = rand.Next(possibleDirections.Count);
            direction = possibleDirections[index];
        }

        private bool CanMoveInDirection(Direction dir, char[,] level)
        {
            int checkX = x;
            int checkY = y;

            switch (dir)
            {
                case Direction.Up:
                    checkY--;
                    break;
                case Direction.Down:
                    checkY++;
                    break;
                case Direction.Left:
                    checkX--;
                    break;
                case Direction.Right:
                    checkX++;
                    break;
            }

            return checkX >= 0 && checkX < level.GetLength(0) && checkY >= 0 && checkY < level.GetLength(1) && !WillCollide(checkX, checkY, level);
        }

        private Direction GetOppositeDirection(Direction currentDirection)
        {
            return currentDirection switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None,
            };
        }

        public void Animate(int cellSize, int cntT, Graphics g)
        {
            var animationStep = cntT % speed;
            float stepSize = 1f / speed;
            float adjustmentFactor = animationStep * stepSize;

            if (respawn)
            {
                prevX = x;
                prevY = y;
            }

            float interpolatedX = prevX != x ? prevX : x;
            float interpolatedY = prevY != y ? prevY : y;

            if (prevX != x || prevY != y)
            {
                switch (direction)
                {
                    case Direction.Left:
                        interpolatedX -= adjustmentFactor;
                        break;
                    case Direction.Right:
                        interpolatedX += adjustmentFactor;
                        break;
                    case Direction.Up:
                        interpolatedY -= adjustmentFactor;
                        break;
                    case Direction.Down:
                        interpolatedY += adjustmentFactor;
                        break;
                }
            }

            interpolatedX *= cellSize;
            interpolatedY *= cellSize;
            DrawGhost(g, cellSize, interpolatedX, interpolatedY, cntT);
        }
        public void DrawGhost(Graphics g, int cellSize, float interpolatedX, float interpolatedY, float cntT)
        {
            Color outerColor, innerColor, eyeColor = Color.White, scaredColor = Color.DarkBlue;
            switch (ghostType)
            {
                case GhostType.Red:
                    outerColor = Color.FromArgb(229,115,115); 
                    innerColor = Color.FromArgb(255, 0, 0);
                    break;
                case GhostType.Pink:
                    outerColor = Color.FromArgb(255, 200, 200);
                    innerColor = Color.FromArgb(255, 192, 203);
                    break;
                case GhostType.Blue:
                    outerColor = Color.FromArgb(173, 216, 230); 
                    innerColor = Color.FromArgb(0, 0, 255); 
                    break;
                case GhostType.Orange:
                    outerColor = Color.FromArgb(255, 183, 77);
                    innerColor = Color.FromArgb(255, 165, 0);                    
                    break;
                default:
                    outerColor = Color.Gray;
                    innerColor = Color.DarkGray;
                    break;
            }

            if (isScared)
            {
                outerColor = scaredColor;
                innerColor = Color.Blue;
            }

            if (respawn)
            {
                float cyclePosition = (cntT % respawnSpeed) / (float)respawnSpeed;
                float sweepAngle = 360 * (1 - cyclePosition);

                float startAngle = 270 - (sweepAngle / 2);
                float endAngle = sweepAngle;

                using (Brush innerBrush = new SolidBrush(innerColor))
                {
                    g.FillPie(innerBrush, interpolatedX, interpolatedY, cellSize, cellSize, startAngle, endAngle);
                }
            }
            else
            {
                float minScale = 0.8f;
                float maxScale = 1.2f;
                float scale = minScale + (float)(Math.Cos(cntT * 2 * Math.PI) * 0.5 + 0.5) * (maxScale - minScale);

                float scaledCellSize = cellSize * scale;
                float xPosition = interpolatedX - (scaledCellSize - cellSize) / 2;
                float yPosition = interpolatedY - (scaledCellSize - cellSize) / 2;

                RectangleF bodyRect = new RectangleF(xPosition, yPosition, scaledCellSize, scaledCellSize);
                using (Brush outerBrush = new SolidBrush(outerColor))
                {
                    g.FillEllipse(outerBrush, bodyRect);
                }

                float eyeOffset = cellSize * 0.25f;
                PointF eyePosition = new PointF(interpolatedX + (cellSize * 0.5f) - (cellSize * 0.05f), interpolatedY + (cellSize * 0.3f));
                float eyeSize = cellSize * 0.2f; 

                switch (direction)
                {
                    case Direction.Left:
                        eyePosition.X -= eyeOffset;
                        break;
                    case Direction.Right:
                        eyePosition.X += eyeOffset;
                        break;
                    case Direction.Up:
                        eyePosition.Y -= eyeOffset;
                        break;
                    case Direction.Down:
                        eyePosition.Y += eyeOffset;
                        break;
                }

                using (Brush eyeBrush = new SolidBrush(eyeColor))
                {
                    g.FillEllipse(eyeBrush, eyePosition.X, eyePosition.Y, eyeSize, eyeSize);
                    g.FillEllipse(eyeBrush, eyePosition.X + eyeSize * 1.5f, eyePosition.Y, eyeSize, eyeSize);
                }
            }
        }

        public void ResetForNextLife(char[,] level)
        {
            level[x, y] = ' ';
            x = initialX;
            y = initialY;
            prevX = x;
            prevY = y;
            direction = Direction.None;
            animation = AnimationType.None;
            respawn = false;
            isScared = false;
            var ghostChar = ' ';
            switch (ghostType)
            {
                case GhostType.Red:
                    ghostChar = '1';
                    break;
                    case GhostType.Pink:
                    ghostChar = '2';
                    break;
                    case GhostType.Blue:
                    ghostChar = '3';
                    break;
                    case GhostType.Orange:
                    ghostChar = '4';
                    break;
            }

            level[x, y] = ghostChar;
        }

        public enum GhostType
        {
            Red = 0,
            Pink = 1,
            Blue = 2,
            Orange = 3
        }

    }

}
