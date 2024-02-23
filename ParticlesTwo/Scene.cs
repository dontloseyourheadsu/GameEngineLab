using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{
    public class Scene
    {
        public List<Emitter> Emitter { get; set; }

        public Scene()
        {
            Emitter = new List<Emitter>();
        }
    }
}
