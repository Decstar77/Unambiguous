using FixMath;
using FixPointCS;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Game {

    [Flags]
    public enum IsoTileFlags : int {
        NONE = 0,
        BLOCKED = 1,
    }

    public struct FlowTile {
        public int parentIndex;
        public int xDeltaIndex;
        public int yDeltaIndex;
        public Vector2Fp pos;
        public Vector2Fp flow;
    }

    public struct IsoTile {
        public IsoTileFlags     flags;
        public int              flatIndex;
        public int              xIndex;
        public int              yIndex;
        public Vector2Fp        worldPos;
        public Vector2          worldDrawPos;
        public Vector2          worldVanishingPoint;
        public ConvexCollider   floorConvexCollider;
        public ConvexCollider   roofConvexCollider;
        public SpriteTexture    sprite;
    }

    public class IsoGrid {
        public static int TILE_WIDTH = 32;
        public static int TILE_WIDTH_HALF = TILE_WIDTH / 2;
        public static int TILE_HEIGHT = 16;
        public static int TILE_HEIGHT_HALF = TILE_HEIGHT / 2;
        public static int TILE_LEVE = 16;
        public static int TILE_LEVEL_HALF = TILE_LEVE / 2;
        public static F64 TILE_WIDTH_HALF_FP = F64.FromInt( TILE_WIDTH_HALF );
        public static F64 TILE_HEIGHT_HALF_FP = F64.FromInt( TILE_HEIGHT_HALF );
        public int widthCount;
        public int heightCount;
        public int level;
        public IsoTile[] tiles = null;
        public static Vector2 localVanishingPoint = new Vector2( 0, -9 );
        public static float IsoRotation = MathF.Atan( 1.0f / 2.0f ); // 26.565 deg
        public static ConvexBounds tileCollider = new ConvexBounds(
            new Vector2( -TILE_WIDTH_HALF, 0 - TILE_HEIGHT_HALF),
            new Vector2( 0, TILE_HEIGHT_HALF - TILE_HEIGHT_HALF),
            new Vector2( TILE_WIDTH_HALF, 0 - TILE_HEIGHT_HALF),
            new Vector2( 0, -TILE_HEIGHT_HALF - TILE_HEIGHT_HALF)
        );
        public static Vector2Fp CARDINAL_RIGHT_WORLD = Vector2Fp.Normalize( Vector2Fp.FromInt( 2, -1 ) );
        public static Vector2Fp CARDINAL_LEFT_WORLD = Vector2Fp.Normalize( Vector2Fp.FromInt( -2, 1 ) );
        public static Vector2Fp CARDINAL_UP_WORLD = Vector2Fp.Normalize( Vector2Fp.FromInt( 2, 1 ) );
        public static Vector2Fp CARDINAL_DOWN_WORLD = Vector2Fp.Normalize( Vector2Fp.FromInt( -2, -1 ) );

        public IsoGrid( int wc, int hc, int level ) {
            widthCount = wc;
            heightCount = hc;
            this.level = level;
            tiles = new IsoTile[wc * hc];
            for ( int x = 0; x < wc; x++ ) {
                for ( int y = 0; y < hc; y++ ) {
                    IsoTile tile = new IsoTile();
                    tile.xIndex = x;
                    tile.yIndex = y;
                    tile.flatIndex = PosIndexToFlatIndex( x, y );
                    tile.worldPos = MapPosToWorldPos( x, y ) + Vector2Fp.FromInt( TILE_WIDTH_HALF, -TILE_HEIGHT_HALF );
                    tile.floorConvexCollider = tileCollider.ToCollider();
                    tile.floorConvexCollider.Translate( tile.worldPos );
                    tile.roofConvexCollider = tile.floorConvexCollider.Clone();
                    tile.roofConvexCollider.Translate( Vector2Fp.FromInt( 0, TILE_HEIGHT ) );
                    tile.worldVanishingPoint = tile.worldPos.ToV2() + localVanishingPoint;
                    tiles[tile.flatIndex] = tile;
                }
            }
        }

        public void Fill( SpriteTexture sprite ) {
            for ( int i = 0; i < tiles.Length; i++ ) {
                tiles[i].sprite = sprite;
            }
        }

        public void PlaceTile( int x, int y, IsoTileFlags flags, SpriteTexture sprite ) {
            Debug.Assert( x >= 0 && x < widthCount );
            Debug.Assert( y >= 0 && y < heightCount );
            int flatIndex = PosIndexToFlatIndex( x, y );
            tiles[flatIndex].flags = flags;
            tiles[flatIndex].sprite = sprite;
        }

        public Vector2Fp MapPosToWorldPos( int x, int y ) {
            return MapPosToWorldPos( new Vector2( x, y ) );
        }

        public Vector2Fp MapPosToWorldPos( Vector2 map ) {
            map.X = map.X - level;
            map.Y = map.Y + level;

            Vector2Fp world = Vector2Fp.Zero;
            world.X = F64.FromFloat( map.X * TILE_WIDTH_HALF + map.Y * TILE_WIDTH_HALF );
            world.Y = F64.FromFloat( -map.X * TILE_HEIGHT_HALF + map.Y * TILE_HEIGHT_HALF );

            return world;
        }

        public Vector2 WorldPosToMapPos( Vector2 world ) {
            Vector2 map = Vector2.Zero;
            map.X = ( ( world.X / TILE_WIDTH_HALF - world.Y / TILE_HEIGHT_HALF ) / 2 ) + level;
            map.Y = ( ( world.X / TILE_WIDTH_HALF + world.Y / TILE_HEIGHT_HALF ) / 2 ) - level;
            return map;
        }

        public Vector2Fp WorldPosToMapPos( Vector2Fp world ) {
            Vector2Fp map = Vector2Fp.Zero;
            map.X = ( ( world.X / TILE_WIDTH_HALF_FP - world.Y / TILE_HEIGHT_HALF_FP ) / F64.Two ) + F64.FromInt( level );
            map.Y = ( ( world.Y / TILE_HEIGHT_HALF_FP + world.X / TILE_WIDTH_HALF_FP ) / F64.Two ) - F64.FromInt( level );
            return map;
        }

        public FlowTile[] PathFind( int destIndex ) {
            FlowTile[] flowTiles = new FlowTile[widthCount * heightCount];
            for ( int i = 0; i < flowTiles.Length; i++ ) {
                flowTiles[i].parentIndex = -1;
            }

            flowTiles[destIndex].parentIndex = -2;
            flowTiles[destIndex].pos = tiles[destIndex].worldPos;
            flowTiles[destIndex].flow = Vector2Fp.Zero;

            int neighborsCount = 0;
            Span<int> neighbors = stackalloc int[8];

            Queue<int> frontier = new Queue<int>( widthCount * heightCount );
            frontier.Enqueue( destIndex );
            while ( frontier.Count != 0 ) {
                int tileIndex = frontier.Dequeue();
                (int xIndex, int yIndex) = FlatIndexToPosIndex( tileIndex );
                GetNeighbors8( ref neighbors, ref neighborsCount, tileIndex );
                for ( int _I = 0; _I < neighborsCount; _I++ ) {
                    int neighborIndex = neighbors[_I];
                    (int xIndexN, int yIndexN) = FlatIndexToPosIndex( neighborIndex );
                    if ( flowTiles[neighborIndex].parentIndex == -1 && tiles[neighborIndex].flags != IsoTileFlags.BLOCKED ) {
                        flowTiles[neighborIndex].parentIndex = tileIndex;
                        flowTiles[neighborIndex].pos = tiles[neighborIndex].worldPos;
                        flowTiles[neighborIndex].flow = flowTiles[tileIndex].pos - flowTiles[neighborIndex].pos;
                        flowTiles[neighborIndex].flow = Vector2Fp.NormalizeFast( flowTiles[neighborIndex].flow );
                        flowTiles[neighborIndex].xDeltaIndex = xIndex - xIndexN;
                        flowTiles[neighborIndex].yDeltaIndex = yIndex - yIndexN;
                        frontier.Enqueue( neighborIndex );
                    }
                }
            }

            (int destX, int destY) = FlatIndexToPosIndex( destIndex );

            // Line of site check using dda
            for ( int i = 0; i < flowTiles.Length; i++ ) {
                if ( flowTiles[i].parentIndex != -1 && i != destIndex ) {
                    (int ix, int iy) = FlatIndexToPosIndex( i );
                    int dx = ix - destX;
                    int dy = iy - destY;
                    int steps = Math.Max( Math.Abs( dx ), Math.Abs( dy ) );
                    // @HACK(DECLAN): Can't use floats here!!
                    float xInc = (float)dx / steps;
                    float yInc = (float)dy / steps;
                    float x = destX;
                    float y = destY;
                    bool los = true;
                    for ( int j = 0; j < steps; j++ ) {
                        x += xInc;
                        y += yInc;
                        int tileIndex = PosIndexToFlatIndex( (int)x, (int)y );
                        if ( flowTiles[tileIndex].parentIndex == -1) {
                            los = false;
                            break;
                        }
                    }

                    if ( los ) {
                        flowTiles[i].flow = flowTiles[destIndex].pos - flowTiles[i].pos;
                        flowTiles[i].flow = Vector2Fp.NormalizeFast( flowTiles[i].flow );
                    }
                }
            }

            return flowTiles;
        }

        public void GetNeighbors8( ref Span<int> neighbors, ref int count, int tileIndex ) {
            (int x, int y) = FlatIndexToPosIndex( tileIndex );
            count = 0;
            if ( x > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x - 1, y );
            }
            if ( x < widthCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x + 1, y );
            }
            if ( y > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x, y - 1 );
            }
            if ( y < heightCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x, y + 1 );
            }
            if ( x > 0 && y > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x - 1, y - 1 );
            }
            if ( x < widthCount - 1 && y > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x + 1, y - 1 );
            }
            if ( x > 0 && y < heightCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x - 1, y + 1 );
            }
            if ( x < widthCount - 1 && y < heightCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x + 1, y + 1 );
            }
        }

        public void GetNeighbors4( ref Span<int> neighbors, ref int count, int tileIndex ) {
            (int x, int y) = FlatIndexToPosIndex( tileIndex );
            count = 0;
            if ( x > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x - 1, y );
            }
            if ( x < widthCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x + 1, y );
            }
            if ( y > 0 ) {
                neighbors[count++] = PosIndexToFlatIndex( x, y - 1 );
            }
            if ( y < heightCount - 1 ) {
                neighbors[count++] = PosIndexToFlatIndex( x, y + 1 );
            }
        }

        public int IsoTileIndexFromWorldPos( Vector2 pos ) {
            Vector2 map = WorldPosToMapPos( pos );
            int x = (int)MathF.Floor(map.X);
            int y = (int)MathF.Floor(map.Y);
            if ( x >= 0 && x < widthCount && y >= 0 && y < heightCount ) {
                return PosIndexToFlatIndex( x, y );
            }
            return -1;
        }

        // Assumes dir is normalized
        public Vector2Fp LockToCardinalWorldDirection( Vector2Fp dir ) {
            F64 dl = Vector2Fp.Dot( dir, CARDINAL_LEFT_WORLD );
            F64 dr = Vector2Fp.Dot( dir, CARDINAL_RIGHT_WORLD );
            F64 du = Vector2Fp.Dot( dir, CARDINAL_UP_WORLD );
            F64 dd = Vector2Fp.Dot( dir, CARDINAL_DOWN_WORLD );
            F64 maxDot = F64.Max( dr, F64.Max( dl, F64.Max( du, dd ) ) );
            if ( maxDot == dl ) {
                return CARDINAL_LEFT_WORLD;
            }
            else if ( maxDot == dr ) {
                return CARDINAL_RIGHT_WORLD;
            }
            else if ( maxDot == du ) {
                return CARDINAL_UP_WORLD;
            }
            else {
                return CARDINAL_DOWN_WORLD;
            }
        }

        public int IsoTileIndexFromWorldPos( Vector2Fp pos ) {
            Vector2Fp map = WorldPosToMapPos( pos );
            int x = F64.FloorToInt( map.X );
            int y = F64.FloorToInt( map.Y );
            if ( x >= 0 && x < widthCount && y >= 0 && y < heightCount ) {
                return PosIndexToFlatIndex( x, y );
            }
            return -1;
        }


        public int PosIndexToFlatIndex( int x, int y ) {
            return x + y * widthCount;
        }

        public (int, int) FlatIndexToPosIndex( int i ) {
            int x = i % widthCount;
            int y = i / widthCount;
            return (x, y);
        }
    }

}
