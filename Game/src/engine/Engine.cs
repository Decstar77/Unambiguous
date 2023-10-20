﻿using GLFW;
using OpenAL;
using System;
using System.Numerics;
using static OpenGL.GL;
using static System.Net.Mime.MediaTypeNames;

namespace Game {

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

        public void DrawText( string text, Vector2 bottomLeft, Font font ) {
            DrawCommand cmd = new DrawCommand();
            cmd.type = DrawCommandType.TEXT;
            cmd.color = Colors.WHITE;
            cmd.bl = bottomLeft;
            cmd.c = font.MeasureText( text );
            cmd.text = text;
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

            //Mat2 rotationMatrix = new Matrix3x2 (MathF.Cos(rot), -MathF.Sin(rot), MathF.Sin(rot), MathF.Cos(rot));
            Mat2 rotationMatrix = Mat2.Rotation( rot );

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

            Mat2 rotationMatrix = Mat2.Rotation(rot);
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

            Mat2 rotationMatrix = Mat2.Rotation(rot);
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

    public struct Camera {
        public float    width;
        public float    height;
        public Vector2  pos;
    }

    public class VertexBuffer {
        public uint     vao;
        public uint     vbo;
        public int      size;
        public int      count;
        public int      stride;
        public bool     dynamic;
    }

    public static class Engine {
        public static EngineInput input = new EngineInput();
        public static float surfaceWidth = 0;
        public static float surfaceHeight = 0;
        public static Vector4 viewport = new Vector4( 0, 0, 0, 0 );
        public static Camera camera = new Camera();
        public static Window window;
        private static SizeCallback framebufferSizeCallback;
        private static KeyCallback  keyCallback;
        public static Matrix4x4 screenProjection;
        public static Matrix4x4 cameraProjection;
        public static ShaderProgram shapeProgram = null;
        public static VertexBuffer  shapeBuffer = null;
        public static ShaderProgram spriteProgram = null;
        public static VertexBuffer  spriteBuffer = null;
        public static ShaderProgram fontProgram = null;
        public static VertexBuffer  fontBuffer = null;
        public static IntPtr alDevice;
        public static IntPtr alContext;
        public static Font font = null;

        public static void Init() {
            Glfw.WindowHint( Hint.ClientApi, ClientApi.OpenGL );
            Glfw.WindowHint( Hint.ContextVersionMajor, 3 );
            Glfw.WindowHint( Hint.ContextVersionMinor, 3 );
            Glfw.WindowHint( Hint.OpenglProfile, Profile.Core );
            Glfw.WindowHint( Hint.Doublebuffer, true );
            Glfw.WindowHint( Hint.Decorated, true );
            Glfw.WindowHint( Hint.Resizable, true );

            window = Glfw.CreateWindow(
                GameSettings.Current.WindowWidth,
                GameSettings.Current.WindowHeight,
                "TItle",
                GLFW.Monitor.None,
                Window.None
            );

            framebufferSizeCallback = ( _, w, h ) => OnFrameBufferSizeCallback( w, h );
            keyCallback = ( _, key, code, state, mods ) => OnKeyCallback( key, code, state, mods );

            Glfw.SetFramebufferSizeCallback( window, framebufferSizeCallback );
            Glfw.SetKeyCallback( window, keyCallback );

            // Center window
            var screen = Glfw.PrimaryMonitor.WorkArea;
            var x = (screen.Width - GameSettings.Current.WindowWidth ) / 2;
            var y = (screen.Height - GameSettings.Current.WindowHeight ) / 2;
            Glfw.SetWindowPosition( window, x, y );

            Glfw.MakeContextCurrent( window );
            Import( Glfw.GetProcAddress );

            int width, height;
            Glfw.GetFramebufferSize( window, out width, out height );

            Glfw.SwapInterval( 1 );

            surfaceWidth = (float)width;
            surfaceHeight = (float)height;

            //camera.width = 240;
            //camera.height = 135;
            camera.width = 480;
            camera.height = 270;

            ResetSurface( width, height );

            font = Content.LoadFonts();

            InitShapeRendering();
            InitSpriteRendering();
            InitFontRendering();

            alDevice = ALC10.alcOpenDevice( null );
            if ( alDevice == IntPtr.Zero ) {
                Console.WriteLine( "Failed to open audio device" );
                return;
            }

            alContext = ALC10.alcCreateContext( alDevice, null );
            if ( alContext == IntPtr.Zero ) {
                Console.WriteLine( "Failed to create audio context" );
                return;
            }

            ALC10.alcMakeContextCurrent( alContext );

            if ( ALC10.alcGetError( alDevice ) == ALC10.ALC_NO_ERROR ) {
                Console.WriteLine( "Audio context created" );
            }
            else {
                Console.WriteLine( "Failed to create audio context" );
            }
        }

        public static void SubmitDrawCommands( DrawCommands commands ) {
            foreach ( DrawCommand cmd in commands.commands ) {
                switch ( cmd.type ) {
                    case DrawCommandType.CIRCLE: {
                        float x1 = cmd.c.X - cmd.r - camera.pos.X;
                        float y1 = cmd.c.Y - cmd.r - camera.pos.Y;
                        float x2 = cmd.c.X + cmd.r - camera.pos.X;
                        float y2 = cmd.c.Y + cmd.r - camera.pos.Y;

                        Vector4 tl = Vector4.Transform( new Vector4( x1, y2, 0.0f, 1.0f ), cameraProjection );
                        Vector4 bl = Vector4.Transform( new Vector4( x1, y1, 0.0f, 1.0f ), cameraProjection );
                        Vector4 br = Vector4.Transform( new Vector4( x2, y1, 0.0f, 1.0f ), cameraProjection );
                        Vector4 tr = Vector4.Transform( new Vector4( x2, y2, 0.0f, 1.0f ), cameraProjection );

                        float[] vertices = new float[]{
                            tl.X, tl.Y,
                            bl.X, bl.Y,
                            br.X, br.Y,
                            tl.X, tl.Y,
                            br.X, br.Y,
                            tr.X, tr.Y
                        };

                        Vector2 c = WorldPosToScreenPos( cmd.c );
                        float r = WorldLengthToScreenLength( cmd.r ) - 2.0f;

                        shapeProgram.Use();
                        shapeProgram.SetUniformInt( "mode", 1 );
                        shapeProgram.SetUniformVec4( "color", cmd.color );
                        shapeProgram.SetUniformVec4( "shapePosAndSize", new Vector4( c.X, viewport.W - c.Y, 0, 0 ) );
                        shapeProgram.SetUniformVec4( "shapeRadius", new Vector4( r, 0, 0, 0 ) );
                        GLEnableAlphaBlending();
                        GLUpdateVertexBuffer( shapeBuffer, vertices );
                        GLDrawVertexBuffer( shapeBuffer );
                    }
                    break;
                    case DrawCommandType.RECT: {
                        Vector4 tl =  new Vector4( cmd.tl.X - camera.pos.X, cmd.tl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 bl =  new Vector4( cmd.bl.X - camera.pos.X, cmd.bl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 br =  new Vector4( cmd.br.X - camera.pos.X, cmd.br.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 tr =  new Vector4( cmd.tr.X - camera.pos.X, cmd.tr.Y - camera.pos.Y, 0.0f, 0.0f );

                        tl = Vector4.Transform( new Vector4( tl.X, tl.Y, 0.0f, 1.0f ), cameraProjection );
                        bl = Vector4.Transform( new Vector4( bl.X, bl.Y, 0.0f, 1.0f ), cameraProjection );
                        br = Vector4.Transform( new Vector4( br.X, br.Y, 0.0f, 1.0f ), cameraProjection );
                        tr = Vector4.Transform( new Vector4( tr.X, tr.Y, 0.0f, 1.0f ), cameraProjection );

                        float[] vertices = new float[]{
                            tl.X, tl.Y,
                            bl.X, bl.Y,
                            br.X, br.Y,
                            tl.X, tl.Y,
                            br.X, br.Y,
                            tr.X, tr.Y
                        };

                        shapeProgram.Use();
                        shapeProgram.SetUniformInt( "mode", 0 );
                        shapeProgram.SetUniformVec4( "color", cmd.color );
                        GLEnableAlphaBlending();
                        GLUpdateVertexBuffer( shapeBuffer, vertices );
                        GLDrawVertexBuffer( shapeBuffer );
                    }
                    break;
                    case DrawCommandType.SPRITE: {
                        Vector4 tl =  new Vector4( cmd.tl.X - camera.pos.X, cmd.tl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 bl =  new Vector4( cmd.bl.X - camera.pos.X, cmd.bl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 br =  new Vector4( cmd.br.X - camera.pos.X, cmd.br.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 tr =  new Vector4( cmd.tr.X - camera.pos.X, cmd.tr.Y - camera.pos.Y, 0.0f, 0.0f );

                        tl = Vector4.Transform( new Vector4( tl.X, tl.Y, 0.0f, 1.0f ), cameraProjection );
                        bl = Vector4.Transform( new Vector4( bl.X, bl.Y, 0.0f, 1.0f ), cameraProjection );
                        br = Vector4.Transform( new Vector4( br.X, br.Y, 0.0f, 1.0f ), cameraProjection );
                        tr = Vector4.Transform( new Vector4( tr.X, tr.Y, 0.0f, 1.0f ), cameraProjection );

                        float[] vertices = new float[]{
                            tl.X, tl.Y, 0.0f ,0.0f,
                            bl.X, bl.Y, 0.0f, 1.0f,
                            br.X, br.Y, 1.0f, 1.0f,
                            tl.X, tl.Y, 0.0f, 0.0f,
                            br.X, br.Y, 1.0f, 1.0f,
                            tr.X, tr.Y, 1.0f, 0.0f
                        };

                        spriteProgram.Use();
                        spriteProgram.SetUniformInt( "texture1", 0 );
                        cmd.spriteTexture?.Bind( 0 );

                        GLEnablePreMultipliedAlphaBlending();
                        GLUpdateVertexBuffer( spriteBuffer, vertices );
                        GLDrawVertexBuffer( spriteBuffer );
                    }
                    break;
                    case DrawCommandType.TEXT: {
                        Vector2 basePos = cmd.bl;
                        Vector2 textDims = cmd.c;

                        float textOffsetX = 0.0f;

                        for ( int i = 0; i < cmd.text.Length; i++ ) {
                            char c = cmd.text[i];

                            if ( font.glyphs.ContainsKey( c ) ) {
                                FontGlyphInfo info = font.glyphs[c];

                                if ( c != ' ' || c != '\t' ) {
                                    float px = basePos.X + textOffsetX;
                                    float py = basePos.Y;
                                    float w = info.width;
                                    float h = info.height;

                                    Vector4 bl =  new Vector4( px,           py,                0.0f, 0.0f );
                                    Vector4 tl =  new Vector4( px,           py + h,            0.0f, 0.0f );
                                    Vector4 br =  new Vector4( px + w,       py,                0.0f, 0.0f );
                                    Vector4 tr =  new Vector4( px + w,       py + h,            0.0f, 0.0f );

                                    tl = Vector4.Transform( new Vector4( tl.X, tl.Y, 0.0f, 1.0f ), cameraProjection );
                                    bl = Vector4.Transform( new Vector4( bl.X, bl.Y, 0.0f, 1.0f ), cameraProjection );
                                    br = Vector4.Transform( new Vector4( br.X, br.Y, 0.0f, 1.0f ), cameraProjection );
                                    tr = Vector4.Transform( new Vector4( tr.X, tr.Y, 0.0f, 1.0f ), cameraProjection );

                                    float[] vertices = new float[]{
                                        tl.X, tl.Y, 0.0f ,0.0f,
                                        bl.X, bl.Y, 0.0f, 1.0f,
                                        br.X, br.Y, 1.0f, 1.0f,
                                        tl.X, tl.Y, 0.0f, 0.0f,
                                        br.X, br.Y, 1.0f, 1.0f,
                                        tr.X, tr.Y, 1.0f, 0.0f
                                    };

                                    fontProgram.Use();
                                    fontProgram.SetUniformInt( "fontTexture", 0 );
                                    font.texture?.Bind( 0 );

                                    //GLEnableAlphaBlending();
                                    GLUpdateVertexBuffer( fontBuffer, vertices );
                                    GLDrawVertexBuffer( fontBuffer );
                                }

                                textOffsetX += info.xAdvance;
                            }
                        }

                        //Vector4 bl =  new Vector4( bottomLeft.X,                bottomLeft.Y,               0.0f, 0.0f );
                        //Vector4 tl =  new Vector4( bottomLeft.X,                bottomLeft.X + textDims.Y,  0.0f, 0.0f );
                        //Vector4 br =  new Vector4( bottomLeft.X + textDims.X,   bottomLeft.X,               0.0f, 0.0f );
                        //Vector4 tr =  new Vector4( bottomLeft.X + textDims.X,   bottomLeft.X + textDims.Y,  0.0f, 0.0f );

                        //tl = Vector4.Transform( new Vector4( tl.X, tl.Y, 0.0f, 1.0f ), cameraProjection );
                        //bl = Vector4.Transform( new Vector4( bl.X, bl.Y, 0.0f, 1.0f ), cameraProjection );
                        //br = Vector4.Transform( new Vector4( br.X, br.Y, 0.0f, 1.0f ), cameraProjection );
                        //tr = Vector4.Transform( new Vector4( tr.X, tr.Y, 0.0f, 1.0f ), cameraProjection );

                        //float[] vertices = new float[]{
                        //    tl.X, tl.Y, 0.0f ,0.0f,
                        //    bl.X, bl.Y, 0.0f, 1.0f,
                        //    br.X, br.Y, 1.0f, 1.0f,
                        //    tl.X, tl.Y, 0.0f, 0.0f,
                        //    br.X, br.Y, 1.0f, 1.0f,
                        //    tr.X, tr.Y, 1.0f, 0.0f
                        //};

                        //fontProgram.Use();
                        ////fontProgram.SetUniformInt( "fontTexture", 0 );
                        ////font.texture?.Bind( 0 );

                        //GLUpdateVertexBuffer( fontBuffer, vertices );
                        //GLDrawVertexBuffer( fontBuffer );
                    }
                    break;
                }
            }
        }

        public static float WorldLengthToScreenLength( float worldLength ) {
            return ( worldLength / camera.width ) * viewport.Z;
        }

        public static Vector2 WorldPosToScreenPos( Vector2 world ) {
            // @NOTE: Convert to [ 0, 1 ] not NDC[ -1, 1 ] because 
            // @NOTE: we're doing a small optimization here by not doing the inverse of the camera matrix
            // @NOTE: but instead just using the camera width and height

            world -= camera.pos;

            float l = viewport.X;
            float r = viewport.X + viewport.Z;
            float nx = world.X / camera.width;
            float sx = l + nx * ( r - l );

            float b = viewport.Y;
            float t = viewport.Y + viewport.W;
            float ny = world.Y / camera.height;
            float sy = b + ny * ( t - b );

            sy = surfaceHeight - sy;

            return new Vector2( sx, sy );
        }

        public static Vector2 ScreenPosToWorldPos( Vector2 screenPos ) {
            screenPos.Y = surfaceHeight - screenPos.Y;

            // @NOTE: Convert to [ 0, 1] not NDC[-1, 1] because 
            // @NOTE: we're doing a small optimization here by not doing the inverse of the camera matrix
            // @NOTE: but instead just using the camera width and height

            float l = viewport.X;
            float r = viewport.X + viewport.Z;
            float nx = ( screenPos.X - l ) / ( r - l );

            float b = viewport.Y;
            float t = viewport.Y + viewport.W;
            float ny = ( screenPos.Y - b ) / ( t - b );

            float wx = nx * camera.width;
            float wy = ny * camera.height;

            return new Vector2( wx, wy );
        }

        public static bool Poll() {
            Array.Copy( input.keys, input.prevKeys, input.keys.Length );
            bool c = !Glfw.WindowShouldClose(window);
            if ( c == true ) {
                Glfw.PollEvents();

                for ( int i = 0x20; i < 348; i++ ) {
                    InputState state = Glfw.GetKey(window, (Keys)(i));
                    input.keys[i] = state != InputState.Release;
                }
            }
            return c;
        }

        public static void Close() {
            Glfw.SetWindowShouldClose( window, true );
        }

        private static void ResetSurface( float w, float h ) {
            float ratioX = w / camera.width;
            float ratioY = h / camera.height;
            float ratio = ratioX < ratioY ? ratioX : ratioY;

            int viewWidth = (int)( camera.width * ratio );
            int viewHeight = (int)( camera.height * ratio );

            int viewX = (int)( ( w - camera.width * ratio ) / 2 );
            int viewY = (int)( ( h - camera.height * ratio ) / 2 );

            glViewport( viewX, viewY, viewWidth, viewHeight );

            surfaceWidth = w;
            surfaceHeight = h;
            viewport = new Vector4( viewX, viewY, viewWidth, viewHeight );
            cameraProjection = Matrix4x4.CreateOrthographicOffCenter( 0.0f, camera.width, 0.0f, camera.height, -1.0f, 1.0f );
            screenProjection = Matrix4x4.CreateOrthographicOffCenter( 0.0f, w, h, 0.0f, -1.0f, 1.0f );
        }

        public static void GLEnableAlphaBlending() {
            glEnable( GL_BLEND );
            glBlendFunc( GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA );
        }

        public static void GLEnablePreMultipliedAlphaBlending() {
            glEnable( GL_BLEND );
            glBlendFunc( GL_ONE, GL_ONE_MINUS_SRC_ALPHA );
        }

        public delegate void CreateVAttribs( int stride );
        public static VertexBuffer GLCreateVertexBuffer( int stride, int size, bool dyanmic, CreateVAttribs createVAttribs ) {
            VertexBuffer buffer = new VertexBuffer();
            buffer.size = size;
            buffer.stride = stride;
            buffer.count = size / stride;
            buffer.dynamic = dyanmic;

            buffer.vao = glGenVertexArray();
            buffer.vbo = glGenBuffer();

            glBindVertexArray( buffer.vao );
            glBindBuffer( GL_ARRAY_BUFFER, buffer.vbo );

            unsafe {
                glBufferData( GL_ARRAY_BUFFER, size, NULL, dyanmic ? GL_DYNAMIC_DRAW : GL_STATIC_DRAW );
                createVAttribs.Invoke( stride );
            }

            return buffer;
        }

        public static VertexBuffer GLCreateVertexBuffer( float[] data, int stride, bool dyanmic ) {
            VertexBuffer buffer = new VertexBuffer();
            buffer.size = data.Length * sizeof( float );
            buffer.stride = stride;
            buffer.count = data.Length / stride;
            buffer.dynamic = dyanmic;

            buffer.vao = glGenVertexArray();
            buffer.vbo = glGenBuffer();

            glBindVertexArray( buffer.vao );
            glBindBuffer( GL_ARRAY_BUFFER, buffer.vbo );

            unsafe {
                fixed ( float* ptr = &data[0] ) {
                    glBufferData( GL_ARRAY_BUFFER, buffer.size, ptr, GL_STATIC_DRAW );
                    glVertexAttribPointer( 0, 2, GL_FLOAT, false, 4 * sizeof( float ), NULL );
                    glVertexAttribPointer( 1, 2, GL_FLOAT, false, 4 * sizeof( float ), (void*)( 2 * sizeof( float ) ) );
                }
            }

            glEnableVertexAttribArray( 0 );
            glEnableVertexAttribArray( 1 );

            return buffer;
        }

        public static void GLUpdateVertexBuffer( VertexBuffer vertexBuffer, float[] data ) {
            glBindBuffer( GL_ARRAY_BUFFER, vertexBuffer.vbo );
            unsafe {
                fixed ( float* ptr = &data[0] ) {
                    glBufferSubData( GL_ARRAY_BUFFER, 0, vertexBuffer.size, ptr );
                }
            }
            glBindBuffer( GL_ARRAY_BUFFER, 0 );
        }

        public static void GLDrawVertexBuffer( VertexBuffer vertexBuffer ) {
            glBindVertexArray( vertexBuffer.vao );
            glDrawArrays( GL_TRIANGLES, 0, vertexBuffer.count );
            glBindVertexArray( 0 );
        }

        public static void InitShapeRendering() {
            shapeProgram = new ShaderProgram( @"
                #version 330 core
                layout (location = 0) in vec2 pos;
                void main()
                {
                    gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
                }",

               @"
                #version 330 core
                out vec4 FragColor;

                uniform int  mode;
                uniform vec4 color;
                uniform vec4 shapePosAndSize;
                uniform vec4 shapeRadius;

                // from http://www.iquilezles.org/www/articles/distfunctions/distfunctions

                float CircleSDF(vec2 r, vec2 p, float rad) {
                    return 1 - max(length(p - r) - rad, 0);
                }

                float BoxSDF(vec2 r, vec2 p, vec2 s) {
                    return 1 - length(max(abs(p - r) - s, 0));
                }
            
                float RoundedBoxSDF(vec2 r, vec2 p, vec2 s, float rad) {
                    return 1 - (length(max(abs(p - r) - s + rad, 0)) - rad);
                }

                void main() {
                    vec2 s = shapePosAndSize.zw;
                    vec2 r = shapePosAndSize.xy;
                    vec2 p = gl_FragCoord.xy;

                    if (mode == 0) {
                        FragColor = color;
                    } else if (mode == 1) {
                        float d = CircleSDF(r, p, shapeRadius.x);
                        d = clamp(d, 0.0, 1.0);
                        FragColor = color * d; //vec4(color.xyz, color.w * d);
                        //FragColor = color;
                    } else if (mode == 2) {
                        float d = RoundedBoxSDF(r, p, s / 2, shapeRadius.x);
                        d = clamp(d, 0.0, 1.0);
                        FragColor = vec4(color.xyz, color.w * d);
                    } else {
                        FragColor = vec4(1, 0, 1, 1);
                    }
                }
                "
            );

            shapeBuffer = GLCreateVertexBuffer( 2 * sizeof( float ), 6 * 2 * sizeof( float ), true, stride => {
                unsafe {
                    glEnableVertexAttribArray( 0 );
                    glVertexAttribPointer( 0, 2, GL_FLOAT, false, stride, NULL );
                }
            } );
        }

        public static void InitSpriteRendering() {
            spriteProgram = new ShaderProgram( @"
                #version 330 core
                layout (location = 0) in vec2 pos;
                layout (location = 1) in vec2 uv;
                out vec2 texCoord;
                void main()
                {
                    texCoord = uv;
                    gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
                }",

                @"
                #version 330 core
                out vec4 result;
                in vec2 texCoord;
                uniform sampler2D texture1;
                void main()
                {
                    result = texture(texture1, texCoord);
                    //result = vec4(texCoord.x, texCoord.y, 0.0, 1.0);
                    //result = vec4(1,1,1, 1.0);
                } "
            );

            spriteBuffer = GLCreateVertexBuffer( 4 * sizeof( float ), 6 * 4 * sizeof( float ), true, stride => {
                unsafe {
                    glEnableVertexAttribArray( 0 );
                    glVertexAttribPointer( 0, 2, GL_FLOAT, false, stride, NULL );
                    glEnableVertexAttribArray( 1 );
                    glVertexAttribPointer( 1, 2, GL_FLOAT, false, stride, (void*)( 2 * sizeof( float ) ) );
                }
            } );
        }

        public static void InitFontRendering() {
            fontProgram = new ShaderProgram( @"
                #version 330 core
                layout (location = 0) in vec2 pos;
                layout (location = 1) in vec2 uv;
                out vec2 texCoord;
                void main()
                {
                    texCoord = uv;
                    gl_Position = vec4(pos.x, pos.y, 0.0, 1.0);
                }",

                @"
                #version 330 core
                out vec4 result;
                in vec2 texCoord;
                uniform sampler2D fontTexture;
                void main()
                {
                    float a = texture(fontTexture, texCoord).r;
                    result = vec4(1,1,1,a);
                    //result = vec4(texCoord.x, texCoord.y, 0.0, 1.0);
                    //result = vec4(1,1,1, 1.0);
                } "
            );

            fontBuffer = GLCreateVertexBuffer( 4 * sizeof( float ), 6 * 4 * sizeof( float ), true, stride => {
                unsafe {
                    glEnableVertexAttribArray( 0 );
                    glVertexAttribPointer( 0, 2, GL_FLOAT, false, stride, NULL );
                    glEnableVertexAttribArray( 1 );
                    glVertexAttribPointer( 1, 2, GL_FLOAT, false, stride, (void*)( 2 * sizeof( float ) ) );
                }
            } );
        }

        public static void OnFrameBufferSizeCallback( int w, int h ) {
            ResetSurface( w, h );
        }

        public static void OnKeyCallback( Keys key, int scanCode, InputState state, ModifierKeys mods ) {

        }

        public static bool KeyIsJustDown( InputKey key ) {
            return input.keys[(int)key] && !input.prevKeys[(int)key];
        }

        public static bool KeyIsDown( InputKey key ) {
            return input.keys[(int)key];
        }

    }
}