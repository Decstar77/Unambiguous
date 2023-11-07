using FontStashSharp;
using SoLoud;

namespace Game
{
    public partial class Content {
        //public static string BasePath = "C:/Projects/CS/Mage/Content/";
        public static string BasePath = "Content/";

        private static FontSystem fontSystem = new FontSystem();

        public static string ResolvePath( string path ) {
            return BasePath + path;
        }

        public static SpriteTexture LoadSpriteTexture( string textureId ) {
            string fullpath = BasePath + "sprites/" +textureId;
            byte[] fileData = File.ReadAllBytes( fullpath );
            if ( fileData == null || fileData.Length == 0 ) {
                return null;
            }

            StbiSharp.Stbi.SetFlipVerticallyOnLoad( false );
            StbiSharp.StbiImage image = StbiSharp.Stbi.LoadFromMemory( new Span<byte> (fileData), 4 );

            bool shouldCreatePaddedAlphaBorder = false;

            for ( int i = 0; i < image.Data.Length; i += 4 ) {
                byte r = image.Data[i];
                byte g = image.Data[i + 1];
                byte b = image.Data[i + 2];
                byte a = image.Data[i + 3];

                if ( IsImageBorder( image.Width, image.Height, i ) && a == 0 ) {
                    shouldCreatePaddedAlphaBorder = true;
                    break;
                }
            }

            int width = image.Width;
            int height = image.Height;
            SpriteTexture spriteTexture;

            if ( shouldCreatePaddedAlphaBorder ) {
                width = image.Width + 2;
                height = image.Height + 2;
                int paddedSize = width * height * 4;
                byte[] paddedData = new byte[ paddedSize ];
                for ( int i = 0; i < paddedSize; i += 4 ) {
                    if ( IsImageBorder( width, height, i ) == false ) {
                        int x = i / 4 % width - 1;
                        int y = i / 4 / width - 1;
                        int index = x + y * image.Width;
                        byte r = image.Data[index * 4];
                        byte g = image.Data[index * 4 + 1];
                        byte b = image.Data[index * 4 + 2];
                        byte a = image.Data[index * 4 + 3];
                        paddedData[i] = r;
                        paddedData[i + 1] = g;
                        paddedData[i + 2] = b;
                        paddedData[i + 3] = a;
                    }
                }

                spriteTexture = new SpriteTexture( width, height, image.NumChannels, paddedData );
            }
            else {
                byte[] data = image.Data.ToArray();
                spriteTexture = new SpriteTexture( width, height, image.NumChannels, data );
            }

            image.Dispose();

            return spriteTexture;
        }

        private static bool IsImageBorder( int w, int h, int i ) {
            int x = i / 4 % w;
            int y = i / 4 / w;
            if ( x == 0 || y == 0 || x == w - 1 || y == h - 1 ) {
                return true;
            }

            return false;
        }

        public static void LoadFonts() {
            fontSystem.AddFont( File.ReadAllBytes( ResolvePath( "fonts/DroidSans.ttf" ) ) );
        }

        public static DynamicSpriteFont GetDefaultFont() {
            DynamicSpriteFont font = fontSystem.GetFont( 32 );
            return font;
        }

        private static Dictionary<string, SoloudObject> sounds = new Dictionary<string, SoloudObject>();
        public static SoloudObject LoadWav( string path ) {
            sounds.TryGetValue( path, out SoloudObject sound );
            if ( sound != null ) {
                return sound;
            }

            string fullpath = BasePath + "sounds/" + path;
            SoLoud.Wav wav = new SoLoud.Wav();
            wav.Load( fullpath );

            sounds.Add( path, wav );

            return wav;
        }
    }
}
