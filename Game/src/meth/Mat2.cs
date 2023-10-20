using System.Numerics;

namespace Game {
    public struct Mat2 {
        public float m00;
        public float m01;
        public float m10;
        public float m11;

        public Mat2( float m00, float m01, float m10, float m11 ) {
            this.m00 = m00;
            this.m01 = m01;
            this.m10 = m10;
            this.m11 = m11;
        }

        public static Mat2 operator *( Mat2 a, Mat2 b ) {
            return new Mat2(
                a.m00 * b.m00 + a.m01 * b.m10,
                a.m00 * b.m01 + a.m01 * b.m11,
                a.m10 * b.m00 + a.m11 * b.m10,
                a.m10 * b.m01 + a.m11 * b.m11
            );
        }

        public static Vector2 operator *( Mat2 a, Vector2 b ) {
            return new Vector2(
                a.m00 * b.X + a.m01 * b.Y,
                a.m10 * b.X + a.m11 * b.Y
            );
        }

        public static Mat2 Identity() {
            return new Mat2(
                1, 0,
                0, 1
            );
        }

        public static Mat2 Rotation( float angle ) {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Mat2(
                c, -s,
                s, c
            );
        }
    }
}
