
using System.Numerics;

namespace Game {
    public class Font {
        public FontTexture texture;
        public Dictionary<int, FontGlyphInfo> glyphs;
        public int fontSize;
        
        public Font(FontTexture fontTexture, Dictionary<int, FontGlyphInfo> glyphs, int fontSize ) {
            this.texture = fontTexture;
            this.glyphs = glyphs;
            this.fontSize = fontSize;
        }

        //https://github.com/raysan5/raylib/blob/18bedbd0952c27b0eb8bc5df0df4acf589cef181/src/rtext.c#L1216
        public Vector2 MeasureText( string text ) {
            float textWidth = 0;
            float textHeight = fontSize;

            foreach (char c in text ) {
                int codePoint = (int)c;
                if ( glyphs.ContainsKey( codePoint ) ) {
                    FontGlyphInfo info = glyphs[codePoint];

                    if (info.xAdvance != 0) {
                        textWidth += info.xAdvance;
                    } else {
                        textWidth += info.width + info.xOffset;
                    }
                }
            }

            return new Vector2( textWidth, textHeight );
        }
    }
}
