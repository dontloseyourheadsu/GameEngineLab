using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{
    public class WindInfluencer : Influencer
    {
        public float WindForce { get; set; }
        private float width;
        PointF windVector;

        public WindInfluencer(float windForce, float width)
        {
            this.WindForce = windForce;
            this.width = width;
        }

        public override PointF GetForce(Particle particle)
        {
            // Calcular la fuerza del viento en función de la posición horizontal de la partícula
            float forceMagnitude = WindForce * (particle.pX / width);

            // La fuerza del viento siempre apunta en la dirección del viento, que asumiremos que es positiva en el eje X
            windVector = new PointF(forceMagnitude, 0);

            return windVector;
        }
    }


}
