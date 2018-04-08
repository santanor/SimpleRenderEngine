using System.Collections.Generic;
using System.IO;
using Runtime.Math;

namespace Runtime.ParserModels
{
    /// <summary>
    /// Custom parser for the .obj filesystem
    /// https://en.wikipedia.org/wiki/Wavefront_.obj_file
    /// </summary>
    public class ObjParser : IParser
    {
        readonly string[] lines;
        readonly List<Mesh> meshes;
        List<Face> faces;
        Mesh lastMesh;
        string[] material;
        List<Vector3> normals;
        List<Vector2> uvs;
        List<Vertex> vertices;


        public ObjParser( string fileName )
        {
            //This filesystem starts each line with a letter representing a vertex (v), normal (vn)  or a face (f)
            lines = File.ReadAllLines(fileName);
            meshes = new List<Mesh>();
            ResetMeshComponents();
        }


        /// <summary>
        /// Custom parser for the .obj filesystem
        /// </summary>
        /// <returns></returns>
        public Mesh[] Parse()
        {
            //Process the file line by line.
            //When a new mesh declaration found, create a mesh for it
            //Meanwhile, keep adding vertex, faces and everything to the previous created

            //We'll loop though the lines and assign each line to a collection.
            for (var i = 0; i < lines.Length; i++)
            {
                //Divides the current line in chunks
                var lineChunks = lines[i].Trim().Split(' ');

                //The start of the line indicates what to do with it
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (lineChunks[0])
                {
                    case "#": //It's a comment, skip it!
                        break;
                    case "o": //It's a new mesh.
                        CreateMesh(lineChunks[1]);
                        break;
                    case "v": //It's a vertex
                        vertices.Add(new Vertex
                        {
                            Coordinates =
                            {
                                X = float.Parse(lineChunks[1]),
                                Y = float.Parse(lineChunks[2]),
                                Z = float.Parse(lineChunks[3])
                            }
                        });
                        break;
                    case "vn": //It's a normal
                        normals.Add(new Vector3
                        {
                            X = float.Parse(lineChunks[1]),
                            Y = float.Parse(lineChunks[2]),
                            Z = float.Parse(lineChunks[3])
                        });
                        break;
                    case "f": //It's a face
                        faces.Add(new Face //Faces are 1 based. remove 1 from every face
                        {
                            A = int.Parse(lineChunks[1].Split('/')[0]) - 1,
                            B = int.Parse(lineChunks[2].Split('/')[0]) - 1,
                            C = int.Parse(lineChunks[3].Split('/')[0]) - 1,
                            D = lineChunks.Length == 5 ? int.Parse(lineChunks[4].Split('/', '/')[0]) - 1 : 0
                        });
                        break;
                    case "vt":
                        uvs.Add(new Vector2
                        {
                            X = float.Parse(lineChunks[1]),
                            Y = float.Parse(lineChunks[2])
                        });
                        break;
                    case "usemtl": //Material reference for the current mesh
                        ParseMaterial(lineChunks[1]);
                        break;
                    case "mtllib": //Material filename
                        material = File.ReadAllLines(lineChunks[1]);
                        break;
                }
            }

            //Finish the last mesh
            FinishMesh();

            return meshes.ToArray();
        }

        /// <summary>
        /// Tries to find the texture for the material selected.
        /// TODO: Dore more fancy stuff with the material
        /// </summary>
        /// <param name="newmtlName"></param>
        void ParseMaterial( string newmtlName )
        {
            var nameFound = false;
            var textureFilename = "";
            //Iterate through the material until we find the current name
            for (var i = 0; i < material.Length; i++)
            {
                if (material[i].Trim() == $"newmtl {newmtlName}")
                    nameFound = true;

                //If the name has been found, it means the next texture name
                //is the needed
                if (nameFound)
                {
                    var splitLine = material[i].Trim().Split(' ');
                    if (splitLine[0] == "map_Kd") // This is the prefix for the texture file
                    {
                        textureFilename = splitLine[1];
                        break; //Exit, we're done here
                    }
                }
            }

            lastMesh.Texture = new Texture(textureFilename);
        }

        /// <summary>
        /// Creates a new mesh, resets the lists of components
        /// </summary>
        /// <param name="name"></param>
        void CreateMesh( string name )
        {
            if (lastMesh != null) FinishMesh();

            lastMesh = new Mesh
            {
                Name = name
            };
        }

        /// <summary>
        /// Uses the lists of components to create the end result for the existing mesh
        /// It also adds it to the list
        /// </summary>
        void FinishMesh()
        {
            //Loop through the vertices and add the normal
            for (var i = 0; i < vertices.Count; i++)
            {
                //Save the variable, struct stuff....
                var vert = vertices[i];
                if (i < normals.Count) vert.Normal = normals[i];

                if (i < uvs.Count) vert.TexCoord = uvs[i];

                vertices[i] = vert;
            }

            lastMesh.Vertices = vertices.ToArray();
            lastMesh.Faces = faces.ToArray();

            meshes.Add(lastMesh);
        }

        /// <summary>
        /// Resets this aditional data structures so it can be used to map new meshes
        /// </summary>
        void ResetMeshComponents()
        {
            vertices = new List<Vertex>();
            normals = new List<Vector3>();
            faces = new List<Face>();
            uvs = new List<Vector2>();
        }
    }
}
