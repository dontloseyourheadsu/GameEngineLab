using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{

    public class Util
    {
        public Random Rand;
        public Image[] FIRE_IMGS, WAT_IMGS;
        List<Image> imgs;

        private static Util instance;

        public static Util Instance
        {
            get
            {
                if (Util.instance == null)
                    Util.instance = new Util();

                return Util.instance;
            }
        }

        public Util()
        {
            Rand = new Random();

            imgs = [Resource1.sfirei, Resource1.sFIREII, Resource1.sFIREIII];

            FIRE_IMGS = imgs.ToArray();

            imgs = [Resource1.WAT_I, Resource1.WAT_II, Resource1.WAT_iii];

            WAT_IMGS = imgs.ToArray();
        }

        public static Image ApplyAlpha(Image baseImage, float alpha)
        {
            // Asegúrate de que el valor de alpha esté en el rango [0, 1]
            alpha = Math.Max(0, Math.Min(1, alpha));

            // Crea un objeto ImageAttributes y establece el valor de la matriz de ajuste de color
            ImageAttributes imageAttributes = new ImageAttributes();
            float[][] colorMatrixElements = {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, alpha, 0},
                new float[] {0, 0, 0, 0, 1}
            };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Crea una nueva imagen con el mismo tamaño que la imagen base
            Image blendedImage = new Bitmap(baseImage.Width, baseImage.Height);

            // Dibuja la imagen base con el valor de transparencia aplicado
            using (Graphics g = Graphics.FromImage(blendedImage))
            {
                g.DrawImage(baseImage, new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                            0, 0, baseImage.Width, baseImage.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            return blendedImage;
        }

        public static Bitmap RotateImage(Image image, float angle)
        {
            // Crea una matriz de transformación de rotación
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, new PointF(image.Width / 2, image.Height / 2));

            // Crea un nuevo bitmap con el mismo tamaño que la imagen original
            Bitmap rotatedImage = new Bitmap(image.Width, image.Height);

            // Dibuja la imagen rotada en el nuevo bitmap
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Aplica la matriz de transformación a la imagen
                g.Transform = matrix;

                // Dibuja la imagen rotada
                g.DrawImage(image, new PointF(0, 0));
            }

            return rotatedImage;
        }
    }
}
