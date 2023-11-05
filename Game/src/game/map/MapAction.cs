using FixMath;

namespace Game {
    public enum MapActionType {
        INVALID,
        MOVE_UNITS,
        SELECTION_GATHER_RESOURCE,
    }

    public interface MapAction {
        MapActionType GetMapActionType();
        void Plonk( List<byte> data );
        void Scoop( byte[] data, ref int offset );
    }

    public class MapAction_MoveUnits : MapAction {
        public EntityId     entId;
        public int          destIndex;

        public MapActionType GetMapActionType() {
            return MapActionType.MOVE_UNITS;
        }

        public void Plonk( List<byte> data ) {
            ByteOps.PlonkEntityId( data, entId );
            ByteOps.PlonkInt( data, destIndex );
        }

        public void Scoop( byte[] data, ref int offset ) {
            entId = ByteOps.ScoopEntityId( data, ref offset );
            destIndex = ByteOps.ScoopInt( data, ref offset );
        }
    }
}
