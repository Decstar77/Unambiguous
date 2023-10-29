using Shared;

namespace Game
{
    public class MapTurn
    {
        public int turnNumber = -1;
        public long checkSum = 0;
        public List<MapAction> actions = new List<MapAction>();
        private List<byte> serializeContainer = new List<byte>(1024);

        public byte[] SerializeToBytes( byte playerNumber ) {
            serializeContainer.Clear();
            serializeContainer.Add( (byte)GamePacketType.MAP_TURN );
            serializeContainer.Add( playerNumber );
            ByteOps.PlonkInt( serializeContainer, turnNumber );
            ByteOps.PlonkLong( serializeContainer, checkSum );
            ByteOps.PlonkInt( serializeContainer, actions.Count );
            actions.ForEach( x => {
                ByteOps.PlonkInt( serializeContainer, (int)x.GetMapActionType() );
                x.Plonk( serializeContainer );
            } );

            return serializeContainer.ToArray();
        }

        public void SerializeFromBytes( byte[] data, int offset ) {
            turnNumber = ByteOps.ScoopInt( data, ref offset );
            checkSum = ByteOps.ScoopLong( data, ref offset );
            int actionCount = ByteOps.ScoopInt( data, ref offset );
            for ( int i = 0; i < actionCount; i++ ) {
                MapActionType type = (MapActionType )ByteOps.ScoopInt(data, ref offset);
                switch ( type ) {
                    case MapActionType.SELECTION_MOVE: {
                        //MapActionSelectionMove action = new MapActionSelectionMove();
                        //action.Scoop( data, ref offset );
                        //actions.Add( action );
                    }
                    break;
                    case MapActionType.SELECTION_GATHER_RESOURCE: {
                        //MapActionSelectionGatherResource action = new MapActionSelectionGatherResource();
                        //action.Scoop( data, ref offset );
                        //actions.Add( action );
                    }
                    break;
                    default: {
                        //Debug.Assert( false, "Unknown map action type" );
                    }
                    break;
                }
            }
        }
    }
}
