using System;
using System.Drawing;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace Runtime
{
    public class Texture
    {
        int height;
        Color[,] textureColors;
        int width;

        public Texture( string fileName )
        {
            Load(fileName);
        }

        /// <summary>
        /// Loads a texture from a file. It'll save it in the internal buffer
        /// </summary>
        /// <param name="fileName"></param>
        void Load( string fileName )
        {
            try
            {
                var b = new Bitmap(fileName);
                textureColors = new Color[b.Width, b.Height];
                width = b.Width;
                height = b.Height;

                for (var i = 0; i < b.Width; i++)
                {
                    for (var j = 0; j < b.Height; j++)
                    {
                        var c = b.GetPixel(i, j);
                        //Manually map the color. Different color structs
                        textureColors[i, j] = new Color
                        {
                            R = c.R,
                            G = c.G,
                            B = c.B,
                            A = c.A
                        };
                    }
                }
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
        public Color Map( float tu, float tv )
        {
            // using a % operator to cycle/repeat the texture if needed
            var u = System.Math.Abs((int) ( tu * width ) % width);
            var v = System.Math.Abs((int) ( tv * height ) % height);

            //checks whether the texture is there or the coordinates are within range
            if (textureColors == null || u > textureColors.GetLength(0) || v > textureColors.GetLength(1))
                return Colors.Magenta;

            return textureColors[u, v];
        }
    }
}
