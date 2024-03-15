using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PLAYGROUND
{
    public partial class MyForm : Form
    {
        Scene scene;
        Canvas canvas;
        float delta;
        int countId;
        public MyForm()
        {
            InitializeComponent();
        }

        private void Init()
        {
            canvas = new Canvas(PCT_CANVAS);
            scene = new Scene();
            scene.AddElement(new VElement());
            delta = 0;
            countId = 0;
          
            // Parámetros para el cuerpo de la jarra
            float cx = 300; // Centro X
            float cy = 300; // Centro Y
            float a = 100; // Semieje mayor
            float b = 150; // Semieje menor
            int points = 30; // Número de puntos para formar el elipse
                              // Parámetros para el rango de ángulos a omitir (parte superior del vaso)
            double angleStart = Math.PI * 5 / 4; // comienza a omitir
            double angleEnd = Math.PI * 7 / 4; // Termina de omitir

            // Agregar puntos para el cuerpo de la jarra
            for (int i = 0; i < points; i++)
            {
                double angle = (Math.PI * 2 / points) * i;

                // Solo agregar puntos si no están en el rango de la parte superior
                if (angle < angleStart || angle > angleEnd)
                {
                    float x = (float)(cx + a * Math.Cos(angle));
                    float y = (float)(cy + b * Math.Sin(angle));

                    // Agrega puntos excepto en la parte superior definida por angleStart y angleEnd
                    scene.Elements.Last().addPoint(x, y, countId++, true, 10); // Asegúrate de usar el último elemento para agregar puntos
                }
            }

            // Asegúrate de conectar el primer y último punto fuera del rango omitido para cerrar el cuerpo del vaso
           
                                                                 // Después de agregar todos los puntos...
            int numberOfPointsAdded = scene.Elements.Last().VPoints.Count; // Cantidad de puntos agregados realmente

            // Conectar cada punto con su consecutivo
            for (int i = 0; i < numberOfPointsAdded - 1; i++)
            {
                scene.Elements.Last().addPole(i, i + 1, 0); // 0 puede ser un placeholder si tu método addPole calcula la longitud basada en las posiciones de los puntos
            }
        }

        private void MyForm_SizeChanged(object sender, EventArgs e)
        {
            Init();
        }
                
        private void TIMER_Tick(object sender, EventArgs e)
        {
            canvas.Render(scene, delta);
            delta += 0.001f;
        }

        private void ADD_POINT_BTN_Click(object sender, EventArgs e)
        {
            scene.Elements[0].addPoint(50,0,countId, false, 30);
            countId++;
        }

        private void PCT_CANVAS_Click(object sender, EventArgs e)
        {

        }

        private void MyForm_Load(object sender, EventArgs e)
        {

        }
    }
}
