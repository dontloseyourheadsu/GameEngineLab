using MonoDinoGrr.Physics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoDinoGrr.Rendering;
using Microsoft.Xna.Framework.Input;

namespace MonoDinoGrr.WorldObjects
{
    public class Player
    {
        public Polygon polygon { get; set; }
        public FormKeeper formKeeper { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 CameraPosition { get; set; }
        public Orientation Orientation { get; set; }
        public Particle LeftLeg { get; set; }
        public Particle RightLeg { get; set; }
        public bool[] lifeHearts { get; set; }
        public int lifePointer { get; set; }
        public int inmunityCntT { get; set; }
        int inmunityTime = 120;
        public bool isDamaged = false;
        public Vector2 PreviousPosition { get; set; }

        public DinoPencil dinoPencil { get; set; }

        public bool isRunning { get; set; }
        public int runningCntT { get; set; }

        public Player(Vector2 position)
        {
            dinoPencil = new DinoPencil();
            Position = position;
            CameraPosition = new Vector2(Position.X - 200, Position.Y - 500);

            var height = 60;
            var width = 40;
            var x = (int)Position.X;
            var y = (int)Position.Y;

            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2, 'g'), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2, 'g'), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2, 'g'), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2, 'g'), // bottom left
            };

            var sticks = new List<Stick>
            {
                new Stick(particles[0], particles[1]), // top edge
                new Stick(particles[1], particles[2]), // right edge
                new Stick(particles[2], particles[3]), // bottom edge
                new Stick(particles[3], particles[0]), // left edge
            };

            polygon = new Polygon(particles, sticks);
            LeftLeg = particles[3];
            RightLeg = particles[2];

            formKeeper = new FormKeeper(polygon);
            lifeHearts = new bool[5] { true, true, true, true, true };
            lifePointer = 4;
        }

        public void Update(int width, int height, List<Polygon> worldPolygons, Background background, Camera camera)
        {
            PreviousPosition = new Vector2(Position.X, Position.Y);
            Position = formKeeper.Center;
            CameraPosition = new Vector2(Position.X-200, Position.Y-500);
            polygon.Update(width, height);
            formKeeper.RestoreOriginalForm();

            dinoPencil.Update(width, height, worldPolygons, camera);

            if (isDamaged && inmunityCntT < inmunityTime)
            {
                inmunityCntT++;
            }
            else
            {
                isDamaged = false;
            }

            HandleKeyBoard(background);
            CheckIfItsMoving();
        }

        private void CheckIfItsMoving()
        {
            if (Position.X > PreviousPosition.X - 0.3f && Position.X < PreviousPosition.X + 0.3f)
            {
                isRunning = false;
                PreviousPosition = new Vector2(Position.X, PreviousPosition.Y);
            }
            else
            {
                isRunning = true;
            }
        }

        public void MoveRight()
        {
            int speed = 1;
            LeftLeg.Position += new Vector2(speed, 0);
            RightLeg.Position += new Vector2(speed, 0);
            Orientation = Orientation.Right;
        }

        public void MoveLeft()
        {
            int speed = 1;
            LeftLeg.Position += new Vector2(-speed, 0);
            RightLeg.Position += new Vector2(-speed, 0);
            Orientation = Orientation.Left;
        }

        public void Jump()
        {
            if (LeftLeg.IsInGround && RightLeg.IsInGround)
            {
                LeftLeg.Position += new Vector2(0, -10);
                RightLeg.Position += new Vector2(0, -10);
            }
        }

        public void RemoveHearts()
        {
            if (lifePointer >= 0 && !isDamaged)
            {
                isDamaged = true;
                lifeHearts[lifePointer--] = false;
                inmunityCntT = 0;
            }
        }

        private void HandleKeyBoard(Background background)
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Z))
            {
                dinoPencil.RemovePolygon();
            }

            if (isDamaged)
            {
                return;
            }

            if (state.IsKeyDown(Keys.A))
            {
                MoveLeft();
                background.BackgroundMoveRight();
            }
            else if (state.IsKeyDown(Keys.D))
            {
                MoveRight();
                background.BackgroundMoveLeft();
            }
            else if (state.IsKeyDown(Keys.W))
            {
                Jump();
            }
        }
    }
}
