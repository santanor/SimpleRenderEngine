using System;

namespace Runtime.Models
{
    public struct Vector3
    {
        public Vector3( float x, float y, float z )
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 One = new Vector3(1f, 1f, 1f);
        public static Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static Vector3 Back = new Vector3(0f, 0f, -1f);
        public static Vector3 Forward = new Vector3(-1f, 0f, 1f);
        public static Vector3 Up = new Vector3(0f, 1f, 0f);
        public static Vector3 Down = new Vector3(0f, -1f, 0f);
        public static Vector3 Left = new Vector3(-1f, 1f, 0f);
        public static Vector3 Right = new Vector3(1f, 0f, 0f);


        private static readonly float zeroTolerance = 1e-6f;

        /// <summary>
        /// Transforms a 3D point to 2D coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="transformationMatrix"></param>
        /// <returns></returns>
        public static Vector3 ToScreenCoordinates( Vector3 point, Matrix transformationMatrix )
        {
            TransformCoordinate(ref point, ref transformationMatrix, out var result);
            return result;
        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">When the method completes, contains the transformed coordinates.</param>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static void TransformCoordinate( ref Vector3 coordinate, ref Matrix transform, out Vector3 result )
        {
            var vector = new Vector4
            {
                X = coordinate.X * transform.M11 + coordinate.Y * transform.M21 +
                    coordinate.Z * transform.M31 + transform.M41,
                Y = coordinate.X * transform.M12 + coordinate.Y * transform.M22 +
                    coordinate.Z * transform.M32 + transform.M42,
                Z = coordinate.X * transform.M13 + coordinate.Y * transform.M23 +
                    coordinate.Z * transform.M33 + transform.M43,
                W = 1f / ( coordinate.X * transform.M14 + coordinate.Y * transform.M24 +
                           coordinate.Z * transform.M34 + transform.M44 )
            };

            result = new Vector3(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize()
        {
            var length = Length();
            if (Math.Abs(length) > zeroTolerance)
            {
                var inv = 1.0f / length;
                X *= inv;
                Y *= inv;
                Z *= inv;
            }
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        /// <returns>The dot product of the two vectors.</returns>
        public static float Dot( Vector3 left, Vector3 right )
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two vectors.</param>
        public static void Dot( ref Vector3 left, ref Vector3 right, out float result )
        {
            result = left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        /// <summary>
        /// Calculates the cross product of two vectors.
        /// </summary>
        /// <param name="left">First source vector.</param>
        /// <param name="right">Second source vector.</param>
        /// <param name="result">When the method completes, contains he cross product of the two vectors.</param>
        public static void Cross( ref Vector3 left, ref Vector3 right, out Vector3 result )
        {
            result = new Vector3(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X);
        }


        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="left">The first vector to subtract.</param>
        /// <param name="right">The second vector to subtract.</param>
        /// <param name="result">When the method completes, contains the difference of the two vectors.</param>
        public static void Subtract( ref Vector3 left, ref Vector3 right, out Vector3 result )
        {
            result = new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        /// <summary>
        /// Calculates the length of the vector.
        /// </summary>
        /// <returns>The length of the vector.</returns>
        /// <remarks>
        /// and speed is of the essence.
        /// </remarks>
        public float Length()
        {
            return (float) Math.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}
