namespace GolfIt
{
    public class Scene
    {
        public Map map;
        public int cellSize = 20;
        public List<Verlet> verlets = new List<Verlet>();
        public List<Obstacle> obstacles = new List<Obstacle>();
        
        public Scene(int level)
        {
            map = new Map(cellSize, level);
        }

        public void Update(Graphics g, PictureBox canvas, int cntT)
        {
            Render(g, cntT);
        }

        public void Render(Graphics g, int cntT)
        {
            map.Render(g, cntT);
        }
    }
}
