namespace GolfIt
{
    public class Scene
    {
        public Map map;
        public int cellSize = 20;
        public List<Verlet> verlets = new List<Verlet>();
        
        public Scene()
        {
            map = new Map(39, 22, cellSize);
        }

        public void Update(Graphics g, PictureBox canvas)
        {
            Render(g);
        }

        public void Render(Graphics g)
        {
            map.Render(g);
        }
    }
}
