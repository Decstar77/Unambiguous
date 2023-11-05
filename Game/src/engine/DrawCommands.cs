using OpenTK.Mathematics;

namespace Game {
    public struct DrawTriangle {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
    }

    public static class Colors {
        public static Vector4 WHITE = new Vector4(1, 1, 1, 1);
        public static Vector4 BLACK = new Vector4(0, 0, 0, 1);
        public static Vector4 RED = new Vector4(0.9f, 0, 0, 1);
        public static Vector4 GREEN = new Vector4(0, 0.9f, 0, 1);
        public static Vector4 BLUE = new Vector4(0, 0, 0.9f, 1);
        public static Vector4 YELLOW = new Vector4(0.9f, 0.9f, 0, 1);
        public static Vector4 CYAN = new Vector4(0, 0.9f, 0.9f, 1);
        public static Vector4 MAGENTA = new Vector4(0.9f, 0, 0.9f, 1);
        public static Vector4 SKY_BLUE = new Vector4(0.5f, 0.5f, 1, 1);
        public static Vector4 PETER_RIVER = FromHex( "#3498db" );
        public static Vector4 AMETHYST = FromHex( "#9b59b6" );
        public static Vector4 MIDNIGHT_BLUE = FromHex( "#2c3e50" );
        public static Vector4 SUN_FLOWER = FromHex( "#f1c40f" );
        public static Vector4 CARROT = FromHex( "#e67e22" );
        public static Vector4 ALIZARIN = FromHex( "#e74c3c" );
        public static Vector4 SILVER = FromHex("#bdc3c7");

        private static Vector4 FromHex( string h ) {
            h = h.Replace( "#", "" );
            byte r = byte.Parse( h.Substring( 0, 2 ), System.Globalization.NumberStyles.HexNumber );
            byte g = byte.Parse( h.Substring( 2, 2 ), System.Globalization.NumberStyles.HexNumber );
            byte b = byte.Parse( h.Substring( 4, 2 ), System.Globalization.NumberStyles.HexNumber );
            return new Vector4( r / 255.0f, g / 255.0f, b / 255.0f, 1 );
        }
    }

    public enum DrawCommandType {
        NONE = 0,
        CIRCLE,
        RECT,
        SPRITE,
        TEXT,
        LINE,
        TRIANGLES,

        SCREEN_RECT
    };

    public struct DrawCommand {
        public DrawCommandType       type;
        public Vector4               color;

        public Vector2              c;
        public float                r;

        public Vector2              tl;
        public Vector2              tr;
        public Vector2              bl;
        public Vector2              br;

        public Vector2[]            verts;

        public SpriteTexture       spriteTexture;

        public int                  gridLevel; // In the ISO grid
        public Vector2              vanishingPoint;

        public bool                 center;
        public string               text;
    }

    public class DrawCommands {
        public List<DrawCommand> commands = new List<DrawCommand>();
        public Vector4 currentColor = Colors.WHITE;

        public void Clear() {
            commands.Clear();
        }

        public void DrawCircle( Vector2 pos, float radius ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.CIRCLE;
            cmd.color = Colors.WHITE;
            cmd.c = pos;
            cmd.r = radius;
            commands.Add( cmd );
        }

        public void DrawCircle( CircleCollider circle ) {
            DrawCircle( circle.center, circle.radius );
        }

        public void DrawRect( Vector2 bl, Vector2 tr, Vector4 color ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.RECT;
            cmd.color = color;
            cmd.bl = bl;
            cmd.br = new Vector2( tr.X, bl.Y );
            cmd.tr = tr;
            cmd.tl = new Vector2( bl.X, tr.Y );
            commands.Add( cmd );
        }

        public void DrawRect( RectBounds rect, Vector4 color ) { DrawRect( rect.min, rect.max, color ); }
        public void DrawRect( Vector2 bl, Vector2 tr ) { DrawRect( bl, tr, Colors.WHITE ); }
        public void DrawRect( RectBounds rect ) { DrawRect( rect, Colors.WHITE ); }

        public void DrawScreenRect( Vector2 bl, Vector2 tr ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.SCREEN_RECT;
            cmd.color = currentColor;
            cmd.bl = bl;
            cmd.br = new Vector2( tr.X, bl.Y );
            cmd.tr = tr;
            cmd.tl = new Vector2( bl.X, tr.Y );
            commands.Add( cmd );
        }

        public void DrawScreenRect( RectBounds rect ) {
            DrawScreenRect( rect.min, rect.max );
        }

        public void DrawBox( BoxCollider box ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.RECT;
            cmd.color = Colors.WHITE;
            box.GetVerts( out cmd.bl, out cmd.br, out cmd.tr, out cmd.tl );
            commands.Add( cmd );
        }

        public void DEBUG_DrawConvexCollider( ConvexCollider polyCollider, Vector4 color ) {
            List<Vector2> verts = polyCollider.Triangulate();
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.TRIANGLES;
            cmd.color = color;
            cmd.verts = verts.ToArray();
            commands.Add( cmd );
        }

        public void DEBUG_DrawConvexCollider( ConvexCollider polyCollider ) {
            DEBUG_DrawConvexCollider( polyCollider, Colors.WHITE );
        }

        public void DrawText( string text, Vector2 p, bool center = false ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.TEXT;
            cmd.color = Colors.WHITE;
            cmd.text = text;
            cmd.c = p;
            cmd.center = center;
            commands.Add( cmd );
        }

        public void DrawRect( Vector2 center, Vector2 dim, float rot ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.RECT;
            cmd.color = Colors.WHITE;

            cmd.bl = -dim / 2.0f;
            cmd.tr = dim / 2.0f;
            cmd.br = new Vector2( cmd.tr.X, cmd.bl.Y );
            cmd.tl = new Vector2( cmd.bl.X, cmd.tr.Y );

            Matrix2 rotationMatrix = Matrix2.CreateRotation( rot );
            cmd.bl = rotationMatrix * cmd.bl;
            cmd.tr = rotationMatrix * cmd.tr;
            cmd.br = rotationMatrix * cmd.br;
            cmd.tl = rotationMatrix * cmd.tl;

            cmd.bl += center;
            cmd.tr += center;
            cmd.br += center;
            cmd.tl += center;

            commands.Add( cmd );
        }

        public void DrawSprite( SpriteTexture texture, Vector2 center, float rot, Vector2 size ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.SPRITE;
            cmd.color = Colors.WHITE;
            cmd.spriteTexture = texture;

            Vector2 dim = new Vector2(texture.width, texture.height) * size;
            cmd.bl = -dim / 2.0f;
            cmd.tr = dim / 2.0f;
            cmd.br = new Vector2( cmd.tr.X, cmd.bl.Y );
            cmd.tl = new Vector2( cmd.bl.X, cmd.tr.Y );

            Matrix2 rotationMatrix = Matrix2.CreateRotation( rot );
            cmd.bl = rotationMatrix * cmd.bl;
            cmd.tr = rotationMatrix * cmd.tr;
            cmd.br = rotationMatrix * cmd.br;
            cmd.tl = rotationMatrix * cmd.tl;

            cmd.bl += center;
            cmd.tr += center;
            cmd.br += center;
            cmd.tl += center;

            commands.Add( cmd );
        }

        public void DrawSprite( SpriteTexture texture, Vector2 center, float rot, int level, Vector2 vanishingPoint ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.SPRITE;
            cmd.color = Colors.WHITE;
            cmd.spriteTexture = texture;

            Vector2 dim = new Vector2(texture.width, texture.height);
            cmd.bl = -dim / 2.0f;
            cmd.tr = dim / 2.0f;
            cmd.br = new Vector2( cmd.tr.X, cmd.bl.Y );
            cmd.tl = new Vector2( cmd.bl.X, cmd.tr.Y );

            Matrix2 rotationMatrix = Matrix2.CreateRotation( rot );
            cmd.bl = rotationMatrix * cmd.bl;
            cmd.tr = rotationMatrix * cmd.tr;
            cmd.br = rotationMatrix * cmd.br;
            cmd.tl = rotationMatrix * cmd.tl;

            cmd.bl += center;
            cmd.tr += center;
            cmd.br += center;
            cmd.tl += center;

            cmd.vanishingPoint = vanishingPoint;
            cmd.gridLevel = level;

            commands.Add( cmd );
        }

        public void DrawSpriteTL( SpriteTexture texture, Vector2 tl, int level, Vector2 vanishingPoint ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.SPRITE;
            cmd.color = Colors.WHITE;
            cmd.spriteTexture = texture;

            Vector2 dim = new Vector2(texture.width, texture.height);
            cmd.tl = tl;
            cmd.tr = tl + new Vector2( dim.X, 0 );
            cmd.bl = tl + new Vector2( 0, dim.Y );
            cmd.br = tl + dim;

            cmd.vanishingPoint = vanishingPoint;
            cmd.gridLevel = level;

            commands.Add( cmd );
        }
    }
}
