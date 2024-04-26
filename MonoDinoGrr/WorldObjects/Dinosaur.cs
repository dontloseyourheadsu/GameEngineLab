using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoDinoGrr.Physics;
using MonoDinoGrr.WorldObjects;
using System.Collections.Generic;

namespace MonoDinoGrr
{
    public class Dinosaur
    {
        public Polygon polygon { get; set; }
        public Particle LeftLeg { get; set; }
        public Particle RightLeg { get; set; }
        public FormKeeper formKeeper { get; set; }
        public Orientation Orientation { get; set; }
        public Orientation ImageOrientation { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Player player { get; set; }
        public string image { get; set; }

        public Dinosaur(int x, int y, int width, int height, string image, Player player)
        {
            this.image = image;
            Width = width;
            Height = height;
            Orientation = Orientation.Left;
            var particles = new List<Particle>
            {
                new Particle(new Vector2(x - width / 2, y - height / 2), 2, 'd'), // top left
                new Particle(new Vector2(x + width / 2, y - height / 2), 2, 'd'), // top right
                new Particle(new Vector2(x + width / 2, y + height / 2), 2, 'd'), // bottom right
                new Particle(new Vector2(x - width / 2, y + height / 2), 2, 'd'), // bottom left
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
            this.player = player;
        }

        public void Update(int width, int height, int cntT)
        {
            polygon.Update(width, height);
            formKeeper.RestoreOriginalForm();
            if (cntT % 60 == 0)
            {
                if (player.Position.X > formKeeper.Center.X)
                {
                    JumpRight();
                }
                else if (player.Position.X < formKeeper.Center.X)
                {
                    JumpLeft();

                }

                StandUp();
            }
        }

        public void JumpRight()
        {
            if (polygon.particles[2].IsInGround && polygon.particles[3].IsInGround)
            {
                LeftLeg.Position += new Vector2(5, -10);
                RightLeg.Position += new Vector2(5, -10);
                Orientation = Orientation.Right;
            }
        }

        public void JumpLeft()
        {
            if (polygon.particles[2].IsInGround && polygon.particles[3].IsInGround)
            {
                LeftLeg.Position += new Vector2(-5, -10);
                RightLeg.Position += new Vector2(-5, -10);
                Orientation = Orientation.Left;
            }
        }

        public void StandUp()
        {
            if (polygon.particles[0].IsInGround)
            {
                polygon.particles[0].Position += new Vector2(0, -10);
            }
            if (polygon.particles[1].IsInGround)
            {
                polygon.particles[1].Position += new Vector2(0, -10);
            }
        }
    }
}
