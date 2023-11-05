﻿using FixMath;
using FixPointCS;
using OpenTK.Mathematics;
using Shared;
using SoLoud;
using System.Diagnostics;

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

        public static bool operator ==( EntityId a, EntityId b ) { return a.index == b.index && a.generation == b.generation; }
        public static bool operator !=( EntityId a, EntityId b ) { return a.index != b.index || a.generation != b.generation; }
        public override bool Equals( object obj ) { return obj is EntityId && this == (EntityId)obj; }
    }

    public enum EntityType : int {
        INVALID = 0,

        UNIT_BEGIN,
        UNIT_WORKER,
        UNIT_END,
    }

    public class Entity {
        public EntityId     id;
        public EntityType   type;
        public Vector2Fp    pos;
        public Vector2      visualPos;

        public bool         hasDestination;
        public Vector2Fp      destination;

        public int gridLevel = 1;
        public int playerNumber;
        public Sprite         sprite;
        public CircleCollider circleCollider;
        public bool           isSelected;
        public RectBounds     selectionBoundsLocal;
        public RectBounds SelectionBoundsWorld { get { RectBounds w = selectionBoundsLocal; w.Translate( visualPos ); return w; } }


        public void TeleportTo( Vector2Fp newPos ) {
            pos = newPos;
            visualPos = pos.ToV2();
        }
    }

    public class GameModeGame : GameMode {
        private SpriteTexture       sprWorker = null;
        private SpriteTexture       sprWorkerSelection = null;
        private SpriteTexture       sprGrid = null;
        private SpriteTexture       sprGridBlock = null;
        private SpriteTexture       sprBall = null;

        private SoloudObject        sndHuh = null;

        private UIMaster            uiMaster = new UIMaster();

        private IsoGrid             groundGrid = new IsoGrid( 10, 10, 0 );
        private IsoGrid             buildingGrid = new IsoGrid( 10, 10, 1 );
        private Entity[]            entities = new Entity[ 100 ];
        private SyncQueues          syncQueues = new SyncQueues();
        private int                 turnNumber = 1;
        private float               turnAccumulator = 0.0f;
        private MapTurn             localTurn = new MapTurn();
        private int                 localPlayerNumber = 0;
        private bool                isMultiplayer = false;
        private bool                isDragging = false;
        private Vector2             startDrag = Vector2.Zero;
        private Vector2             endDrag = Vector2.Zero;
        private Vector2             testVec = Vector2.Zero;

        public override void Init( GameModeInitArgs args ) {
            isMultiplayer = args.multiplayer;
            if ( isMultiplayer == true ) {
                localPlayerNumber = args.localPlayerNumber;
            }
            else {
                localPlayerNumber = 1;
            }

            sprWorker = Content.LoadSpriteTexture( "unit_basic_man_single.png" );
            sprWorkerSelection = Content.LoadSpriteTexture( "unit_basic_man_selection.png" );
            sprGrid = Content.LoadSpriteTexture( "tile_test.png" );
            sprGridBlock = Content.LoadSpriteTexture( "tile_blocker.png" );
            sprBall = Content.LoadSpriteTexture( "ball_basic.png" );
            sndHuh = Content.LoadWav( "huh.wav" );

            groundGrid.Fill( sprGrid );
            buildingGrid.PlaceTile( 0, 0, IsoTileFlags.BLOCKED, sprGridBlock );
            buildingGrid.PlaceTile( 2, 3, IsoTileFlags.BLOCKED, sprGridBlock );
            buildingGrid.PlaceTile( 3, 3, IsoTileFlags.BLOCKED, sprGridBlock );
            buildingGrid.PlaceTile( 7, 4, IsoTileFlags.BLOCKED, sprGridBlock );

            Vector2Fp p = buildingGrid.MapPosToWorldPos( 5, 5 );
            MapApply_SpawnWorkder( p, 1 );

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
                    localTurn.checkSum = MapComputeCheckSum();
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

                    MapTick( player1Turn, player2Turn );

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

            if ( Engine.MouseScrollDelta() != 0 ) {
                float zoom = Engine.camera.zoom + Engine.MouseScrollDelta() * -0.1f;
                Engine.CameraSetZoomPoint( zoom, Engine.MouseScreenPos() );
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
                    Vector2 startWorld = Engine.ScreenPosToWorldPos( startDrag );
                    Vector2 endWorld = Engine.ScreenPosToWorldPos( endDrag );

                    Vector2 minWorld = Vector2.ComponentMin( startWorld, endWorld );
                    Vector2 maxWorld = Vector2.ComponentMax( startWorld, endWorld );

                    RectBounds selectionRect = new RectBounds().SetFromMinMax( minWorld, maxWorld );
                    for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                        Entity ent = entities[entityIndex];
                        if ( ent != null ) {
                            if ( Intersections.RectVsRect( selectionRect, ent.SelectionBoundsWorld ) ) {
                                ent.isSelected = true;
                            }
                            else {
                                ent.isSelected = false;
                            }
                        }
                    }
                }
                else {
                    for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                        Entity ent = entities[entityIndex];
                        if ( ent != null ) {
                            ent.isSelected = false;
                        }
                    }
                    //
                }
            }

            if ( Engine.MouseJustUp( 2 ) ) {
                for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                    Entity ent = entities[entityIndex];
                    if ( ent != null ) {
                        if ( ent.isSelected == true ) {
                            Vector2 worldPos = Engine.ScreenPosToWorldPos( Engine.MouseScreenPos() );
                            MapCreateAction_MoveUnit( ent, worldPos );
                        }
                    }
                }
            }
        }

        public override void UpdateRender( float dt ) {
            DrawCommands drawCommands = new DrawCommands();

            for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity ent = entities[ entityIndex ];
                if ( ent != null && ent.sprite.texture != null ) {
                    Vector2 p = ent.pos.ToV2();
                    ent.visualPos = Vector2.Lerp( ent.visualPos, p, 0.1f );

                    if ( ent.isSelected == true ) {
                        drawCommands.DrawSprite( sprWorkerSelection, ent.visualPos - new Vector2( 0.5f, 0 ), 0, ent.gridLevel, ent.visualPos );
                    }

                    Vector2 spriteCenter = ent.visualPos + ent.sprite.originOffset;
                    drawCommands.DrawSprite( ent.sprite.texture, spriteCenter, 0, ent.gridLevel, ent.visualPos );
                }
            }

            for ( int x = 0; x < groundGrid.widthCount; x++ ) {
                for ( int y = groundGrid.heightCount - 1; y >= 0; y-- ) {
                    int flatIndex = groundGrid.PosIndexToFlatIndex( x, y );
                    if ( groundGrid.tiles[flatIndex].sprite != null ) {
                        drawCommands.DrawSprite(
                            groundGrid.tiles[flatIndex].sprite,
                            groundGrid.tiles[flatIndex].worldPos.ToV2(),
                            0, groundGrid.level,
                            groundGrid.tiles[flatIndex].worldVanishingPoint
                        );
                    }
                }
            }

            for ( int x = 0; x < buildingGrid.widthCount; x++ ) {
                for ( int y = buildingGrid.heightCount - 1; y >= 0; y-- ) {
                    int flatIndex = groundGrid.PosIndexToFlatIndex( x, y );
                    if ( buildingGrid.tiles[flatIndex].sprite != null ) {
                        drawCommands.DrawSprite( buildingGrid.tiles[flatIndex].sprite,
                            buildingGrid.tiles[flatIndex].worldPos.ToV2(),
                            0, buildingGrid.level,
                            buildingGrid.tiles[flatIndex].worldVanishingPoint
                        );
                    }
                }
            }

            drawCommands.commands = drawCommands.commands.OrderBy( x => x.gridLevel ).ThenBy( x => -x.vanishingPoint.Y ).ToList();

            if ( isDragging ) {
                Vector2 startWorld = Engine.ScreenPosToWorldPos( startDrag );
                Vector2 endWorld = Engine.ScreenPosToWorldPos( endDrag );
                Vector2 minWorld = Vector2.ComponentMin( startWorld, endWorld );
                Vector2 maxWorld = Vector2.ComponentMax( startWorld, endWorld );
                drawCommands.DrawRect( minWorld, maxWorld, new Vector4( 0.2f, 0.2f, 0.8f, 0.5f ) );
            }

            if ( CVars.DrawVanishingPoint.Value || false ) {
                for ( int x = 0; x < buildingGrid.widthCount; x++ ) {
                    for ( int y = buildingGrid.heightCount - 1; y >= 0; y-- ) {
                        int flatIndex = groundGrid.PosIndexToFlatIndex( x, y );
                        if ( buildingGrid.tiles[flatIndex].sprite != null ) {
                            drawCommands.DrawCircle( buildingGrid.tiles[flatIndex].worldVanishingPoint, 1 );
                        }
                    }
                }
            }

            if ( CVars.DrawGroundGridWorldPoints.Value || false ) {
                for ( int x = 0; x < groundGrid.widthCount; x++ ) {
                    for ( int y = groundGrid.heightCount - 1; y >= 0; y-- ) {
                        int flatIndex = groundGrid.PosIndexToFlatIndex( x, y );
                        if ( groundGrid.tiles[flatIndex].sprite != null ) {
                            drawCommands.DrawCircle( groundGrid.tiles[flatIndex].worldPos.ToV2(), 1 );
                        }
                    }
                }
            }

            if ( CVars.DrawSelectionBounds.Value || false ) {
                for ( int i = 0; i < entities.Length; i++ ) {
                    if ( entities[i] != null ) {
                        drawCommands.DrawRect( entities[i].SelectionBoundsWorld, new Vector4( 0.5f, 0.5f, 0.5f, 0.5f ) );
                    }
                }
            }

            if ( CVars.DrawPlayerStats.Value || false ) {
                drawCommands.DrawText( $"PlayerNumber={localPlayerNumber}", Vector2.Zero );
            }

            if ( true ) {
                Vector2 mousePosWorld = Engine.MouseWorldPos();
                int index = groundGrid.IsoTileIndexFromWorldPos( mousePosWorld );
                if ( index >= 0 ) {
                    bool isBlocked = buildingGrid.tiles[index].flags.HasFlag( IsoTileFlags.BLOCKED );
                    Vector4 color= isBlocked ? new Vector4( 1, 0, 0, 0.5f ) : new Vector4( 0, 1, 0, 0.5f );
                    drawCommands.DEBUG_DrawConvexCollider( groundGrid.tiles[index].roofConvexCollider, color );
                }

                drawCommands.DrawText( $"mousePosWorld={mousePosWorld}", Vector2.Zero );

                //drawCommands.DrawCircle( mousePosWorld, 1.0f );
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

        private Entity MapLookUpEntity( EntityId id ) {
            for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity ent = entities[entityIndex];
                if ( ent != null ) {
                    if ( ent.id == id ) {
                        return ent;
                    }
                }
            }

            return null;
        }

        private void MapTick( MapTurn player1Turn, MapTurn player2Turn ) {
            //Logger.Log( $"Tick={ turnNumber } \t|| Checksum={ player1Turn.checkSum }" );
            Debug.Assert( player1Turn.turnNumber == player2Turn.turnNumber );
            Debug.Assert( player1Turn.turnNumber == turnNumber );
            Debug.Assert( player1Turn.checkSum == player2Turn.checkSum );

            // Player 1 actions
            for ( int actionIndex = 0; actionIndex < player1Turn.actions.Count; actionIndex++ ) {
                MapApplyAction( player1Turn.actions[actionIndex] );
            }
            // Player 2 actions
            for ( int actionIndex = 0; actionIndex < player2Turn.actions.Count; actionIndex++ ) {
                MapApplyAction( player2Turn.actions[actionIndex] );
            }

            // Update entities
            for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity ent = entities[entityIndex];
                if ( ent != null ) {
                    switch ( ent.type ) {
                        case EntityType.UNIT_WORKER: {
                            if ( ent.hasDestination == true ) {
                                ent.pos = Vector2Fp.Lerp( ent.pos, ent.destination, F64.FromFloat( 0.1f ) );
                            }
                        }
                        break;
                    }
                }
            }

            turnNumber++;
        }

        public long MapComputeCheckSum() {
            long sum = 0;
            for ( int entityIndex = 0; entityIndex < entities.Length; entityIndex++ ) {
                Entity ent = entities[entityIndex];
                if ( ent != null ) {
                    sum -= ent.pos.RawX;
                    sum += ent.pos.RawY;
                }
            }

            return sum;
        }

        public void MapApplyAction( MapAction mapAction ) {
            switch ( mapAction.GetMapActionType() ) {
                case MapActionType.INVALID: {
                    Logger.Log( "ERROR INVALID ACTION" );
                };
                break;
                case MapActionType.MOVE_UNITS: {
                    MapAction_MoveUnits action = ( MapAction_MoveUnits )mapAction;
                    Entity ent = MapLookUpEntity( action.entId );
                    if ( ent != null ) {
                        ent.hasDestination = true;
                        ent.destination = action.destination;
                    }
                }
                break;
                default: {
                    Logger.Log( "ERROR INVALID ACTION" );
                }
                break;
            }
        }

        public Entity MapApply_SpawnEntity( EntityType type, int playerNumber ) {
            for ( int i = 0; i < entities.Length; i++ ) {
                if ( entities[i] == null ) {
                    Entity ent = entities[i] = new Entity();
                    ent.type = type;
                    ent.playerNumber = playerNumber;
                    ent.id.index = i;
                    ent.id.generation = 1;
                    return ent;
                }
            }

            Debug.Assert( false );

            return null;
        }

        public Entity MapApply_SpawnWorkder( Vector2Fp pos, int playerNumber ) {
            Entity ent = MapApply_SpawnEntity( EntityType.UNIT_WORKER, playerNumber );
            if ( ent != null ) {
                ent.TeleportTo( pos );
                ent.sprite.texture = sprWorker;
                ent.sprite.originOffset = new Vector2( 0, 12 );
                ent.selectionBoundsLocal.SetFromCenterDims( new Vector2( -1, 6 ), new Vector2( 5, 12 ) );
            }
            return ent;
        }

        public void MapCreateAction_MoveUnit( Entity ent, Vector2 destination ) {
            MapAction_MoveUnits action =new MapAction_MoveUnits();
            action.entId = ent.id;
            action.destination = destination.ToFp();
            localTurn.actions.Add( action );

            //Engine.AudioPlay( sndHuh );
        }

        public override void Shutdown() {

        }
    }
}
