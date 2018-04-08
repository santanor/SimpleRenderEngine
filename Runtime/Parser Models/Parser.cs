using System;
using System.Linq;
using Runtime.Math;

namespace Runtime.ParserModels
{
    /// <summary>
    /// Parses a 3D class, it'll be clever enough to decide which method to use based on the file itself
    /// </summary>
    public class Parser
    {
        readonly Extensions extension;
        readonly string fileName;

        /// <summary>
        /// Creates a new parser based on the filename. Th parser system
        /// will be decided based on the file extension
        /// </summary>
        /// <param name="fileName"></param>
        public Parser( string fileName )
        {
            this.fileName = fileName;
            if (Enum.TryParse<Extensions>(fileName.Split('.').Last(), out var ext)) extension = ext;
        }

        /// <summary>
        /// Parses the given file and returns an array of the meshes within it
        /// </summary>
        /// <returns></returns>
        public Mesh[] Parse()
        {
            IParser parser;

            //Each extension will have a different object taking care of it
            switch (extension)
            {
                case Extensions.obj:
                    parser = new ObjParser(fileName);
                    break;
                case Extensions.babylon:
                    return ParseBabylon();
                case Extensions.fbx:
                    return ParseFbx();
                default:
                    return null;
            }

            return parser?.Parse();
        }


        Mesh[] ParseFbx()
        {
            return null;
        }

        Mesh[] ParseBabylon()
        {
            return null;
        }
    }
}
