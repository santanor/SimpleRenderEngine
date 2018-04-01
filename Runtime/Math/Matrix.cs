using System.Runtime.InteropServices;

namespace Runtime.Math
{
    /// <summary>
    /// Represents a 4x4 mathematical matrix.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Matrix
    {
        /// <summary>
        /// The size of the <see cref="Matrix"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = 4 * 4 * sizeof(float);

        /// <summary>
        /// A <see cref="Matrix"/> with all of its components set to zero.
        /// </summary>
        public static readonly Matrix Zero;

        /// <summary>
        /// The identity <see cref="Matrix"/>.
        /// </summary>
        public static readonly Matrix Identity = new Matrix {M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f};

        /// <summary>
        /// Value at row 1 column 1 of the matrix.
        /// </summary>
        public float M11;

        /// <summary>
        /// Value at row 1 column 2 of the matrix.
        /// </summary>
        public float M12;

        /// <summary>
        /// Value at row 1 column 3 of the matrix.
        /// </summary>
        public float M13;

        /// <summary>
        /// Value at row 1 column 4 of the matrix.
        /// </summary>
        public float M14;

        /// <summary>
        /// Value at row 2 column 1 of the matrix.
        /// </summary>
        public float M21;

        /// <summary>
        /// Value at row 2 column 2 of the matrix.
        /// </summary>
        public float M22;

        /// <summary>
        /// Value at row 2 column 3 of the matrix.
        /// </summary>
        public float M23;

        /// <summary>
        /// Value at row 2 column 4 of the matrix.
        /// </summary>
        public float M24;

        /// <summary>
        /// Value at row 3 column 1 of the matrix.
        /// </summary>
        public float M31;

        /// <summary>
        /// Value at row 3 column 2 of the matrix.
        /// </summary>
        public float M32;

        /// <summary>
        /// Value at row 3 column 3 of the matrix.
        /// </summary>
        public float M33;

        /// <summary>
        /// Value at row 3 column 4 of the matrix.
        /// </summary>
        public float M34;

        /// <summary>
        /// Value at row 4 column 1 of the matrix.
        /// </summary>
        public float M41;

        /// <summary>
        /// Value at row 4 column 2 of the matrix.
        /// </summary>
        public float M42;

        /// <summary>
        /// Value at row 4 column 3 of the matrix.
        /// </summary>
        public float M43;

        /// <summary>
        /// Value at row 4 column 4 of the matrix.
        /// </summary>
        public float M44;

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void Translation( ref Vector3 value, out Matrix result )
        {
            Translation(value.X, value.Y, value.Z, out result);
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for all three coordinate planes.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix Translation( Vector3 value )
        {
            Translation(ref value, out var result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="z">Z-coordinate offset.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void Translation( float x, float y, float z, out Matrix result )
        {
            result = Identity;
            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        // ReSharper disable once InconsistentNaming
        private static void LookAtLH( ref Vector3 eye, ref Vector3 target, ref Vector3 up, out Matrix result )
        {
            Vector3.Subtract(ref target, ref eye, out var zaxis);
            zaxis.Normalize();
            Vector3.Cross(ref up, ref zaxis, out var xaxis);
            xaxis.Normalize();
            Vector3.Cross(ref zaxis, ref xaxis, out var yaxis);

            result = Identity;
            result.M11 = xaxis.X;
            result.M21 = xaxis.Y;
            result.M31 = xaxis.Z;
            result.M12 = yaxis.X;
            result.M22 = yaxis.Y;
            result.M32 = yaxis.Z;
            result.M13 = zaxis.X;
            result.M23 = zaxis.Y;
            result.M33 = zaxis.Z;

            Vector3.Dot(ref xaxis, ref eye, out result.M41);
            Vector3.Dot(ref yaxis, ref eye, out result.M42);
            Vector3.Dot(ref zaxis, ref eye, out result.M43);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at matrix.</returns>
        // ReSharper disable once InconsistentNaming
        public static Matrix LookAtLH( Vector3 eye, Vector3 target, Vector3 up )
        {
            LookAtLH(ref eye, ref target, ref up, out var result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        // ReSharper disable once InconsistentNaming
        private static void PerspectiveFovRH( float fov, float aspect, float znear, float zfar, out Matrix result )
        {
            var yScale = (float) ( 1.0f / System.Math.Tan(fov * 0.5f) );
            var q = zfar / ( znear - zfar );

            result = new Matrix
            {
                M11 = yScale / aspect,
                M22 = yScale,
                M33 = q,
                M34 = -1.0f,
                M43 = q * znear
            };
        }

        /// <summary>
        /// Creates a right-handed, perspective projection matrix based on a field of view.
        /// </summary>
        /// <param name="fov">Field of view in the y direction, in radians.</param>
        /// <param name="aspect">Aspect ratio, defined as view space width divided by height.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <returns>The created projection matrix.</returns>
        // ReSharper disable once InconsistentNaming
        public static Matrix PerspectiveFovRH( float fov, float aspect, float znear, float zfar )
        {
            PerspectiveFovRH(fov, aspect, znear, zfar, out var result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        private static void RotationYawPitchRoll( float yaw, float pitch, float roll, out Matrix result )
        {
            var quaternion = new Quaternion();
            Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out quaternion);
            RotationQuaternion(ref quaternion, out result);
        }

        /// <summary>
        /// Creates a rotation matrix with a specified yaw, pitch, and roll.
        /// </summary>
        /// <param name="yaw">Yaw around the y-axis, in radians.</param>
        /// <param name="pitch">Pitch around the x-axis, in radians.</param>
        /// <param name="roll">Roll around the z-axis, in radians.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix RotationYawPitchRoll( float yaw, float pitch, float roll )
        {
            RotationYawPitchRoll(yaw, pitch, roll, out var result);
            return result;
        }

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        /// <param name="result">The created rotation matrix.</param>
        private static void RotationQuaternion( ref Quaternion rotation, out Matrix result )
        {
            var xx = rotation.X * rotation.X;
            var yy = rotation.Y * rotation.Y;
            var zz = rotation.Z * rotation.Z;
            var xy = rotation.X * rotation.Y;
            var zw = rotation.Z * rotation.W;
            var zx = rotation.Z * rotation.X;
            var yw = rotation.Y * rotation.W;
            var yz = rotation.Y * rotation.Z;
            var xw = rotation.X * rotation.W;

            result = Identity;
            result.M11 = 1.0f - 2.0f * ( yy + zz );
            result.M12 = 2.0f * ( xy + zw );
            result.M13 = 2.0f * ( zx - yw );
            result.M21 = 2.0f * ( xy - zw );
            result.M22 = 1.0f - 2.0f * ( zz + xx );
            result.M23 = 2.0f * ( yz + xw );
            result.M31 = 2.0f * ( zx + yw );
            result.M32 = 2.0f * ( yz - xw );
            result.M33 = 1.0f - 2.0f * ( yy + xx );
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <param name="result">The product of the two matrices.</param>
        private static void Multiply( ref Matrix left, ref Matrix right, out Matrix result )
        {
            var temp = new Matrix
            {
                M11 = left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 +
                      left.M14 * right.M41,
                M12 = left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 +
                      left.M14 * right.M42,
                M13 = left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 +
                      left.M14 * right.M43,
                M14 = left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 +
                      left.M14 * right.M44,
                M21 = left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 +
                      left.M24 * right.M41,
                M22 = left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 +
                      left.M24 * right.M42,
                M23 = left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 +
                      left.M24 * right.M43,
                M24 = left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 +
                      left.M24 * right.M44,
                M31 = left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 +
                      left.M34 * right.M41,
                M32 = left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 +
                      left.M34 * right.M42,
                M33 = left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 +
                      left.M34 * right.M43,
                M34 = left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 +
                      left.M34 * right.M44,
                M41 = left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 +
                      left.M44 * right.M41,
                M42 = left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 +
                      left.M44 * right.M42,
                M43 = left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 +
                      left.M44 * right.M43,
                M44 = left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 +
                      left.M44 * right.M44
            };
            result = temp;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix operator *( Matrix left, Matrix right )
        {
            Multiply(ref left, ref right, out var result);
            return result;
        }
    }
}