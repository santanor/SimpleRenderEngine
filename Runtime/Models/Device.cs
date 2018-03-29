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

            //Use the unsafe keyword. This uses pointers and black magic stuff.

            /*
             *     unsafe
    {
        // Get a pointer to the back buffer.
        int pBackBuffer = (int)writeableBitmap.BackBuffer;

        // Find the address of the pixel to draw.
        pBackBuffer += row * writeableBitmap.BackBufferStride;
        pBackBuffer += column * 4;

        // Compute the pixel's color.
        int color_data = 255 << 16; // R
        color_data |= 128 << 8;   // G
        color_data |= 255 << 0;   // B

        // Assign the color data to the pixel.
        *((int*) pBackBuffer) = color_data;
    }

    // Specify the area of the bitmap that changed.
    writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));

    // Release the back buffer and make it available for display.
    writeableBitmap.Unlock();
             */

            bmp.Unlock();
        }
    }
}
