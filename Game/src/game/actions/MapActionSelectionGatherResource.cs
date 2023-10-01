
using FixMath;

namespace Game {
    public class MapActionSelectionGatherResource : MapAction {
        public EntityId[] entityIds = new EntityId[0];
        public EntityId resourceNodeId = EntityId.INVALID;

        public MapActionType GetMapActionType() {
            return MapActionType.SELECTION_MOVE;
        }

        public void Plonk( List<byte> data ) {
            ByteOps.PlonkInt( data, entityIds.Length );
            for( int i = 0; i < entityIds.Length; i++ ) {
                ByteOps.PlonkEntityId( data, entityIds[i] );
            }
            ByteOps.PlonkEntityId( data, resourceNodeId );
        }

        public void Scoop( byte[] data, ref int offset ) {
            int count = ByteOps.ScoopInt( data, ref offset );
            entityIds = new EntityId[count];
            for( int i = 0; i < count; i++ ) {
                entityIds[i] = ByteOps.ScoopEntityId( data, ref offset );
            }
            resourceNodeId = ByteOps.ScoopEntityId( data, ref offset );
        }

        public void Apply( Map map ) {
            Entity? resource = map.LookUpEntityFromId( resourceNodeId );
            if( resource == null ) {
                return;
            }

            Vector2Fp p = resource.pos + new Vector2Fp( MapTile.WORLD_WIDTH_UNITS_FP / F64.Two, MapTile.WORLD_HEIGHT_UNITS_FP / F64.Two );

            for( int i = 0; i < entityIds.Length; i++ ) {
                Entity? e = map.LookUpEntityFromId( entityIds[i] );
                if( e != null && e.type == EntityType.GENERAL ) {
                    Vector2Fp dir = e.pos - p;
                    dir = Vector2Fp.Normalize( dir );
                    dir *= F64.FromInt( 25 );
                    e.target = p + dir;
                }
            }
        }
    }
}
