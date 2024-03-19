using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolfIt
{
    public interface Obstacle
    {
        public void Render(Graphics g, PictureBox canvas);
        public Collision DetectCollision(Ball ball);
    }
}
