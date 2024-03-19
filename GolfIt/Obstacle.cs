using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolfIt
{
    public interface Obstacle
    {
        public Collision DetectCollision(Ball ball);
        public void Render(Graphics g, PictureBox canvas);
        public void Update(Graphics g, PictureBox canvas, Map map, int cntT);
    }
}
