using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Runtime;
using Runtime.Math;
using Runtime.ParserModels;

namespace SimpleRenderEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Camera camera;
        private Device device;
        private int frameCount;
        private Stopwatch frameWatch;
        private IList<Mesh> meshes;

        public MainWindow()
        {
            InitializeComponent();
            StartupRuntimeEngine();
        }

        /// <summary>
        /// Creates the elements to run the engine, the camera, meshes and stuff like that
        /// </summary>
        private void StartupRuntimeEngine()
        {
            camera = new Camera();

            var parser = new Parser("Shuttle.obj");
            meshes = parser.Parse();

            var texture = new Texture("tex.jpg", 1024, 1024);
            // ReSharper disable once PossibleNullReferenceException
            meshes.FirstOrDefault().Texture = texture;

            camera.Position = new Vector3(0, 0, 5000.0f);
            camera.Target = meshes[0].Position;

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

            frameWatch = new Stopwatch();

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
            CalculateFps();
            //Restarts the background with black
            device.Clear(0, 0, 0, 255);

            for (var index = 0; index < meshes.Count; index++)
            {
                var mesh = meshes[index];
                // rotating slightly the cube during each frame rendered
                mesh.Rotation = new Vector3(mesh.Rotation.X +0.02f, mesh.Rotation.Y + 0.02f, mesh.Rotation.Z);

                // Doing the various matrix operations
                device.Render(camera, mesh);
            }

            // Flushing the back buffer into the front buffer
            device.Present();
        }

        /// <summary>
        /// Every second displays how many frames have passed. This is a basic FPS counter
        /// </summary>
        private void CalculateFps()
        {
            //Start the stopwatch the first time
            if (!frameWatch.IsRunning) frameWatch.Start();
            if (frameWatch.ElapsedMilliseconds > 1000)
            {
                FrameCounter.Content = string.Empty;
                FrameCounter.Content = frameCount.ToString();
                frameWatch.Reset();
                frameCount = 0;
            }

            frameCount++;
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            //Closes the app
            if (e.Key == Key.Escape) Close();
        }
    }
}
