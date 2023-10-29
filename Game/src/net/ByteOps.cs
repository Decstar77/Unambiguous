
//using FixMath;

namespace Game {
    public static class ByteOps {
        public static void PlonkInt( List<byte> data, int v ) {
            data.Add( (byte)( v >> 24 ) );
            data.Add( (byte)( v >> 16 ) );
            data.Add( (byte)( v >> 8 ) );
            data.Add( (byte)( v ) );
        }

        public static void PlonkLong( List<byte> data, long v ) {
            data.Add( (byte)( v >> 56 ) );
            data.Add( (byte)( v >> 48 ) );
            data.Add( (byte)( v >> 40 ) );
            data.Add( (byte)( v >> 32 ) );
            data.Add( (byte)( v >> 24 ) );
            data.Add( (byte)( v >> 16 ) );
            data.Add( (byte)( v >> 8 ) );
            data.Add( (byte)( v ) );
        }

        //public static void PlonkEntityId( List<byte> data, EntityId id ) {
        //    PlonkInt( data, id.index );
        //    PlonkInt( data, id.generation );
        //}

        //public static void PlonkVector2Fp( List<byte> data, Vector2Fp v ) {
        //    PlonkLong( data, v.RawX );
        //    PlonkLong( data, v.RawY );
        //}

        public static int ScoopInt( byte[] data, ref int offset ) {
            int v = (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | (data[offset + 3]);
            offset += 4;
            return v;
        }

        public static long ScoopLong( byte[] data, ref int offset ) {
            long v = ((long)data[offset] << 56) |
            ((long)data[offset + 1] << 48) |
            ((long)data[offset + 2] << 40) |
            ((long)data[offset + 3] << 32) |
            ((long)data[offset + 4] << 24) |
            ((long)data[offset + 5] << 16) |
            ((long)data[offset + 6] << 8) |
            ((long)data[offset + 7]);
            offset += 8;
            return v;
        }

        //public static EntityId ScoopEntityId( byte[] data, ref int offset ) {
        //    EntityId id = new EntityId();
        //    id.index = ScoopInt( data, ref offset );
        //    id.generation = ScoopInt( data, ref offset );
        //    return id;
        //}

        //public static Vector2Fp ScoopVector2Fp( byte[] data, ref int offset ) {
        //    Vector2Fp v = new Vector2Fp();
        //    v.RawX = ScoopLong( data, ref offset );
        //    v.RawY = ScoopLong( data, ref offset );
        //    return v;
        //}
    }
}
