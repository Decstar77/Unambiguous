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
        public Vector2Fp    destination;

        public MapActionType GetMapActionType() {
            return MapActionType.MOVE_UNITS;
        }

        public void Plonk( List<byte> data ) {
            ByteOps.PlonkEntityId( data, entId );
            ByteOps.PlonkVector2Fp( data, destination );
        }

        public void Scoop( byte[] data, ref int offset ) {
            entId = ByteOps.ScoopEntityId( data, ref offset );
            destination = ByteOps.ScoopVector2Fp( data, ref offset );
        }
    }
}
