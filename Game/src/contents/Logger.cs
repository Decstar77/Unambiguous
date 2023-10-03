
namespace Game {
    public class Logger {
        public static bool StreamToFile = false;
        public static string FileName = $"log_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt";
        public static StreamWriter sw = null;

        public static void Log( string message ) {
            if( StreamToFile ) {
                if( sw == null ) {
                    sw = new StreamWriter( FileName );
                }
                sw.WriteLine( message );
                sw.Flush();
            }
            else {
                Console.WriteLine( message );
            }
        }
    }
}
