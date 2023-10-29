using System.Drawing;
using System.Runtime.InteropServices;

namespace Shared {
    public enum GamePacketType : byte {
        INVALID = 0,
        MAP_START,
        MAP_TURN
    }
}