using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game.Content;

namespace Game {

    public class FontBakerResult {
        public FontBakerResult( Dictionary<int, FontGlyphInfo> glyphs, byte[] bitmap, int width, int height ) {
            if ( glyphs == null )
                throw new ArgumentNullException( nameof( glyphs ) );

            if ( bitmap == null )
                throw new ArgumentNullException( nameof( bitmap ) );

            if ( width <= 0 )
                throw new ArgumentOutOfRangeException( nameof( width ) );

            if ( height <= 0 )
                throw new ArgumentOutOfRangeException( nameof( height ) );

            if ( bitmap.Length < width * height )
                throw new ArgumentException( "pixels.Length should be higher than width * height" );

            Glyphs = glyphs;
            Bitmap = bitmap;
            Width = width;
            Height = height;
        }

        public Dictionary<int, FontGlyphInfo> Glyphs { get; }

        public byte[] Bitmap { get; }

        public int Width { get; }

        public int Height { get; }
    }
}
