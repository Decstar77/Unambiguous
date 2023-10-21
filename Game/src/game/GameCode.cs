using System.Numerics;

namespace Game {

    public enum EntityType {
        INVALID = 0,
        BALL = 1,
    }

    public class Entity {
        public EntityType type;
        public Vector2 pos;
        public int gridLevel = 1;
        public SpriteTexture? sprite;
    }

    public class GameCode {
        public IsoGrid grid = new IsoGrid(10, 10, 2);
        public Entity localPlayer = new Entity();
        public SpriteTexture? gridTexture = null;
        public SpriteTexture? gridBlockTexture = null;
        public SpriteTexture? ballTexture = null;
        public Entity[] entities = new Entity[100];

        public void Init() {
            localPlayer.sprite = Content.LoadSpriteTexture( "unit_basic_man_single.png" );
            gridTexture = Content.LoadSpriteTexture( "tile_test.png" );
            gridBlockTexture = Content.LoadSpriteTexture( "tile_blocker.png" );
            ballTexture = Content.LoadSpriteTexture( "ball_basic.png" );
            localPlayer.pos.X = 1;

            grid.FillLevel( 0, gridTexture );
            grid.PlaceTile( 0, 0, 1, gridBlockTexture );
            grid.PlaceTile( 2, 3, 1, gridBlockTexture );
            grid.PlaceTile( 7, 4, 1, gridBlockTexture );

            SpawnBall( grid.MapPosToWorldPos( 1, 5, 0 ) );
            SpawnBall( grid.MapPosToWorldPos( 3, 5, 0 ) );
            SpawnBall( grid.MapPosToWorldPos( 5, 5, 0 ) );
            SpawnBall( grid.MapPosToWorldPos( 7, 5, 0 ) );
        }

        public void UpdateTick( float dt ) {
            float spood = 25.0f;

            if ( Engine.KeyIsDown( InputKey.ESCAPE ) ) {
                Engine.Close();
            }

            if ( Engine.KeyIsDown( InputKey.W ) ) {
                Engine.camera.pos.Y += spood * dt;
            }

            if ( Engine.KeyIsDown( InputKey.S ) ) {
                Engine.camera.pos.Y -= spood * dt;
            }

            if ( Engine.KeyIsDown( InputKey.A ) ) {
                Engine.camera.pos.X -= spood * dt;
            }

            if ( Engine.KeyIsDown( InputKey.D ) ) {
                Engine.camera.pos.X += spood * dt;
            }

            if ( Engine.input.scrollY != 0) {
                float zoom = Engine.camera.zoom + Engine.input.scrollY * -0.1f;
                Engine.CameraSetZoomPoint( zoom, Engine.input.mousePos );
            }
        }

        public void UpdateRender( float dt ) {
            DrawCommands drawCommands = new DrawCommands();

            for ( int i = 0; i < entities.Length; i++ ) {
                if ( entities[i] != null ) {
                    drawCommands.DrawSprite( entities[i].sprite, entities[i].pos, 0, entities[i].gridLevel, entities[i].pos );
                }
            }

            Vector2 playerVanishingPoint = localPlayer.pos - new Vector2( 0, 12 ) ;
            drawCommands.DrawSprite( localPlayer.sprite, localPlayer.pos, 0, 1, playerVanishingPoint );

            for ( int z = 0; z < grid.levelCount; z++ ) {
                for ( int x = 0; x < grid.widthCount; x++ ) {
                    for ( int y = grid.heightCount - 1; y >= 0; y-- ) {
                        if ( grid.tiles[x, y, z].sprite != null ) {
                            drawCommands.DrawSprite( grid.tiles[x, y, z].sprite, grid.tiles[x, y, z].worldPos, 0, z, grid.tiles[x, y, z].worldVanishingPoint );
                        }
                    }
                }
            }

            drawCommands.commands = drawCommands.commands.OrderBy( x => x.gridLevel ).ThenBy( x => -x.vanishingPoint.Y ).ToList();

            if ( CVars.DrawVanishingPoint.Value || false ) {
                for ( int x = 0; x < grid.widthCount; x++ ) {
                    for ( int y = grid.heightCount - 1; y >= 0; y-- ) {
                        if ( grid.tiles[x, y, 1].sprite != null ) {
                            drawCommands.DrawCircle( grid.tiles[x, y, 1].worldVanishingPoint, 1 );
                        }
                    }
                }
                drawCommands.DrawCircle( playerVanishingPoint, 1 );
            }

            //drawCommands.DrawRect( new Vector2( -10, -10 ), new Vector2( 10, 10 ) );

            //drawCommands.DrawText($"{Engine.camera.pos}", new Vector2(0, 0));
            drawCommands.DrawText( $"{Engine.input.mousePos}", new Vector2( 0, 0 ) );

            Engine.SubmitDrawCommands( drawCommands );
        }

        public Entity? SpawnEntity( EntityType type ) {
            for ( int i = 0; i < entities.Length; i++ ) {
                if ( entities[i] == null ) {
                    entities[i] = new Entity();
                    entities[i].type = type;
                    return entities[i];
                }
            }

            return null;
        }

        public Entity? SpawnBall( Vector2 pos ) {
            Entity? ent = SpawnEntity( EntityType.BALL );
            if ( ent == null ) {
                return null;
            }

            ent.pos = pos;
            ent.sprite = ballTexture;

            return ent;
        }

    }
}
