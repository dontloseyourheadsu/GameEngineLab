using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PLAYGROUND
{
    public class Scene
    {

        
        public List<VElement> Elements { get; set; }

        public Scene()
        {
            Elements = new List<VElement>();
            
        }

        public void AddElement(VElement element)
        {
            Elements.Add(element);
        }
        
        public void Render(Graphics g, Size size)
        {
            for (int s = 0; s < Elements.Count; s++)
            {
                Elements[s].Render(g,size.Width,size.Height);
            }
        }
    }
}
