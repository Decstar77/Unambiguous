using FixMath;
using Microsoft.VisualBasic;
using Raylib_cs;
using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Game {

    public interface GameScreen {
        public GameScreen Update();
    }

    public enum UISubmissionType {
        INVALID = 0,
        BUTTON,
        BLOCK_BUTTON,
    };

    public class UISubmission {
        public UISubmissionType type;
        public string text = "";
        public Vector2 textSize;
        public Rectangle rect;
        public bool isHovered;
        public bool isPressed;
        public Color baseColor;
        public Color hoveredColor;
        public Color pressedColor;
    }

    public class UIState {
        private Vector2 surfaceMouse = new Vector2();
        public bool elementHovered = false;
        private List<UISubmission> submissions = new List<UISubmission>();

        public void Reset() {
            surfaceMouse = Raylib.GetMousePosition();
            elementHovered = false;
            submissions.Clear();
        }

        public void Draw() {
            foreach( UISubmission sub in submissions ) {
                switch( sub.type ) {
                    case UISubmissionType.BUTTON: {
                        if( sub.isPressed ) {
                            Raylib.DrawRectangleRec( sub.rect, sub.pressedColor );
                        }
                        else if( sub.isHovered ) {
                            Raylib.DrawRectangleRec( sub.rect, sub.hoveredColor );
                        }
                        else {
                            Raylib.DrawRectangleRec( sub.rect, sub.baseColor );
                        }

                        Vector2 textPos = new Vector2(sub.rect.x + sub.rect.width / 2 - sub.textSize.X / 2,
                                                        sub.rect.y + sub.rect.height / 2 - sub.textSize.Y / 2);
                        Raylib.DrawTextEx( Raylib.GetFontDefault(), sub.text, textPos, 20, 1, Color.BLACK );
                    }
                    break;
                    case UISubmissionType.BLOCK_BUTTON: {
                        if( sub.isPressed ) {
                            Raylib.DrawRectangleRec( sub.rect, sub.pressedColor );
                        }
                        else if( sub.isHovered ) {
                            Raylib.DrawRectangleRec( sub.rect, sub.hoveredColor );
                        }
                        else {
                            Raylib.DrawRectangleRec( sub.rect, sub.baseColor );
                        }

                        Vector2 textPos = new Vector2(sub.rect.x + sub.rect.width / 2 - sub.textSize.X / 2,
                                                        sub.rect.y + sub.rect.height / 2 - sub.textSize.Y / 2);
                        Raylib.DrawTextEx( Raylib.GetFontDefault(), sub.text, textPos, 20, 1, Color.BLACK );
                    }
                    break;
                    default:
                        Debug.Assert( false );
                        break;
                }
            }
        }

        public bool DrawButtonCenter( int cx, int cy, string text ) {
            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 20, 1);
            Vector2 rectSize = new Vector2(textSize.X + 20, textSize.Y + 20);
            Rectangle rect = new Rectangle((float)cx - rectSize.X / 2, (float)cy - rectSize.Y / 2, rectSize.X, rectSize.Y);

            UISubmission submission = new UISubmission();
            submission.type = UISubmissionType.BUTTON;
            submission.text = text;
            submission.textSize = textSize;
            submission.rect = rect;
            submission.baseColor = Color.SKYBLUE;//UIColorsGet(UI_COLOR_SLOT_BACKGROUND);
            submission.hoveredColor = submission.baseColor;
            submission.hoveredColor.r = (byte)( (float)submission.hoveredColor.r * 1.2f );
            submission.hoveredColor.g = (byte)( (float)submission.hoveredColor.g * 1.2f );
            submission.hoveredColor.b = (byte)( (float)submission.hoveredColor.b * 1.2f );
            submission.pressedColor = submission.baseColor;
            submission.pressedColor.r = (byte)( (float)submission.pressedColor.r * 1.5f );
            submission.pressedColor.g = (byte)( (float)submission.pressedColor.g * 1.5f );
            submission.pressedColor.b = (byte)( (float)submission.pressedColor.b * 1.5f );
            submission.isHovered = Raylib.CheckCollisionPointRec( surfaceMouse, rect );
            submission.isPressed = submission.isHovered && Raylib.IsMouseButtonReleased( MouseButton.MOUSE_BUTTON_LEFT );

            submissions.Add( submission );
            elementHovered = elementHovered || submission.isHovered;

            return submission.isPressed;
        }

        public bool DrawButtonTopLeft( int x, int y, string text ) {
            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 20, 1);
            Vector2 rectSize = new Vector2(textSize.X + 20, textSize.Y + 20);
            int cx = (int)(x + rectSize.X / 2);
            int cy = (int)(y + rectSize.Y / 2);
            return DrawButtonCenter( cx, cy, text );
        }

        public bool DrawBlockButton( Vector2 center, Vector2 dims, Color c, string text ) {
            UISubmission submission = new UISubmission();
            submission.type = UISubmissionType.BLOCK_BUTTON;
            submission.text = text;
            submission.textSize = Raylib.MeasureTextEx( Raylib.GetFontDefault(), text, 20, 1 );
            submission.rect = new Rectangle( center.X - dims.X / 2, center.Y - dims.Y / 2, dims.X, dims.Y );
            submission.baseColor = c;
            submission.hoveredColor = c;
            submission.hoveredColor.r = (byte)( (float)submission.hoveredColor.r * 1.2f );
            submission.hoveredColor.g = (byte)( (float)submission.hoveredColor.g * 1.2f );
            submission.hoveredColor.b = (byte)( (float)submission.hoveredColor.b * 1.2f );
            submission.pressedColor = c;
            submission.pressedColor.r = (byte)( (float)submission.pressedColor.r * 1.5f );
            submission.pressedColor.g = (byte)( (float)submission.pressedColor.g * 1.5f );
            submission.pressedColor.b = (byte)( (float)submission.pressedColor.b * 1.5f );
            submission.isHovered = Raylib.CheckCollisionPointRec( surfaceMouse, submission.rect );
            submission.isPressed = submission.isHovered && Raylib.IsMouseButtonReleased( MouseButton.MOUSE_BUTTON_LEFT );

            submissions.Add( submission );
            elementHovered = elementHovered || submission.isHovered;

            return submission.isPressed;
        }
    }

    public class GameSettings {
        public static GameSettings Current = new GameSettings();
        public int ServerPort { get; set; } = 27164;
        public string ServerIp { get; set; } = "127.0.0.1";
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 720;
        public int WindowPosX { get; set; } = -1;
        public int WindowPosY { get; set; } = -1;
    }

    public class SreenMainMenu : GameScreen {
        public bool initialized = false;
        public UIState uiState = new UIState();
        public Sound clickSound;

        public virtual GameScreen Update() {
            if( !initialized ) {
                clickSound = Raylib.LoadSound( Content.ResolvePath( "purchase_01.wav" ) );
                initialized = true;
            }

            GameScreen nextScreen = this;

            Raylib.BeginDrawing();
            Raylib.ClearBackground( Color.WHITE );

            uiState.Reset();
            if( uiState.DrawButtonCenter( Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2 - 100, "Single Player" ) ) {
                Raylib.PlaySound( clickSound );
                nextScreen = new ScreenGame();
                //                 MapCreate(gameLocal.map, true);
                //                 MapStart(gameLocal.map);
                //                 gameLocal.playerNumber = 1;
                //                 gameLocal.screen = SCREEN_TYPE_GAME;
            }
            if( uiState.DrawButtonCenter( Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2, "Multiplayer" ) ) {
                if( GameClient.NetworkIsConnected() == false && GameClient.ConnectToServer( GameSettings.Current.ServerIp, GameSettings.Current.ServerPort ) ) {
                }
            }
            if( uiState.DrawButtonCenter( Raylib.GetScreenWidth() / 2, Raylib.GetScreenHeight() / 2 + 100, "Quit(How dare you !)" ) ) {
                //gameLocal.running = false;
            }

            if( GameClient.NetworkIsConnected() ) {
                byte[] data = new byte[1024];
                while( GameClient.NetworkPoll( data ) == true ) {
                    if( data[0] == (byte)GamePacketType.MAP_START ) {
                        ScreenGame game = new ScreenGame();
                        game.playerNumber = data[1];
                        game.isSinglePlayer = false;
                        nextScreen = game;
                        Console.WriteLine( $"Start game, playerNumber: {game.playerNumber}" );
                    }
                }
            }

            uiState.Draw();

            Raylib.EndDrawing();

            return nextScreen;
        }
    }

    public class ScreenGame : GameScreen {
        // Todo: Put this in a game local class!
        public bool initialized = false;
        public Map map = new Map();
        public int surfaceWidth;
        public int surfaceHeight;
        public RenderTexture2D surface;
        public Camera2D camera;
        public UIState uiState = new UIState();
        public Vector2 startDrag = Vector2.Zero;
        public Vector2 endDrag = Vector2.Zero;
        public bool dragging = false;
        public int playerNumber = 1;
        public float turnAccumulator = 0;
        public bool isSinglePlayer = true;
        public SyncQueues sync = new SyncQueues();
        public MapTurn localTurn = new MapTurn();
        public MapTurn localTurnAI = new MapTurn();

        public virtual GameScreen Update() {
            if( initialized == false ) {
                surfaceWidth = 1280;
                surfaceHeight = 720;
                surface = Raylib.LoadRenderTexture( surfaceWidth, surfaceHeight );

                camera = new Camera2D();
                camera.zoom = 1;
                camera.offset = new Vector2( surfaceWidth / 2.0f, surfaceHeight / 2.0f );

                map.Create();
                map.Start();

                sync.Start();

                initialized = true;

                return this;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            if( isSinglePlayer ) {
                turnAccumulator += Raylib.GetFrameTime();
                if( turnAccumulator >= SyncQueues.turnRateMS ) {
                    turnAccumulator = 0;
                    localTurn.turnNumber = map.turnNumber;
                    localTurnAI.turnNumber = map.turnNumber;
                    map.DoTurn( localTurn, localTurnAI );

                    // @Speed: We just need to clear/run the construtor again. Not allocate a new one.
                    localTurn = new MapTurn();
                    localTurnAI = new MapTurn();
                }
            }
            else {
                byte[] data = new byte[1024];
                while( GameClient.NetworkPoll( data ) ) {
                    if( data[0] == (byte)GamePacketType.MAP_TURN ) {
                        int playerNumber = data[1];
                        MapTurn turn = new MapTurn();
                        turn.SerializeFromBytes( data, 2 );
                        sync.AddTurn( playerNumber, turn );
                    }
                }

                turnAccumulator += Raylib.GetFrameTime();
                if( turnAccumulator >= SyncQueues.turnRateMS ) {
                    if( sync.CanTurn() ) {
                        turnAccumulator = 0;

                        localTurn.checkSum = map.ComputeCheckSum();
                        localTurn.turnNumber = map.turnNumber + sync.GetSlidingWindowWidth();
                        sync.AddTurn( playerNumber, localTurn );

                        byte[] bigData = localTurn.SerializeToBytes( (byte)GamePacketType.MAP_TURN, (byte)playerNumber );
                        GameClient.NetworkSendPacket( bigData, true );

                        // @Speed: We just need to clear/run the construtor again. Not allocate a new one.
                        localTurn = new MapTurn();

                        MapTurn player1Turn = sync.GetNextTurn( 1 );
                        MapTurn player2Turn = sync.GetNextTurn( 2 );
                        map.DoTurn( player1Turn, player2Turn );
                        sync.FinishTurn();
                    }
                    else {
                        // We are waiting here. Do something smort
                    }
                }
            }

            uiState.Reset();
            string r1Text = $"R1: {100}ms";
            string r2Text = $"R1: {100}ms";
            uiState.DrawButtonTopLeft( 10, 10, r1Text );
            uiState.DrawButtonTopLeft( 10, 40, r2Text );
            if( map.SelectionContainsEntityType( EntityType.BUILDINGS_TOWN_CENTER ) ) {
                uiState.DrawBlockButton(
                    new Vector2( Raylib.GetScreenWidth() - 60, Raylib.GetScreenHeight() - 60 ),
                    new Vector2( 100, 100 ),
                    Color.GRAY, "Build\nGeneral" );
            }

            float scale = MathF.Min((float)Raylib.GetScreenWidth() / surfaceWidth, (float)Raylib.GetScreenHeight() / surfaceHeight);
            Vector2 mouse = Raylib.GetMousePosition();
            Vector2 mouseDelta = Raylib.GetMouseDelta();
            bool mouseMoved = true;
            if( mouseDelta.X == 0 && mouseDelta.Y == 0 ) {
                mouseMoved = false;
            }

            Vector2 surfaceMouse = Vector2.Zero;
            surfaceMouse.X = ( mouse.X - ( Raylib.GetScreenWidth() - ( surfaceWidth * scale ) ) * 0.5f ) / scale;
            surfaceMouse.Y = ( mouse.Y - ( Raylib.GetScreenHeight() - ( surfaceHeight * scale ) ) * 0.5f ) / scale;
            surfaceMouse = Vector2.Clamp( surfaceMouse, Vector2.Zero, new Vector2( (float)surfaceWidth, (float)surfaceHeight ) );

            Vector2 mouseWorld = Raylib.GetScreenToWorld2D(new Vector2(surfaceMouse.X, surfaceMouse.Y), camera);
            Vector2Fp mouseWorldFp = Vector2Fp.FromFloat(mouseWorld.X, mouseWorld.Y);

            DoCameraPanning();
            DoCameraZooming();

            if( uiState.elementHovered == false ) {
                if( Raylib.IsMouseButtonPressed( MouseButton.MOUSE_BUTTON_LEFT ) ) {
                    startDrag = mouseWorld;
                }

                if( Raylib.IsMouseButtonDown( MouseButton.MOUSE_BUTTON_LEFT ) && mouseMoved ) {
                    dragging = true;
                }

                if( dragging == true ) {
                    endDrag = mouseWorld;
                }

                if( Raylib.IsMouseButtonReleased( MouseButton.MOUSE_LEFT_BUTTON ) ) {
                    if( dragging == true ) {
                        dragging = false;

                        Rect selectionRect = new Rect();
                        selectionRect.min = Vector2.Min( startDrag, endDrag );
                        selectionRect.max = Vector2.Max( startDrag, endDrag );

                        startDrag = Vector2.Zero;
                        endDrag = Vector2.Zero;

                        map.selection.Clear();
                        for( int i = 0; i < map.entities.Length; i++ ) {
                            Entity ent = map.entities[i];
                            if( ent != null && ent.flags.HasFlag( EntityFlags.SELECTABLE ) ) {
                                Bounds bounds = ent.CaclEntityBounds();
                                if( Intersections.RectVsBounds( selectionRect, bounds ) && ent.playerNumber == playerNumber ) {
                                    map.selection.Add( ent.id );
                                    ent.selected = true;
                                }
                                else {
                                    ent.selected = false;
                                }
                            }
                        }
                    }
                    else {
                        map.selection.Clear();
                        for( int i = 0; i < map.entities.Length; i++ ) {
                            Entity ent = map.entities[i];
                            if( ent != null && ent.flags.HasFlag( EntityFlags.SELECTABLE ) ) {
                                Bounds bounds = ent.CaclEntityBounds();
                                if( bounds.ContainsPoint( mouseWorld ) ) {
                                    map.selection.Add( ent.id );
                                    ent.selected = true;
                                }
                                else {
                                    ent.selected = false;
                                }
                            }
                        }
                    }
                }

                if( Raylib.IsMouseButtonReleased( MouseButton.MOUSE_RIGHT_BUTTON ) ) {
                    if( map.selection.Count > 0 ) {

                        Entity? entUnderMouse = null;
                        for( int i = 0; i < map.entities.Length; i++ ) {
                            Entity ent = map.entities[i];
                            if( ent != null ) {
                                Bounds bounds = ent.CaclEntityBounds();
                                if( bounds.ContainsPoint( mouseWorld ) ) {
                                    entUnderMouse = ent;
                                    break;
                                }
                            }
                        }

                        if( entUnderMouse == null ) {
                            MapActionSelectionMove action = new MapActionSelectionMove();
                            action.entityIds = map.selection.ToArray();
                            action.target = mouseWorldFp;
                            localTurn.actions.Add( action );
                        }
                        else if( entUnderMouse.type == EntityType.RESOURCE_R1 ) {
                            MapActionSelectionGatherResource action = new MapActionSelectionGatherResource();
                            action.entityIds = map.selection.ToArray();
                            action.resourceNodeId = entUnderMouse.id;
                            localTurn.actions.Add( action );
                        }
                    }
                }
            }

            stopWatch.Stop();
            //TimeSpan ts = stopWatch.Elapsed;
            //Console.WriteLine("RunTime " + ts.TotalMilliseconds);

            //map.TileWorldPosToIndices(mouseWorldFp, out int tileX, out int tileY);
            //Console.WriteLine($"MouseWorld: {mouseWorldFp} Tile: {tileX}, {tileY}");

            Raylib.BeginTextureMode( surface );
            Raylib.BeginMode2D( camera );
            Raylib.ClearBackground( Color.RAYWHITE );

            if( false ) {
                for( int y = 0; y < Map.MAP_TILE_H_COUNT; y++ ) {
                    for( int x = 0; x < Map.MAP_TILE_W_COUNT; x++ ) {
                        MapTile tile = map.tiles[x, y];
                        Rectangle r = new Rectangle(tile.visPos.X + 2, tile.visPos.Y + 2,
                            MapTile.WORLD_WIDTH_UNITS - 2, MapTile.WORLD_HEIGHT_UNITS - 2
                        );
                        Color c = tile.isWalkable ? Color.GREEN : Color.RED;
                        Raylib.DrawRectangleRec( r, Raylib.Fade( c, 0.7f ) );
                        //DrawText(TextFormat("%d", tile->flatIndex), (i32)tile->visPos.x, (i32)tile->visPos.y, 10, BLACK);
                    }
                }
            }

            for( int i = 0; i < map.entities.Length; i++ ) {
                Entity ent = map.entities[i];
                if( ent != null ) {
                    if( ent.type == EntityType.GENERAL ) {
                        if( ent.selected == true ) {
                            Raylib.DrawCircleV( ent.visPos, 13, Color.GREEN );
                        }
                        Color c = ent.playerNumber == 1 ? Color.BLUE : Color.RED;
                        Raylib.DrawCircleV( ent.visPos, 10, c );
                    }
                    else if( ent.type == EntityType.BUILDINGS_TOWN_CENTER ) {
                        int w = ent.widthInTiles * MapTile.WORLD_WIDTH_UNITS;
                        int h = ent.heightInTiles * MapTile.WORLD_HEIGHT_UNITS;
                        Rectangle r = new Rectangle(ent.visPos.X, ent.visPos.Y, w, h);
                        Color c = ent.playerNumber == 1 ? Color.BLUE : Color.RED;
                        Raylib.DrawRectangleRec( r, c );
                        if( ent.selected == true ) {
                            Raylib.DrawRectangleLinesEx( r, 2, Color.GREEN );
                        }
                    }
                    else if( ent.type == EntityType.RESOURCE_R1 ) {
                        int w = ent.widthInTiles * MapTile.WORLD_WIDTH_UNITS;
                        int h = ent.heightInTiles * MapTile.WORLD_HEIGHT_UNITS;
                        Rectangle r = new Rectangle(ent.visPos.X, ent.visPos.Y, w, h);
                        Color c = Color.ORANGE;
                        Raylib.DrawRectangleRec( r, c );
                    }
                    else if( ent.type == EntityType.RESOURCE_R2 ) {
                        int w = ent.widthInTiles * MapTile.WORLD_WIDTH_UNITS;
                        int h = ent.heightInTiles * MapTile.WORLD_HEIGHT_UNITS;
                        Rectangle r = new Rectangle(ent.visPos.X, ent.visPos.Y, w, h);
                        Color c = Color.PURPLE;
                        Raylib.DrawRectangleRec( r, c );
                    }
                }
            }

            if( dragging == true ) {
                Vector2 topLeft = Vector2.Zero;
                topLeft.X = MathF.Min( startDrag.X, endDrag.X );
                topLeft.Y = MathF.Min( startDrag.Y, endDrag.Y );
                Vector2 bottomRight = Vector2.Zero;
                bottomRight.X = MathF.Max( startDrag.X, endDrag.X );
                bottomRight.Y = MathF.Max( startDrag.Y, endDrag.Y );
                Rectangle rec = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                Raylib.DrawRectangleRec( rec, Raylib.Fade( Color.GREEN, 0.5f ) );
            }

            Raylib.EndMode2D();
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            Raylib.ClearBackground( Color.WHITE );

            Rectangle r1 = new Rectangle(0.0f, 0.0f, (float)surface.texture.width, (float)-surface.texture.height);
            Rectangle r2 = new Rectangle();
            r2.x = ( Raylib.GetScreenWidth() - ( (float)surfaceWidth * scale ) ) * 0.5f;
            r2.y = ( Raylib.GetScreenHeight() - ( (float)surfaceHeight * scale ) ) * 0.5f;
            r2.width = (float)surfaceWidth * scale;
            r2.height = (float)surfaceHeight * scale;
            Raylib.DrawTexturePro( surface.texture, r1, r2, new Vector2(), 0.0f, Color.WHITE );

            uiState.Draw();

            Raylib.EndDrawing();

            GameScreen nextScreen = this;
            return nextScreen;
        }

        public void DoCameraPanning() {
            float speed = 10.0f;
            if( Raylib.IsKeyDown( KeyboardKey.KEY_W ) ) {
                camera.target.Y -= speed;
            }
            if( Raylib.IsKeyDown( KeyboardKey.KEY_S ) ) {
                camera.target.Y += speed;
            }
            if( Raylib.IsKeyDown( KeyboardKey.KEY_A ) ) {
                camera.target.X -= speed;
            }
            if( Raylib.IsKeyDown( KeyboardKey.KEY_D ) ) {
                camera.target.X += speed;
            }
        }

        public void DoCameraZooming() {
            float speed = 0.05f;
            if( Raylib.IsKeyDown( KeyboardKey.KEY_Q ) ) {
                camera.zoom -= speed;
            }
            if( Raylib.IsKeyDown( KeyboardKey.KEY_E ) ) {
                camera.zoom += speed;
            }
        }
    }

    public class AttribBoyo : Attribute {

    }

    public class Something {
        [AttribBoyo]
        public int x;
        [AttribBoyo]
        public int y;
    }


    public class Game {
        public static GameScreen screen = new SreenMainMenu();

        public static void Main( string[] args ) {

            TypeInfo typeInfo = typeof(Something).GetTypeInfo();
            Console.WriteLine( "The assembly qualified name of MyClass is " + typeInfo.AssemblyQualifiedName );
            var fs = typeInfo.GetFields();
            foreach( var f in fs ) {
                var attrs = f.GetCustomAttributes();
                foreach( var attr in attrs )
                    Console.WriteLine( "Attribute on MyClass: " + attr.GetType().Name );
            }


            for( int i = 0; i < args.Length; i++ ) {
                if( args[i] == "-settings" ) {
                    if( i + 1 < args.Length ) {
                        string settingsText = File.ReadAllText(args[i + 1]);
                        if( settingsText.Length != 0 ) {
                            GameSettings? newSettings = JsonSerializer.Deserialize<GameSettings>(settingsText);
                            if( newSettings != null ) {
                                GameSettings.Current = newSettings;
                            }
                            else {
                                Console.WriteLine( "Error: Failed to parse settings file" );
                                return;
                            }
                        }
                        else {
                            Console.WriteLine( "Error: No settings file specified" );
                            return;
                        }
                        i++;
                    }
                    else {
                        Console.WriteLine( "Error: No settings file specified" );
                        return;
                    }
                }
            }

            GameSettings settings = GameSettings.Current;
            Raylib.SetConfigFlags( ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_VSYNC_HINT );
            Raylib.InitWindow( settings.WindowWidth, settings.WindowHeight, "Game" );
            if( settings.WindowPosX != -1 && settings.WindowPosY != -1 ) {
                Raylib.SetWindowPosition( settings.WindowPosX, settings.WindowPosY );
            }
            else {
                Raylib.SetWindowPosition( ( Raylib.GetMonitorWidth( 0 ) - settings.WindowWidth ) / 2, ( Raylib.GetMonitorHeight( 0 ) - settings.WindowHeight ) / 2 );
            }

            Raylib.SetTargetFPS( 60 );
            Raylib.InitAudioDevice();

            while( !Raylib.WindowShouldClose() ) {
                screen = screen.Update();
            }

            Raylib.CloseWindow();
        }
    }
}
