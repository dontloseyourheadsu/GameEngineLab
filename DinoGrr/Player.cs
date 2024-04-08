using DinoGrr.Physics;
using System.Numerics;

namespace DinoGrr
{
    public class Player
    {
        public List<Polygon> Polygons { get; set; }
        public int maxPolygonPoints = 100;
        public int polygonPoints = 0;
        public Particle PreviousParticle { get; set; }
        public Particle CurrentParticle { get; set; }
        public Polygon NewPolygon { get; set; }

        public Player()
        {
            Polygons = new List<Polygon>();
        }

        public void AddParticle(int mouseX, int mouseY, int mass)
        {
            CurrentParticle = new Particle(new Physics.Vector2(mouseX, mouseY), mass);
            NewPolygon.particles.Add(CurrentParticle);
            if (PreviousParticle != null)
            {
                NewPolygon.sticks.Add(new Stick(PreviousParticle, CurrentParticle));
            }
            PreviousParticle = CurrentParticle;
        }
        
        public void AddPolygon()
        {
            Polygons.Add(NewPolygon);
            CurrentParticle = null;
            PreviousParticle = null;
            NewPolygon = null;
        }

        public void RemovePolygon()
        {
            if (polygonPoints > 0)
            {
                Polygons.RemoveAt(Polygons.Count - 1);
                polygonPoints--;
            }
        }

        public void Update(int width, int height)
        {
            foreach (var polygon in Polygons)
            {
                polygon.Update(width, height);
            }
        }
    }
}
