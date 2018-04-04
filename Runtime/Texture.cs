using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Runtime
{
    public class Texture
    {
        private byte[] internalBuffer;
        private int width;
        private int height;

        public Texture( string fileName, int width, int height )
        {
            this.width = width;
            this.height = height;
            Load(fileName);
        }

        /// <summary>
        /// Loads a texture from a file. It'll save it in the internal buffer
        /// </summary>
        /// <param name="fileName"></param>
        private void Load( string fileName )
        {
            try
            {
                internalBuffer = File.ReadAllBytes(fileName);

                var bmp = new WriteableBitmap(
                    width,
                    height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null);

                bmp.Lock();
                Marshal.Copy(internalBuffer , 0, bmp.BackBuffer, internalBuffer .Length);
                // Specify the area of the bitmap that changed.
                bmp.AddDirtyRect(new Int32Rect(0, 0, (int) bmp.Width, (int) bmp.Height));
                bmp.Unlock();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Takes the UV coordinates and returns the corresponding pixel color in the texture
        /// </summary>
        /// <param name="tu"></param>
        /// <param name="tv"></param>
        /// <returns></returns>
        public Color Map(float tu, float tv)
        {
            if (internalBuffer == null)
                return Colors.White;

            // using a % operator to cycle/repeat the texture if needed
            var u = System.Math.Abs((int) (tu*width) % width);
            var v = System.Math.Abs((int) (tv*height) % height);

            var pos = ( u + v * width );
            var b = internalBuffer[pos];
            var g = internalBuffer[pos + 1];
            var r = internalBuffer[pos + 2];
            var a = internalBuffer[pos + 3];

            return new Color
            {
                R = r,
                G = g,
                B = b,
                A = a
            };
        }
    }
}
