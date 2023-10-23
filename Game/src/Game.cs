using OpenTK.Windowing.Desktop;
using System.Text.Json;
namespace Game {

    public class GameSettings {
        public static GameSettings Current = new GameSettings();
        public int ServerPort { get; set; } = 27164;
        public string ServerIp { get; set; } = "127.0.0.1";
        public int WindowWidth { get; set; } = 1280;
        public int WindowHeight { get; set; } = 720;
        public int WindowPosX { get; set; } = -1;
        public int WindowPosY { get; set; } = -1;
    }

    public class Game {

        public static void Main( string[] args ) {
            ParseArgs( args );
            Engine engine = new Engine(1280, 720, "Huh ?");
            engine.Run();
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
