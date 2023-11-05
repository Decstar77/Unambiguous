namespace Game {
    public static class Util {
        public static float Lerp( float a, float b, float t ) {
            return a + ( b - a ) * t;
        }

        public static float DegToRad( float x ) {
            return x * (float)Math.PI / 180.0f;
        }

        public static float RadToDeg( float x ) {
            return x * 180.0f / (float)Math.PI;
        }

        public static int IntClamp( int x, int min, int max ) {
            if ( x < min ) {
                return min;
            }
            else if ( x > max ) {
                return max;
            }
            else {
                return x;
            }
        }
    }
}
