using System.Diagnostics;
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
        readonly float cameraMovementSpeed = 100f;
        Camera camera;
        Device device;
        int frameCount;
        Stopwatch frameWatch;
        Mesh[] meshes;

        public MainWindow()
        {
            InitializeComponent();
            StartupRuntimeEngine();
        }

        /// <summary>
        /// Creates the elements to run the engine, the camera, meshes and stuff like that
        /// </summary>
        void StartupRuntimeEngine()
        {
            camera = new Camera();

            var parser = new Parser("Shuttle\\space-shuttle-orbiter.obj");
            meshes = parser.Parse();

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
        void Render( object sender, object e )
        {
            CalculateFps();
            //Restarts the background with black
            device.Clear(0, 0, 0, 255);

            device.Render(camera, meshes);
            // Flushing the back buffer into the front buffer
            device.Present();
        }

        /// <summary>
        /// Every second displays how many frames have passed. This is a basic FPS counter
        /// </summary>
        void CalculateFps()
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
            //Manages the camera movement. Dirty, but it works
            if (e.Key == Key.W)
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y,
                                              camera.Position.Z - cameraMovementSpeed);

            if (e.Key == Key.S)
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y,
                                              camera.Position.Z + cameraMovementSpeed);
            if (e.Key == Key.A)
                camera.Position = new Vector3(camera.Position.X - cameraMovementSpeed, camera.Position.Y,
                                              camera.Position.Z);

            if (e.Key == Key.D)
                camera.Position = new Vector3(camera.Position.X + cameraMovementSpeed, camera.Position.Y,
                                              camera.Position.Z);
            if (e.Key == Key.Q)
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y + cameraMovementSpeed,
                                              camera.Position.Z);

            if (e.Key == Key.E)
                camera.Position = new Vector3(camera.Position.X, camera.Position.Y - cameraMovementSpeed,
                                              camera.Position.Z);
        }
    }
}
