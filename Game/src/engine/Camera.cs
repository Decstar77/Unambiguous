using OpenTK.Mathematics;

namespace Game {
    public struct Camera {
        public float    minWidth;
        public float    minHeight;

        public float    maxWidth;
        public float    maxHeight;

        public float    width;
        public float    height;
        public float    zoom;

        public Vector2  pos;
    }
}
