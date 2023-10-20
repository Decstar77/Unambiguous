using GLFW;
using OpenGL;
using System.Numerics;
using System.Text.Json;
namespace Game {

    public interface GameScreen {
        public GameScreen Update();
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

        public virtual GameScreen Update() {
            return this;
        }
    }

    public class ScreenGame : GameScreen {
        // Todo: Put this in a game local class!
        public bool initialized = false;


        public virtual GameScreen Update() {
            if ( initialized == false ) {

            }

            return this;
        }

    }

    public class AttribBoyo : Attribute {

    }


    public class Game {
        public static GameScreen screen = new SreenMainMenu();

        public static void Main( string[] args ) {
            ParseArgs( args );

            Engine.Init();

            SpriteTexture? sprTile = Content.LoadSpriteTexture( "tile_test.png" );
            if ( sprTile == null ) {
                return;
            }

            GameCode code = new GameCode();
            code.Init();

            int tickRate = 60;
            float tickTime = 1.0f / tickRate;
            float timeAccumc = 0.0f;

            double nowTime = Glfw.Time;
            while ( Engine.Poll() ) {

                double newTime = Glfw.Time;
                double deltaTime = newTime - nowTime;
                nowTime = newTime;

                timeAccumc += (float)deltaTime;
                while ( timeAccumc > tickTime ) {
                    timeAccumc -= tickTime;
                    code.UpdateTick( tickTime );
                }

                //DrawCommands drawCommands = new DrawCommands();
                //drawCommands.RenderDrawSprite( sprTile, new Vector2( 0, 0 ), 0, new Vector2( 1, 1 ) );
                //drawCommands.RenderDrawSprite( sprMan, new Vector2( 0, 0 ), 0, new Vector2( 1, 1 ) );

                GL.glClearColor( 0.2f, 0.3f, 0.3f, 1.0f );
                GL.glClear( GL.GL_COLOR_BUFFER_BIT );
                //Engine.SubmitDrawCommands( drawCommands );
                code.UpdateRender( (float)deltaTime );

                Glfw.SwapBuffers( Engine.window );
            }

            Glfw.Terminate();
        }

        public static void ParseArgs( string[] args ) {
            for ( int i = 0; i < args.Length; i++ ) {
                if ( args[i] == "-settings" ) {
                    if ( i + 1 < args.Length ) {
                        string settingsText = File.ReadAllText(args[i + 1]);
                        if ( settingsText.Length != 0 ) {
                            GameSettings? newSettings = JsonSerializer.Deserialize<GameSettings>(settingsText);
                            if ( newSettings != null ) {
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
        }
    }
}
