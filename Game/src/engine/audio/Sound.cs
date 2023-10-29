using SoLoud;

namespace Game {
    public struct Sound {
        public bool Valid { get { return soloudObject != null; } }
        
        public SoloudObject soloudObject;
    }
}
