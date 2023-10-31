using FixMath;
using OpenTK.Mathematics;
namespace Game {
    public static class MathExtensions {
        public static Vector2Fp ToFp( this Vector2 v) {
            return Vector2Fp.FromFloat( v.X, v.Y );
        }
    }
}
