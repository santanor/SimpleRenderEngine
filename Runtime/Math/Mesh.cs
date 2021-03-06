﻿namespace Runtime.Math
{
    /// <summary>
    /// A 3D object
    /// </summary>
    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Texture Texture { get; set; }
    }
}