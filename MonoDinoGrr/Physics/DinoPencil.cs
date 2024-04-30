using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoDinoGrr.Rendering;
using System.Collections.Generic;

namespace MonoDinoGrr.Physics
{
    public class DinoPencil
    {
        public Particle PreviousParticle { get; set; }
        public Particle CurrentParticle { get; set; }
        public Particle PivotPoint { get; set; }
        public Polygon NewPolygon { get; set; }
        public List<Polygon> Polygons { get; set; }
        public List<FormKeeper> FormKeepers { get; set; }
        public int maxPolygonPoints = 200;
        public int polygonPoints = 0;

        Vector2 mouseG = new Vector2(0, 0);
        Vector2 previouseMousePosition;
        bool mouseDrawing = false;

        public DinoPencil()
        {
            Polygons = new List<Polygon>();
            FormKeepers = new List<FormKeeper>();
        }

        public void AddParticle(int mouseX, int mouseY, int mass)
        {
            if (polygonPoints >= maxPolygonPoints)
            {
                return;
            }
            CurrentParticle = new Particle(new Vector2(mouseX, mouseY), mass, 'p');
            NewPolygon.particles.Add(CurrentParticle);
            if (PreviousParticle != null)
            {
                NewPolygon.sticks.Add(new Stick(PreviousParticle, CurrentParticle, 0.9f));
            }
            PreviousParticle = CurrentParticle;
            polygonPoints++;
        }

        public void AddPolygon()
        {
            Polygons.Add(NewPolygon);
            FormKeepers.Add(new FormKeeper(NewPolygon, 0.5f));
            CurrentParticle = null;
            PreviousParticle = null;
            NewPolygon = null;
            PivotPoint = null;
        }

        public void RemovePolygon()
        {
            if (polygonPoints > 0)
            {
                polygonPoints -= Polygons[Polygons.Count - 1].particles.Count;
                Polygons.RemoveAt(Polygons.Count - 1);
                FormKeepers.RemoveAt(FormKeepers.Count - 1);
            }
        }

        public void Update(int width, int height, List<Polygon> worldPolygons, Camera camera)
        {
            for (int i = 0; i < Polygons.Count; i++)
            {
                Polygon? polygon = Polygons[i];
                polygon.Update(width, height);
                FormKeeper? formKeeper = FormKeepers[i];
                formKeeper.RestoreOriginalForm();

                for (int j = 0; j < worldPolygons.Count; j++)
                {
                    Polygon? worldPolygon = worldPolygons[j];
                    for(int k = 0; k < polygon.particles.Count; k++)
                    {
                        Particle? particle = polygon.particles[k];
                        particle.CheckPolygonCollision(worldPolygon);
                    }
                }
            }

            HandleMouse(camera);
        }

        private void HandleMouse(Camera camera)
        {
            previouseMousePosition = mouseG;
            MouseState mouseState = Mouse.GetState();
            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (!mouseDrawing)
                {
                    NewPolygon = new Polygon(new List<Particle>(), new List<Stick>());
                    mouseDrawing = true;
                }

                if (previouseMousePosition.X != mouseX 
                    && previouseMousePosition.Y != mouseY)
                {
                    var mass = 2;
                    var realPlacement = camera.TranslateToOrigin(new Point(mouseX, mouseY));
                    AddParticle(realPlacement.X, realPlacement.Y, mass);
                }
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                if (mouseDrawing)
                {
                    AddPolygon();
                }
                mouseDrawing = false;
            }

            mouseG = new Vector2(mouseX, mouseY);
        }
    }
}
