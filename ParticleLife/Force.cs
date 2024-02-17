using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife
{
    internal static class Force
    {
        public static float Repel(float value)
        {
            return Math.Abs(value);
        }

        public static float Attract(float value)
        {
            return -Math.Abs(value);
        }
    }
}
