using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{

    public class GravityInfluencer : Influencer
    {
        public float GravitationalConstant { get; set; }
        private float height;
        PointF gravityForce;
        public float forceMagnitude, distance;

        public GravityInfluencer(float gravitationalConstant, float height)
        {
            this.GravitationalConstant = gravitationalConstant;
            this.height = height;
        }

        public override PointF GetForce(Particle particle)
        {
            // Calcular la distancia vertical entre la particula y el punto de referencia (por ejemplo, el suelo)
            distance = height - particle.pY;

            // Calcular la fuerza de la gravedad
            forceMagnitude = (GravitationalConstant * particle.Mass * 1000) / (distance * distance); // F = G * (m1 * m2) / r^2

            // La fuerza de la gravedad siempre apunta hacia abajo, en dirección negativa en el eje Y
            gravityForce = new PointF(0, -forceMagnitude);

            return gravityForce;
        }
    }
}
