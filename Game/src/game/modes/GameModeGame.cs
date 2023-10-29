using OpenTK.Mathematics;
using Shared;
using SoLoud;
using System.Diagnostics;

namespace Game {

    public enum EntityType {
        INVALID = 0,
        BALL = 1,
    }

    public class Entity {
        public EntityType type;
        public Vector2 pos;
        public int gridLevel = 1;
        public SpriteTexture sprite;
        public CircleCollider circleCollider;
    }

    public class GameModeGame : GameMode {
        private IsoGrid grid = new IsoGrid(10, 10, 2);
        private SpriteTexture workerTexture = null;
        private SpriteTexture gridTexture = null;
        private SpriteTexture gridBlockTexture = null;
        private SpriteTexture ballTexture = null;
        private SoloudObject sndHuh = null;
        private UIMaster uiMaster = new UIMaster();
        private Entity[]            entities = new Entity[ 100 ];
        private int                 entityCount = 0;
        private SyncQueues          syncQueues = new SyncQueues();
        private int                 turnNumber = 1;
        private float               turnAccumulator = 0.0f;
        private MapTurn             localTurn = new MapTurn();
        private int                 localPlayerNumber = 0;
        private bool                isMultiplayer = false;
        private bool                isDragging = false;
        private Vector2             startDrag = Vector2.Zero;
        private Vector2             endDrag = Vector2.Zero;

        public override void Init( GameModeInitArgs args ) {
            isMultiplayer = args.multiplayer;
            if ( isMultiplayer == true ) {
                localPlayerNumber = args.localPlayerNumber;
            }
            else {
                localPlayerNumber = 1;
            }

            workerTexture = Content.LoadSpriteTexture( "unit_basic_man_single.png" );
            gridTexture = Content.LoadSpriteTexture( "tile_test.png" );
            gridBlockTexture = Content.LoadSpriteTexture( "tile_blocker.png" );
            ballTexture = Content.LoadSpriteTexture( "ball_basic.png" );
            sndHuh = Content.LoadWav( "huh.wav" );

            grid.FillLevel( 0, gridTexture );
            grid.PlaceTile( 0, 0, 1, gridBlockTexture );
            grid.PlaceTile( 2, 3, 1, gridBlockTexture );
            grid.PlaceTile( 3, 3, 1, gridBlockTexture );
            grid.PlaceTile( 7, 4, 1, gridBlockTexture );

            syncQueues.Start();
        }

        public override void NetworkPacketReceived( byte[] data, int length ) {
            if ( length > 0 ) {
                GamePacketType type = (GamePacketType )data[ 0 ];
                if ( type == GamePacketType.MAP_TURN ) {
                    MapTurn turn = new MapTurn();
                    int playerNumber = data[ 1 ];
                    turn.SerializeFromBytes( data, 2 );
                    syncQueues.AddTurn( playerNumber, turn );
                }
            }
        }

        public override void UpdateTick( float dt ) {
            turnAccumulator += dt;
            if ( turnAccumulator >= SyncQueues.TurnRateMS ) {
                turnAccumulator = 0;
                if ( syncQueues.CanTurn() == true ) {
                    localTurn.checkSum = 2039;
                    localTurn.turnNumber = turnNumber + syncQueues.GetSlidingWindowWidth();

                    syncQueues.AddTurn( localPlayerNumber, localTurn );

                    if ( isMultiplayer == true ) {
                        byte[] turnPacket = localTurn.SerializeToBytes( (byte)localPlayerNumber );
                        Engine.NetworkSendPacket( turnPacket, true );
                    }
                    else {
                        MapTurn turn = new MapTurn();
                        turn.turnNumber = localTurn.turnNumber;
                        turn.checkSum = localTurn.checkSum;
                        syncQueues.AddTurn( 2, turn ); // Dummy turn for AI
                    }

                    localTurn = new MapTurn(); // @SPEED: Alloc is needed because sync is holding onto a pointer...

                    MapTurn player1Turn = syncQueues.GetNextTurn( 1 );
                    MapTurn player2Turn = syncQueues.GetNextTurn( 2 );

                    Tick( player1Turn, player2Turn );

                    syncQueues.FinishTurn();
                }
                else {

                }
            }

            float cameraX = ( Engine.KeyIsDown( InputKey.D ) ? 1.0f : 0.0f ) - ( Engine.KeyIsDown( InputKey.A ) ? 1.0f : 0.0f );
            float cameraY = ( Engine.KeyIsDown( InputKey.W ) ? 1.0f : 0.0f ) - ( Engine.KeyIsDown( InputKey.S ) ? 1.0f : 0.0f );
            if ( cameraX != 0.0f || cameraY != 0.0f ) {
                Vector2 cameraDir = Vector2.Normalize( new Vector2( cameraX, cameraY ) );
                Engine.camera.pos += cameraDir * 25.0f * dt;
            }

            bool mouseMoved = true;
            Vector2 mouseScreen = Engine.MouseScreenPos();
            Vector2 mouseDelta = Engine.MouseScreenDelta();
            if ( mouseDelta.X == 0 && mouseDelta.Y == 0 ) {
                mouseMoved = false;
            }

            if ( Engine.MouseJustDown( 1 ) ) { startDrag = mouseScreen; } 
            if ( Engine.MouseDown( 1 ) && mouseMoved ) { isDragging = true; }
            if ( isDragging ) { endDrag = mouseScreen; }
            if ( Engine.MouseJustUp( 1 ) ) {
                if ( isDragging ) {
                    isDragging = false;
                }
            }
        }

        private void Tick( MapTurn player1Turn, MapTurn player2Turn ) {
            Debug.Assert( player1Turn.turnNumber == player2Turn.turnNumber );
            Debug.Assert( player1Turn.turnNumber == turnNumber );
            turnNumber++;
        }

        public override void UpdateRender( float dt ) {
            DrawCommands drawCommands = new DrawCommands();

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
            }

            if ( isDragging ) {
                Vector2 min = Vector2.ComponentMin( startDrag, endDrag );
                Vector2 max = Vector2.ComponentMax( startDrag, endDrag );
                drawCommands.currentColor = new Vector4( 0.2f, 0.2f, 0.8f, 0.5f );
                drawCommands.DrawScreenRect( min, max );
            }

            //for ( int x = 0; x < grid.widthCount; x++ ) {
            //    for ( int y = grid.heightCount - 1; y >= 0; y-- ) {
            //        if ( grid.tiles[x, y, 1].sprite != null ) {
            //            drawCommands.DEBUG_DrawConvexCollider( grid.tiles[x, y, 1].convexCollider );
            //        }
            //    }
            //}

            //drawCommands.DrawCircle( localPlayer.circleCollider );

            //ConvexCollider t = new ConvexCollider(
            //    new Vector2(-10, -10),
            //    new Vector2(-10, 10),
            //    new Vector2(10, 10),
            //    new Vector2(10, -10)
            //);


            //drawCommands.DrawRect( new Vector2( -10, -10 ), new Vector2( 10, 10 ) );
            //drawCommands.DrawCircle( localPlayer.pos, 1 );

            //drawCommands.DrawText($"{Engine.camera.pos}", new Vector2(0, 0));
            //drawCommands.DrawText( $"{Engine.input.mousePos}", new Vector2( 0, 0 ) );

            uiMaster.UpdateAndRender( drawCommands );

            Engine.SubmitDrawCommands( drawCommands );
        }

        public override void Shutdown() {

        }
    }
}
