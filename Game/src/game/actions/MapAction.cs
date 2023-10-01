namespace Game {
    public enum MapActionType {
        INVALID,
        SELECTION_MOVE,
        SELECTION_GATHER_RESOURCE,
    }

    public interface MapAction {
        MapActionType GetMapActionType();
        void Plonk( List<byte> data );
        void Scoop( byte[] data, ref int offset );
        void Apply( Map map );
    }
}
