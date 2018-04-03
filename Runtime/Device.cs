using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Runtime.Math;
using Matrix = Runtime.Math.Matrix;

namespace Runtime
{
    /// <summary>
    /// It's kind of the core?
    /// </summary>
    public class Device
    {
        private readonly byte[] backBuffer;
        private readonly float[] depthBuffer;
        private readonly WriteableBitmap bmp;
        private readonly int renderWidth;
        private readonly int renderHeight;

        public Device( WriteableBitmap bmp )
        {
            this.bmp = bmp;
            //The back buffer is the total number of pixels and the color of each pixel (4 positions per pixel)
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
            renderWidth = bmp.PixelWidth;
            renderHeight = bmp.PixelHeight;
            depthBuffer = new float[bmp.PixelWidth * bmp.PixelHeight];
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

            // Clearing Depth Buffer
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
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
        private void PutPixel( int x, int y, float z, Color color )
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = ( x + y * renderWidth );
            var index4 = index * 4;

            if (depthBuffer[index] < z)
            {
                return; // Discard
            }

            depthBuffer[index] = z;

            backBuffer[index4] = color.B;
            backBuffer[index4 + 1] = color.G;
            backBuffer[index4 + 2] = color.R;
            backBuffer[index4 + 3] = color.A;
        }

        /// <summary>
        /// Project takes some 3D coordinates and transform them
        /// in 2D coordinates using the transformation matrix
        /// </summary>
        private Vertex Project( Vertex vertex, Matrix transMat, Matrix world )
        {
            // transforming the coordinates
            var point2D = Vector3.TransformCoordinates(vertex.Coordinates, transMat);

            var point3D = Vector3.TransformCoordinates(vertex.Coordinates, world);
            var normal3DWorld = Vector3.TransformCoordinates(vertex.Normal, world);

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2D.X * renderWidth + renderWidth / 2.0f;
            var y = -point2D.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point3D.Z),
                Normal = normal3DWorld,
                WorldCoordinates = point3D
            };
        }

        /// <summary>
        /// DrawPoint calls PutPixel but does the clipping operation before
        /// </summary>
        private void DrawPoint( Vector3 point, Color color )
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
                PutPixel((int) point.X, (int) point.Y,point.Z, color);
        }

        /// <summary>
        /// Clamps the value between min a max values
        /// </summary>
        private float Clamp( float value, float min = 0, float max = 1 )
        {
            return System.Math.Max(min, System.Math.Min(value, max));
        }

        /// <summary>
        /// Interpolating the value between 2 vertices
        /// min is the starting point, max the ending point
        /// and gradient the % between the 2 points
        /// </summary>
        private float Interpolate( float min, float max, float gradient )
        {
            return min + ( max - min ) * Clamp(gradient);
        }

        /// <summary>
        /// drawing line between 2 points from left to right
        /// papb -> pcpd
        /// pa, vb, vc, vd must then be sorted before
        /// </summary>
        private void ProcessScanLine( ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color color )
        {
            var pa = va.Coordinates;
            var pb = vb.Coordinates;
            var pc = vc.Coordinates;
            var pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == vb.Y or vc.Y == vd.Y, gradient is forced to 1
            var gradient1 = System.Math.Abs(pa.Y - pb.Y) > float.MinValue ? (data.CurrentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = System.Math.Abs(pc.Y - pd.Y) > float.MinValue ? (data.CurrentY - pc.Y) / (pd.Y - pc.Y) : 1;

            var sx = (int)Interpolate(pa.X, pb.X, gradient1);
            var ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            var z1 = Interpolate(pa.Z, pb.Z, gradient1);
            var z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.Ndotla, data.Ndotlb, gradient1);
            var enl = Interpolate(data.Ndotlc, data.Ndotld, gradient2);

            // drawing a line from left (sx) to right (ex)
            for (var x = sx; x < ex; x++)
            {
                var gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                // changing the color value using the cosine of the angle
                // between the light vector and the normal vector
                DrawPoint(new Vector3(x, data.CurrentY, z), color * ndotl);
            }
        }

        /// <summary>
        /// Compute the cosine of the angle between the light vector and the normal vector
        /// Returns a value between 0 and 1
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="normal"></param>
        /// <param name="lightPosition"></param>
        /// <returns></returns>
        private float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return System.Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        private void DrawTriangle( Vertex v1, Vertex v2, Vertex v3, Color color )
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            // Light position
            var lightPos = new Vector3(0, 10, 10);
            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            var nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            var nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            var nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            var data = new ScanLineData();

            // computing lines' directions
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // --
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < p2.Y)
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl2;
                        ProcessScanLine(data, v1, v3, v1, v2, color);
                    }
                    else
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl2;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v1, v3, v2, v3, color);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       --
            //      - -
            //     -  -
            // P2 -   -
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;

                    if (y < p2.Y)
                    {
                        data.Ndotla = nl1;
                        data.Ndotlb = nl2;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v1, v2, v1, v3, color);
                    }
                    else
                    {
                        data.Ndotla = nl2;
                        data.Ndotlb = nl3;
                        data.Ndotlc = nl1;
                        data.Ndotld = nl3;
                        ProcessScanLine(data, v2, v3, v1, v3, color);
                    }
                }
            }
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

            for (var i = 0; i < meshes.Length; i++)
            {
                var mesh = meshes[i];
                // Beware to apply rotation before translation
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y,
                                                              mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                //Iterate over the faces, get the vertices of each face and draw the lines between them
                Parallel.For(0, mesh.Faces.Length, ( j ) =>
                {
                    var pixelA = Project(mesh.Vertices[mesh.Faces[j].A], transformMatrix, worldMatrix);
                    var pixelB = Project(mesh.Vertices[mesh.Faces[j].B], transformMatrix,worldMatrix);
                    var pixelC = Project(mesh.Vertices[mesh.Faces[j].C], transformMatrix,worldMatrix);

                    var color = 0.25f + j % mesh.Faces.Length * 0.75f / mesh.Faces.Length;
                    DrawTriangle(pixelA, pixelB, pixelC, new Color
                    {
                        ScB = color,
                        ScG = color,
                        ScR = color,
                        ScA = 1f
                    });
                });
            }
        }
    }
}
