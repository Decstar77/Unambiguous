using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game {

    public struct IsoTile {
        public int              xIndex;
        public int              yIndex;
        public int              zIndex;
        public Vector2          worldPos;
        public Vector2          worldVanishingPoint;
        public SpriteTexture?   sprite;
    }

    public class IsoGrid {
        public static float TILE_WIDTH = 32;
        public static float TILE_WIDTH_HALF = TILE_WIDTH / 2;
        public static float TILE_HEIGHT = 16;
        public static float TILE_HEIGHT_HALF = TILE_HEIGHT / 2;
        public static float TILE_LEVE = 16;
        public static float TILE_LEVEL_HALF = TILE_LEVE / 2;
        public int widthCount;
        public int heightCount;
        public int levelCount;
        public IsoTile[,,] tiles = null;
        public static Vector2 localVanishingPoint = new Vector2( 0, -9 );

        public IsoGrid( int wc, int hc, int lc ) {
            widthCount = wc;
            heightCount = hc;
            levelCount = lc;
            tiles = new IsoTile[wc, hc, lc];
            for ( int x = 0; x < wc; x++ ) {
                for ( int y = 0; y < hc; y++ ) {
                    for ( int z = 0; z < lc; z++ ) {
                        IsoTile tile = new IsoTile();
                        tile.xIndex = x;
                        tile.yIndex = y;
                        tile.zIndex = z;
                        tile.worldPos = MapPosToWorldPos( x, y, z );
                        tile.worldVanishingPoint = tile.worldPos + localVanishingPoint;
                        tiles[x, y, z] = tile;
                    }
                }
            }
        }

        public void FillLevel( int z, SpriteTexture? sprite ) {
            for ( int x = 0; x < widthCount; x++ ) {
                for ( int y = 0; y < heightCount; y++ ) {
                    tiles[x, y, z].sprite = sprite;
                }
            }
        }

        public void PlaceTile( int x, int y, int z, SpriteTexture? sprite ) {
            tiles[x, y, z].sprite = sprite;
        }

        public Vector2 MapPosToWorldPos( int x, int y, int z ) {
            x = x - z;
            y = y + z;

            Vector2 world = Vector2.Zero;
            world.X = x * TILE_WIDTH_HALF + y * TILE_WIDTH_HALF;
            world.Y = -x * TILE_HEIGHT_HALF + y * TILE_HEIGHT_HALF;

            return world;
        }

        public Vector2 WorldPosToMapPos( Vector2 world ) {
            Vector2 map = Vector2.Zero;
            map.X = ( world.X / TILE_WIDTH_HALF + world.Y / TILE_HEIGHT_HALF ) / 2;
            map.Y = ( world.Y / TILE_HEIGHT_HALF - world.X / TILE_WIDTH_HALF ) / 2;
            return map;
        }
    }
}
