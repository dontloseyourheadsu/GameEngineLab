using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MonoDinoGrr.Physics
{
    public class FormKeeper
    {
        private Polygon targetPolygon;
        private List<Vector2> initialLocalPositions;
        private Vector2 initialCenterOfMass;
        public Vector2 Center { get; private set; }
        public float Stiffness { get; set; }

        public FormKeeper(Polygon polygon, float stiffness = 0.1f)
        {
            targetPolygon = polygon;
            initialCenterOfMass = CalculateCenterOfMass(polygon.particles);
            initialLocalPositions = polygon.particles.Select(p => p.Position - initialCenterOfMass).ToList();
            Stiffness = stiffness;
        }

        private Vector2 CalculateCenterOfMass(List<Particle> particles)
        {
            float totalMass = particles.Sum(p => p.Mass);
            Vector2 centerOfMass = new Vector2(0, 0);

            for (int i = 0; i < particles.Count; i++)
            {
                Particle? particle = particles[i];
                centerOfMass += particle.Position * particle.Mass;
            }

            Center = centerOfMass / totalMass;
            return Center;
        }

        public void RestoreOriginalForm()
        {
            Vector2 currentCenterOfMass = CalculateCenterOfMass(targetPolygon.particles);

            for (int i = 0; i < targetPolygon.particles.Count; i++)
            {
                var particle = targetPolygon.particles[i];
                if (particle.Locked) continue; 

                var desiredPosition = initialLocalPositions[i] + currentCenterOfMass;
                var currentPosition = particle.Position;

                var restoreVector = desiredPosition - currentPosition;

                particle.Position += restoreVector * Stiffness;
            }
        }
    }
}
