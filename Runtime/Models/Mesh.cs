namespace Runtime.Models
{
    /// <summary>
    /// A 3D object
    /// </summary>
    public class Mesh
    {
        public Mesh( string name, int vertCount )
        {
            Name = name;
            Vertices = new Vector3[vertCount];
        }

        public string Name { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }
}
