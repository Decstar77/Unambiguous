using OpenTK.Mathematics;

namespace Game {
    public struct DrawTriangle {
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;
    }

    public static class Colors {
        public static Vector4 WHITE = new Vector4(1, 1, 1, 1);
    }

    public enum DrawCommandType {
        NONE = 0,
        CIRCLE,
        RECT,
        SPRITE,
        TEXT,
        LINE,
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

        public SpriteTexture?       spriteTexture;

        public int                  gridLevel; // In the ISO grid
        public Vector2              vanishingPoint;

        public string               text;
    }

    public class DrawCommands {
        public List<DrawCommand> commands = new List<DrawCommand>();

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

        public void DrawRect( Vector2 bl, Vector2 tr ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.RECT;
            cmd.color = Colors.WHITE;
            cmd.bl = bl;
            cmd.br = new Vector2( tr.X, bl.Y );
            cmd.tr = tr;
            cmd.tl = new Vector2( bl.X, tr.Y );
            commands.Add( cmd );
        }

        public void DrawBox( BoxCollider box ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.RECT;
            cmd.color = Colors.WHITE;
            box.GetVerts( out cmd.bl, out cmd.br, out cmd.tr, out cmd.tl );
            commands.Add( cmd );
        }

        public void DrawPolyCollider( PolyCollider polyCollider ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.LINE;
            cmd.color = Colors.WHITE;
            cmd.verts = polyCollider.verts;
            commands.Add( cmd );
        }

        public void DrawText( string text, Vector2 p ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.TEXT;
            cmd.color = Colors.WHITE;
            cmd.text = text;
            cmd.c = p;
            commands.Add( cmd );
        }

        public void RenderDrawRect( Vector2 center, Vector2 dim, float rot ) {
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

        public void RenderDrawSprite( SpriteTexture texture, Vector2 center, float rot, Vector2 size ) {
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

            cmd.c = center;

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

            cmd.c = center;

            cmd.vanishingPoint = vanishingPoint;
            cmd.gridLevel = level;

            commands.Add( cmd );
        }
    }
}
