using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Runtime.Models;

namespace SimpleRenderEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Camera camera;
        private Device device;
        private IList<Mesh> meshes;

        public MainWindow()
        {
            InitializeComponent();
            StartupRuntimeEngine();
        }

        private void StartupRuntimeEngine()
        {
            camera = new Camera();
            meshes = new List<Mesh> {CreateCube()};

            camera.Position = new Vector3(0, 0, 10.0f);
            camera.Target = Vector3.Zero;

            // Choose the back buffer resolution here
            var bmp = new WriteableBitmap(
                (int) Img.Width,
                (int) Img.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null);

            device = new Device(bmp);
            device.Clear(0, 255, 134, 255);
            // Our XAML Image control
            Img.Source = bmp;

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += Render;
        }

        /// <summary>
        /// Rendering loop handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Render( object sender, object e )
        {
            //device.Clear(0, 255, 134, 255);

            for (var index = 0; index < meshes.Count; index++)
            {
                var mesh = meshes[index];
                // rotating slightly the cube during each frame rendered
                mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);

                // Doing the various matrix operations
                //device.Render(camera, mesh);
            }

            // Flushing the back buffer into the front buffer
            //device.Present();
        }

        /// <summary>
        /// Returns a basic unitary cube
        /// </summary>
        /// <returns></returns>
        private Mesh CreateCube()
        {
            var mesh = new Mesh("Cube", 8)
            {
                Vertices =
                {
                    [0] = new Vector3(-1, 1, 1),
                    [1] = new Vector3(1, 1, 1),
                    [2] = new Vector3(-1, -1, 1),
                    [3] = new Vector3(-1, -1, -1),
                    [4] = new Vector3(-1, 1, -1),
                    [5] = new Vector3(1, 1, -1),
                    [6] = new Vector3(1, -1, 1),
                    [7] = new Vector3(1, -1, -1)
                }
            };
            return mesh;
        }
    }
}
