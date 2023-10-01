using FixMath;
using System.Diagnostics;
using System.Numerics;

namespace Game {
    public struct EntityId {
        public int index;
        public int generation;
        public EntityId() {
            index = -1;
            generation = -1;
        }

        public static EntityId INVALID = new EntityId();
        public bool IsValid() {
            return index != -1 && generation != -1;
        }
    }

    public enum EntityType {
        INVALID = 0,
        GENERAL,
        BUILDINGS_START,
        BUILDINGS_TOWN_CENTER,
        BUILDINGS_END,
        RESOURCE_START,
        RESOURCE_R1,
        RESOURCE_R2,
        RESOURCE_END
    }

    [Flags]
    public enum EntityFlags {
        NONE = 0,
        SELECTABLE = 1 << 0,
    }

    public class Entity {
        public EntityId id;
        public EntityType type;
        public EntityFlags flags;
        public Vector2Fp pos;
        public Vector2 visPos;
        public int playerNumber;
        public bool selected;
        public int baseTileXIndex;
        public int baseTileYIndex;
        public int widthInTiles;
        public int heightInTiles;
        public int resourceAmount;

        public Vector2Fp target;

        public Bounds CaclEntityBounds() {
            switch( type ) {
                case EntityType.GENERAL: {
                    Bounds bounds = new Bounds();
                    bounds.type = BoundsType.CIRCLE;
                    bounds.circle.center = visPos;
                    bounds.circle.radius = 10;
                    return bounds;
                }
                case EntityType.RESOURCE_R1:
                case EntityType.RESOURCE_R2:
                case EntityType.BUILDINGS_TOWN_CENTER: {
                    Bounds bounds = new Bounds();
                    bounds.type = BoundsType.RECT;
                    bounds.rect.min = visPos;
                    bounds.rect.max = visPos + new Vector2( widthInTiles * MapTile.WORLD_WIDTH_UNITS, heightInTiles * MapTile.WORLD_HEIGHT_UNITS );
                    return bounds;
                }
                default: {
                    Debug.Assert( false, "Unknown entity type for bounds" );
                } break;
            }

            return new Bounds();
        }
    }
}
