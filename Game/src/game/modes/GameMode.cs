namespace Game {
    public struct GameModeInitArgs {
        public int localPlayerNumber;
        public bool multiplayer;
    }

    public abstract class GameMode {
        public abstract void Init( GameModeInitArgs args );
        public abstract void UpdateTick( float dt );
        public abstract void UpdateRender( float dt );
        public abstract void Shutdown();
        public virtual void NetworkPacketReceived( byte[] data, int length ) {}
    }
}
