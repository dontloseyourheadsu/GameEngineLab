using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLAYGROUND
{
    public class VElement
    {

        public List<VPoint> VPoints;
        public List<VPole> VPoles;

        public VElement() 
        {
            VPoints = new List<VPoint>();
            VPoles = new List<VPole>();
        }

        public void addPoint(float x,float y,int id, bool pin, int sizeOfRadius)
        {
            VPoints.Add(new VPoint(x, y, id, pin, sizeOfRadius));

        }

        public void addPole(int i1, int i2, float length)
        {
            VPoles.Add(new VPole(VPoints[i1], VPoints[i2],length));

        }
        public void Render(System.Drawing.Graphics g, int Canvasw, int Canvash)
        {
            for(int i = 0; i < VPoints.Count; i++)
            {

                VPoints[i].Render(Canvasw, Canvash, g, VPoints);

            }

            for (int i = 0; i < VPoles.Count; i++)
            {

                VPoles[i].Render(g,Canvasw, Canvash);

            }


        }
    }
}
