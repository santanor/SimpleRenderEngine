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
        private readonly int width;
        private readonly int height;
        private WriteableBitmap bmp;

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
                var buffer= File.ReadAllBytes(fileName);

                bmp = new WriteableBitmap(
                    width,
                    height,
                    300,
                    300,
                    PixelFormats.Bgra32,
                    null);
                Marshal.Copy(buffer, 0, bmp.BackBuffer, buffer.Length);
                internalBuffer = new byte[width * bmp.BackBufferStride];
                bmp.CopyPixels(internalBuffer, bmp.BackBufferStride, 0);

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
                return Colors.Aqua;

            // using a % operator to cycle/repeat the texture if needed
            var u = System.Math.Abs((int) (tu*width) % width);
            var v = System.Math.Abs((int) (tv*height) % height);

            var pos = ( u + v * width ) * (PixelFormats.Bgra32.BitsPerPixel/8);
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
