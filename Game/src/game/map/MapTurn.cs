using Shared;
using System.Linq;

namespace Game {
    public static class MapActionRegistry {
        private static Type[] typeTable = null;

        public static void Init() {
            Type action = typeof( MapAction );
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( s => s.GetTypes() )
                .Where( p => action.IsAssignableFrom( p ) )
                .ToList();

            typeTable = new Type[ types.Count ];

            foreach ( Type type in types ) {
                if ( type.IsAbstract == false ) {
                    MapAction actionInstance = ( MapAction )Activator.CreateInstance( type );
                    MapActionType actionType = actionInstance.GetMapActionType();
                    typeTable[(int)actionType] = type;
                }
            }
        }

        public static MapAction CreateAction( MapActionType type ) { 
            Type actionType = typeTable[ ( int )type ];
            return (MapAction)Activator.CreateInstance( actionType );
        }
    }

    public class MapTurn {
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
            for ( int actionIndex = 0; actionIndex < actionCount; actionIndex++ ) {
                MapActionType type = ( MapActionType )ByteOps.ScoopInt( data, ref offset );
                MapAction action = MapActionRegistry.CreateAction( type );
                action.Scoop( data, ref offset );
                actions.Add( action );
            }
        }
    }
}
