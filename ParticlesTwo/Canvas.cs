using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ParticlesTwo
{

    public class Canvas
    {
        public Size size;
        public Bitmap bitmap;
        public float Width, Height;
        public byte[] bits;
        public Graphics g;
        public int pixelFormatSize, stride;
        public static BitmapData bitmapData;
        public int bytesPerPixel, heightInPixels, widthInBytes;
        public Rectangle rect;

        public Canvas(Size size)
        {
            this.size = size;
            Init(size.Width, size.Height);
        }

        private void Init(int width, int height)
        {
            PixelFormat format;
            GCHandle handle;
            IntPtr bitPtr;
            int padding;
            format = PixelFormat.Format32bppArgb;
            Width = width;
            Height = height;
            pixelFormatSize = Image.GetPixelFormatSize(format) / 8; // 8 bits = 1 byte
            stride = width * pixelFormatSize; // total pixels (width) times ARGB (4)
            padding = (stride % 4); // PADD = move every pixel in bytes
            stride += padding == 0 ? 0 : 4 - padding; // 4 byte multiple Alpha, Red, Green, Blue
            bits = new byte[stride * height]; // total pixels (width) times ARGB (4) times Height
            handle = GCHandle.Alloc(bits, GCHandleType.Pinned); // TO LOCK THE MEMORY
            bitPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bits, 0);
            bitmap = new Bitmap(width, height, stride, format, bitPtr);
            g = Graphics.FromImage(bitmap); // Para hacer pruebas regulares}
            rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        public void FastClear()
        {
            unsafe
            {
                try
                {
                    bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    bytesPerPixel = Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                    heightInPixels = bitmapData.Height;
                    widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                    Parallel.For(0, heightInPixels, y => // usando proceso en paralelo
                    {
                        byte* bits = PtrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            bits[x + 0] = 0;// (byte)Blue;
                            bits[x + 1] = 0;// (byte)Green;
                            bits[x + 2] = 0;// (byte)Red;
                            bits[x + 3] = 0;// (byte)Red;
                        }
                    });
                    bitmap.UnlockBits(bitmapData);
                }
                catch (Exception ex) { }
            }
            // GC.Collect();
        }

        public void Render(Scene scene, float deltaTime)
        {
            FastClear();

            for (int e = 0; e < scene.Emitter.Count; e++)
            {
                scene.Emitter[e].Render(g, deltaTime);
            }
        }
    }
}
