using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Runtime.Models
{
    /// <summary>
    /// It's kind of the core?
    /// </summary>
    public class Device
    {
        private readonly byte[] backBuffer;
        private WriteableBitmap bmp;

        public Device( WriteableBitmap bmp )
        {
            this.bmp = bmp;
            //The back buffer is the total number of pixels and the color of each pixel (4 positions per pixel)
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
        }

        /// <summary>
        /// Clears the back buffer with the specific color
        /// </summary>
        public void Clear( byte r, byte g, byte b, byte a )
        {
            //Windows uses BGRA
            for (var i = 0; i < backBuffer.Length; i += 4)
            {
                backBuffer[i] = b;
                backBuffer[i + 1] = g;
                backBuffer[i + 2] = r;
                backBuffer[i + 3] = a;
            }
        }

        /// <summary>
        /// Sets the backBuffer to the bitmap's stream
        /// </summary>
        public void Present()
        {
            bmp.Lock();
            Marshal.Copy(backBuffer, 0, bmp.BackBuffer, backBuffer.Length);
            // Specify the area of the bitmap that changed.
            bmp.AddDirtyRect(new Int32Rect(0, 0, (int) bmp.Width, (int) bmp.Height));
            bmp.Unlock();
        }

        /// <summary>
        /// Puts a specific color in a screen coordinates
        /// </summary>
        private void PutPixel( int x, int y, Color color )
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = ( x + y * bmp.PixelWidth ) * 4;

            backBuffer[index] = color.B;
            backBuffer[index + 1] = color.G;
            backBuffer[index + 2] = color.R;
            backBuffer[index + 3] = color.A;
        }

        /// <summary>
        /// Project takes some 3D coordinates and transform them
        /// in 2D coordinates using the transformation matrix
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="transMat"></param>
        /// <returns></returns>
        public Vector2 Project( Vector3 coord, Matrix transMat )
        {
            // transforming the coordinates
            var point = Vector3.ToScreenCoordinates(coord, transMat);
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = -point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
            return new Vector2(x, y);
        }

        /// <summary>
        /// DrawPoint calls PutPixel but does the clipping operation before
        /// </summary>
        /// <param name="point"></param>
        private void DrawPoint( Vector2 point )
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
                PutPixel((int) point.X, (int) point.Y, new Color
                {
                    ScB = 1f,
                    ScG = 1f,
                    ScR = 0f,
                    ScA = 1f
                });
        }

        /// <summary>
        /// The main method of the engine that re-compute each vertex projection
        /// during each frame
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshes"></param>
        public void Render( Camera camera, params Mesh[] meshes )
        {
            // To understand this part, please read the prerequisites resources
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.Up);
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f,
                                                           (float) bmp.PixelWidth / bmp.PixelHeight,
                                                           0.01f, 1.0f);

            foreach (var mesh in meshes)
            {
                // Beware to apply rotation before translation
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y,
                                                              mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (var vertex in mesh.Vertices)
                {
                    // First, we project the 3D coordinates into the 2D space
                    var point = Project(vertex, transformMatrix);
                    // Then we can draw on screen
                    DrawPoint(point);
                }
            }
        }
    }
}
