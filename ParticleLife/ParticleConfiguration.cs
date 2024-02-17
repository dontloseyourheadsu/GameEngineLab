using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleLife
{
    internal record ParticleConfiguration(Color receiver, Color sender, float gravity, float forceDistance, float friction);
}
