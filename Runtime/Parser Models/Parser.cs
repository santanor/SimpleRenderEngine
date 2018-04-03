using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Runtime.Math;

namespace Runtime.ParserModels
{
    /// <summary>
    /// Parses a 3D class, it'll be clever enough to decide which method to use based on the file itself
    /// </summary>
    public class Parser
    {
        private string fileName;
        private Extensions extension;

        /// <summary>
        /// Creates a new parser based on the filename. Th parser system
        /// will be decided based on the file extension
        /// </summary>
        /// <param name="fileName"></param>
        public Parser( string fileName )
        {
            this.fileName = fileName;
            if (Enum.TryParse<Extensions>(fileName.Split('.').Last(), out var ext))
            {
                extension = ext;
            }
        }

        /// <summary>
        /// Parses the given file and returns an array of the meshes within it
        /// </summary>
        /// <returns></returns>
        public Mesh[] Parse()
        {
            switch (extension)
            {
                case Extensions.obj:
                    return ParseObj();
                case Extensions.babylon:
                    return ParseBabylon();
                case Extensions.fbx:
                    return ParseFbx();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Custom parser for the .obj filesystem
        /// </summary>
        /// <returns></returns>
        private Mesh[] ParseObj()
        {
            //This filesystem starts each line with a letter representing a vertex (v), normal (vn)  or a face (f)
            string[] lines = File.ReadAllLines(fileName);

            var vertices = new List<Vertex>();
            var normals = new List<Vector3>();
            var faces = new List<Face>();
            var name = "";
            //We'll loop though the lines and assign each line to a collection.
            for (var i = 0; i < lines.Length; i++)
            {
                //Divides the current line in chunks
                var lineChunks = lines[i].Split(' ');


                switch (lineChunks[0])
                {
                    case "v"://It's a vertex
                        vertices.Add( new Vertex()
                        {
                            Coordinates =
                            {
                                X = float.Parse(lineChunks[1]),
                                Y = float.Parse(lineChunks[2]),
                                Z = float.Parse(lineChunks[3])
                            }
                        });
                        break;
                    case "vn"://It's a normal
                        normals.Add( new Vector3
                        {
                            X = float.Parse(lineChunks[1]),
                            Y = float.Parse(lineChunks[2]),
                            Z = float.Parse(lineChunks[3])
                        });
                        break;
                    case "f"://It's a face
                        faces.Add(new Face//Faces are 1 based. remove 1 from every face
                        {
                            A = int.Parse(lineChunks[1].Split('/')[0])-1,
                            B = int.Parse(lineChunks[2].Split('/')[0])-1,
                            C = int.Parse(lineChunks[3].Split('/')[0])-1,
                            D = lineChunks.Length == 5 ? int.Parse(lineChunks[4].Split('/','/')[0])-1 : 0,
                        });
                        break;
                    case "o":
                        name = lineChunks[1];
                    break;
                }
            }

            //Loop through the vertices and add the normal
            for (var i = 0; i < vertices.Count; i++)
            {
                //Make sure there's a normal
                var normal = normals?[i];
                if (!normal.HasValue) break;

                //Save the variable, struct stuff....
                var vert =  vertices[i];
                vert.Normal = new Vector3()
                {
                    X = normal.Value.X,
                    Y = normal.Value.Y,
                    Z = normal.Value.Z
                };
                vertices[i] = vert;
            }

            var mesh = new Mesh
            {
                Name = name,
                Vertices = vertices.ToArray(),
                Faces = faces.ToArray()
            };

            return new []{mesh};
        }


        private Mesh[] ParseFbx()
        {
            return null;
        }

        private Mesh[] ParseBabylon()
        {
            return null;
        }
    }
}
