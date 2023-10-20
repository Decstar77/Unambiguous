namespace Game {
    public struct FontCharacterRange {
        public static readonly FontCharacterRange BasicLatin = new FontCharacterRange(0x0020, 0x007F);
        public static readonly FontCharacterRange Latin1Supplement = new FontCharacterRange(0x00A0, 0x00FF);
        public static readonly FontCharacterRange LatinExtendedA = new FontCharacterRange(0x0100, 0x017F);
        public static readonly FontCharacterRange LatinExtendedB = new FontCharacterRange(0x0180, 0x024F);
        public static readonly FontCharacterRange Cyrillic = new FontCharacterRange(0x0400, 0x04FF);
        public static readonly FontCharacterRange CyrillicSupplement = new FontCharacterRange(0x0500, 0x052F);
        public static readonly FontCharacterRange Hiragana = new FontCharacterRange(0x3040, 0x309F);
        public static readonly FontCharacterRange Katakana = new FontCharacterRange(0x30A0, 0x30FF);
        public static readonly FontCharacterRange Greek = new FontCharacterRange(0x0370, 0x03FF);
        public static readonly FontCharacterRange CjkSymbolsAndPunctuation = new FontCharacterRange(0x3000, 0x303F);
        public static readonly FontCharacterRange CjkUnifiedIdeographs = new FontCharacterRange(0x4e00, 0x9fff);
        public static readonly FontCharacterRange HangulCompatibilityJamo = new FontCharacterRange(0x3130, 0x318f);
        public static readonly FontCharacterRange HangulSyllables = new FontCharacterRange(0xac00, 0xd7af);

        public int Start { get; }

        public int End { get; }

        public int Size => End - Start + 1;

        public FontCharacterRange( int start, int end ) {
            Start = start;
            End = end;
        }

        public FontCharacterRange( int single ) : this( single, single ) {
        }
    }
}
