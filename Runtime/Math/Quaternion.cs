using System.Runtime.InteropServices;

namespace Runtime.Math
{
    /// <summary>
    /// Represents a four dimensional mathematical quaternion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Quaternion
    {
        /// <summary>
        /// A <see cref="Quaternion"/> with all of its components set to zero.
        /// </summary>
        public static readonly Quaternion Zero;

        /// <summary>
        /// A <see cref="Quaternion"/> with all of its components set to one.
        /// </summary>
        public static readonly Quaternion One = new Quaternion(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The identity <see cref="Quaternion"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The X component of the quaternion.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the quaternion.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z component of the quaternion.
        /// </summary>
        public float Z;

        /// <summary>
        /// The W component of the quaternion.
        /// </summary>
        public float W;

        public Quaternion( float x, float y, float z, float w )
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Creates a quaternion given a yaw, pitch, and roll value.
        /// </summary>
        /// <param name="yaw">The yaw of rotation.</param>
        /// <param name="pitch">The pitch of rotation.</param>
        /// <param name="roll">The roll of rotation.</param>
        /// <param name="result">When the method completes, contains the newly created quaternion.</param>
        public static void RotationYawPitchRoll( float yaw, float pitch, float roll, out Quaternion result )
        {
            var halfRoll = roll * 0.5f;
            var halfPitch = pitch * 0.5f;
            var halfYaw = yaw * 0.5f;

            var sinRoll = (float) System.Math.Sin(halfRoll);
            var cosRoll = (float) System.Math.Cos(halfRoll);
            var sinPitch = (float) System.Math.Sin(halfPitch);
            var cosPitch = (float) System.Math.Cos(halfPitch);
            var sinYaw = (float) System.Math.Sin(halfYaw);
            var cosYaw = (float) System.Math.Cos(halfYaw);

            result.X = cosYaw * sinPitch * cosRoll + sinYaw * cosPitch * sinRoll;
            result.Y = sinYaw * cosPitch * cosRoll - cosYaw * sinPitch * sinRoll;
            result.Z = cosYaw * cosPitch * sinRoll - sinYaw * sinPitch * cosRoll;
            result.W = cosYaw * cosPitch * cosRoll + sinYaw * sinPitch * sinRoll;
        }
    }
}