using FixMath;
using System.Diagnostics;
using System.Numerics;

namespace Game {

    public struct MapTileIndex {
        public int xIndex;
        public int yIndex;
    }


    public struct MapTile {
        public static int WORLD_WIDTH_UNITS = 25;
        public static int WORLD_HEIGHT_UNITS = 25;
        public static F64 WORLD_WIDTH_UNITS_FP = F64.FromInt(MapTile.WORLD_WIDTH_UNITS);
        public static F64 WORLD_HEIGHT_UNITS_FP = F64.FromInt(MapTile.WORLD_HEIGHT_UNITS);

        public Vector2Fp pos;
        public Vector2 visPos;
        public int xIndex;
        public int yIndex;
        public bool isWalkable;
    }

    public struct PlayerResources {
        public int r1;
        public int r2;
    }

    public class Map {
        public static int MAP_TILE_W_COUNT = 100;
        public static int MAP_TILE_H_COUNT = 100;

        public static F64 MAP_TILE_W_COUNT_FP = F64.FromInt(MAP_TILE_W_COUNT);
        public static F64 MAP_TILE_H_COUNT_FP = F64.FromInt(MAP_TILE_H_COUNT);

        public int turnNumber;
        public MapTile[,] tiles = new MapTile[MAP_TILE_W_COUNT, MAP_TILE_H_COUNT];
        public Entity[] entities = new Entity[1000];
        public List<EntityId> selection = new List<EntityId>(1000);

        public PlayerResources player1Resources = new PlayerResources();
        public PlayerResources player2Resources = new PlayerResources();

        public void Create() {
            for( int y = 0; y < MAP_TILE_H_COUNT; y++ ) {
                for( int x = 0; x < MAP_TILE_W_COUNT; x++ ) {
                    MapTile tile;
                    tile.xIndex = x;
                    tile.yIndex = y;
                    tile.isWalkable = true;

                    // We offset the tiles by half a tile so that the center of the map is (0, 0)
                    tile.pos = Vector2Fp.FromFloat(
                        ( (float)tile.xIndex - (float)MAP_TILE_W_COUNT / 2.0f ) * (float)MapTile.WORLD_WIDTH_UNITS,
                        ( (float)tile.yIndex - (float)MAP_TILE_H_COUNT / 2.0f ) * (float)MapTile.WORLD_HEIGHT_UNITS
                    );

                    tile.visPos = tile.pos.ToV2();
                    tiles[x, y] = tile;
                }
            }

            player1Resources.r1 = 100;
            player1Resources.r2 = 0;

            player2Resources.r1 = 100;
            player2Resources.r2 = 0;

            SpawnGeneral( Vector2Fp.FromInt( -50, 0 ), 1 );
            SpawnGeneral( Vector2Fp.FromInt( 50, 0 ), 2 );
            SpawnTownCenter( 44, 50, 1 );
            SpawnTownCenter( 54, 50, 2 );

            SpawnResource1Node( 44, 45 );
            SpawnResource1Node( 46, 45 );
            SpawnResource1Node( 48, 45 );
            SpawnResource1Node( 50, 45 );
        }

        public void Start() {
            turnNumber = 1;
        }

        public void DoTurn( MapTurn player1Turn, MapTurn player2Turn ) {
#if false
            Logger.Log( $"Running turn {turnNumber} with player 1 turn {player1Turn.turnNumber}" );
            Logger.Log( $"Running turn {turnNumber} with player 2 turn {player2Turn.turnNumber}" );
#endif


            Debug.Assert( player1Turn.turnNumber == turnNumber );
            Debug.Assert( player2Turn.turnNumber == turnNumber );
            Debug.Assert( player1Turn.checkSum == player2Turn.checkSum );

            for( int i = 0; i < player1Turn.actions.Count; i++ ) {
                player1Turn.actions[i].Apply( this );
            }

            for( int i = 0; i < player2Turn.actions.Count; i++ ) {
                player2Turn.actions[i].Apply( this );
            }

            for( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity e = entities[entityIndex];
                if( e != null ) {
                    switch( e.type ) {
                        case EntityType.GENERAL: {
                            F64 speed = F64.FromFloat(0.1f);
                            Vector2Fp dir = e.target - e.pos;
                            e.pos = e.pos + dir * speed;
                            e.visPos = Vector2.Lerp( e.visPos, e.pos.ToV2(), 0.1f );
                        }
                        break;
                        case EntityType.BUILDINGS_TOWN_CENTER: {
                        }
                        break;
                        case EntityType.RESOURCE_R1: {
                        }
                        break;
                        case EntityType.RESOURCE_R2: {
                        }
                        break;
                        default: {
                            Debug.Assert( false, "Unknown entity type for map turn" );
                        }
                        break;
                    }
                }
            }

            turnNumber++;
        }

        public long ComputeCheckSum() {
            long bigSum = turnNumber;
            for( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity e = entities[entityIndex];
                if( e != null ) {
                    switch( e.type ) {
                        case EntityType.GENERAL: {
                            bigSum += e.pos.RawX;
                            bigSum -= e.pos.RawY;
                        }
                        break;
                        case EntityType.BUILDINGS_TOWN_CENTER: {
                        }
                        break;
                        case EntityType.RESOURCE_R1: {
                        }
                        break;
                        case EntityType.RESOURCE_R2: {
                        }
                        break;
                        default: {
                            Logger.Log( "Unknown entity type for map turn in CheckSum" );
                            //Debug.Assert( false, "Unknown entity type for map turn" );
                        }
                        break;
                    }
                }
            }

            return bigSum;
        }

        public Entity? LookUpEntityFromId( EntityId id ) {
            Entity? e = entities[id.index];
            if( e != null && e.id.generation == id.generation ) {
                return e;
            }
            return null;
        }

        public bool SelectionContainsEntityType( EntityType type ) {
            for( int i = 0; i < selection.Count; i++ ) {
                Entity? e = LookUpEntityFromId( selection[i] );
                if( e != null && e.type == type ) {
                    return true;
                }
            }

            return false;
        }

        public Entity? SpawnEntity( EntityType type, EntityFlags flags, int playerNumber ) {
            for( int i = 0; i < entities.Length; i++ ) {
                if( entities[i] == null ) {
                    entities[i] = new Entity();
                    entities[i].flags = flags;
                    entities[i].id.index = i;
                    entities[i].id.generation = 1;
                    entities[i].type = type;
                    entities[i].playerNumber = playerNumber;
                    return entities[i];
                }
            }

            return null;
        }

        public Entity? SpawnGeneral( Vector2Fp pos, int playerNumber ) {
            Entity? entity = SpawnEntity( EntityType.GENERAL, EntityFlags.SELECTABLE, playerNumber );
            if( entity != null ) {
                entity.pos = pos;
                entity.visPos = pos.ToV2();
                entity.target = pos; // HACK
            }

            return entity;
        }

        public Entity? SpawnTownCenter( int baseTileX, int baseTileY, int playerNumber ) {
            Entity? entity = SpawnEntity(EntityType.BUILDINGS_TOWN_CENTER, EntityFlags.SELECTABLE, playerNumber);
            if( entity != null ) {
                entity.baseTileXIndex = baseTileX;
                entity.baseTileYIndex = baseTileY;
                entity.pos = TileIndicesToWorldPos( baseTileX, baseTileY );
                entity.visPos = entity.pos.ToV2();
                entity.widthInTiles = 2;
                entity.heightInTiles = 2;
                TileMarkAsWalkable( baseTileX, baseTileY, entity.widthInTiles, entity.heightInTiles, false );
            }

            return entity;
        }

        public Entity? SpawnResource1Node( int baseTileX, int baseTileY ) {
            Entity? entity = SpawnEntity(EntityType.RESOURCE_R1, EntityFlags.NONE, 0);
            if( entity != null ) {
                entity.baseTileXIndex = baseTileX;
                entity.baseTileYIndex = baseTileY;
                entity.pos = TileIndicesToWorldPos( baseTileX, baseTileY );
                entity.visPos = entity.pos.ToV2();
                entity.widthInTiles = 1;
                entity.heightInTiles = 1;
                TileMarkAsWalkable( baseTileX, baseTileY, entity.widthInTiles, entity.heightInTiles, false );
            }

            return entity;
        }

        public Entity? SpawnResource2Node( int baseTileX, int baseTileY ) {
            Entity? entity = SpawnEntity(EntityType.RESOURCE_R2, EntityFlags.NONE, 0);
            if( entity != null ) {
                entity.baseTileXIndex = baseTileX;
                entity.baseTileYIndex = baseTileY;
                entity.pos = TileIndicesToWorldPos( baseTileX, baseTileY );
                entity.visPos = entity.pos.ToV2();
                entity.widthInTiles = 2;
                entity.heightInTiles = 2;
                TileMarkAsWalkable( baseTileX, baseTileY, entity.widthInTiles, entity.heightInTiles, false );
            }

            return entity;
        }

        public Vector2Fp TileIndicesToWorldPos( int xIndex, int yIndex ) {
            Vector2Fp pos = Vector2Fp.FromFloat(
                ((float)xIndex - (float)MAP_TILE_W_COUNT / 2.0f) * (float)MapTile.WORLD_WIDTH_UNITS,
                ((float)yIndex - (float)MAP_TILE_H_COUNT / 2.0f) * (float)MapTile.WORLD_HEIGHT_UNITS
            );

            return pos;
        }

        public bool TileWorldPosToIndices( Vector2Fp pos, out int x, out int y ) {
            // We have to solve for tile.xIndex (The yIndex is solved the same way)
            // We have the following equation:
            // px = (tile.xIndex - MAX_MAP_TILE_W_COUNT / 2.0f) * MAP_TILE_WIDTH
            // px / MAP_TILE_WIDTH = tile.xIndex - MAX_MAP_TILE_W_COUNT / 2.0f
            // px / MAP_TILE_WIDTH + MAX_MAP_TILE_W_COUNT / 2.0f = tile.xIndex
            // Therefore:
            // tile.xIndex = px / MAP_TILE_WIDTH + MAX_MAP_TILE_W_COUNT / 2.0f

            x = -1;
            y = -1;

            F64 xIndexF = pos.X / MapTile.WORLD_WIDTH_UNITS_FP + MAP_TILE_W_COUNT_FP / F64.Two;
            F64 yIndexF = pos.Y / MapTile.WORLD_HEIGHT_UNITS_FP + MAP_TILE_H_COUNT_FP / F64.Two;

            if( xIndexF < F64.Zero || xIndexF >= MAP_TILE_W_COUNT_FP ) { return false; }
            if( yIndexF < F64.Zero || yIndexF >= MAP_TILE_H_COUNT_FP ) { return false; }

            x = F64.FloorToInt( xIndexF );
            y = F64.FloorToInt( yIndexF );

            return true;
        }

        public void TileMarkAsWalkable( int baseX, int baseY, int wCount, int hCount, bool isWalkable ) {
            Debug.Assert( baseX + wCount < MAP_TILE_W_COUNT );
            Debug.Assert( baseY + hCount < MAP_TILE_H_COUNT );

            for( int y = 0; y < hCount; y++ ) {
                for( int x = 0; x < wCount; x++ ) {
                    tiles[baseX + x, baseY + y].isWalkable = isWalkable;
                }
            }
        }

    }
}
