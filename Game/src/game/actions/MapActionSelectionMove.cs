
using FixMath;

namespace Game {
    public partial class MapActionSelectionMove : MapAction {
        public EntityId[] entityIds = new EntityId[0];
        public Vector2Fp target = Vector2Fp.Zero;

        public MapActionType GetMapActionType() {
            return MapActionType.SELECTION_MOVE;
        }

        public void Plonk( List<byte> data ) {
            ByteOps.PlonkInt( data, entityIds.Length );
            for( int i = 0; i < entityIds.Length; i++ ) {
                ByteOps.PlonkEntityId( data, entityIds[i] );
            }
            ByteOps.PlonkVector2Fp( data, target );
        }

        public void Scoop( byte[] data, ref int offset ) {
            int count = ByteOps.ScoopInt( data, ref offset );
            entityIds = new EntityId[count];
            for( int i = 0; i < count; i++ ) {
                entityIds[i] = ByteOps.ScoopEntityId( data, ref offset );
            }
            target = ByteOps.ScoopVector2Fp( data, ref offset );
        }

        public void Apply( Map map ) {
            for( int i = 0; i < entityIds.Length; i++ ) {
                Entity? e = map.LookUpEntityFromId( entityIds[i] );
                if( e != null ) {
                    e.target = target;
                }
            }
        }
    }
}
