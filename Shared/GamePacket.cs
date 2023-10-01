using System.Drawing;
using System.Runtime.InteropServices;

namespace Shared {
    public interface GamePacketSendable {
        public int SizeInBytes();
        public void SaveIn( int offset, byte[] data );
    }

    public enum GamePacketType : byte {
        INVALID = 0,
        MAP_START,
        MAP_TURN
    }
}