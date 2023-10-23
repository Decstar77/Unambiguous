using FontStashSharp;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace Game {

    public struct Camera {
        public float    minWidth;
        public float    minHeight;

        public float    maxWidth;
        public float    maxHeight;

        public float    width;
        public float    height;
        public float    zoom;

        public Vector2  pos;
    }

    public class Engine : GameWindow {
        public static Engine self = null;
        public static EngineInput input = new EngineInput();
        public static float surfaceWidth = 0;
        public static float surfaceHeight = 0;
        public static Vector4 viewport = new Vector4( 0, 0, 0, 0 );
        public static Camera camera = new Camera();
        public static Matrix4 screenProjection;
        public static Matrix4 cameraProjection;
        public static ShaderProgram shapeProgram = null;
        public static VertexBuffer  shapeBuffer = null;
        public static ShaderProgram spriteProgram = null;
        public static VertexBuffer  spriteBuffer = null;
        public static FontRenderer fontRenderer = null;
        public static SoLoud.Soloud soloud;
        public static GameCode gameCode = null;

        public Engine( int width, int height, string title ) :
            base( GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title } ) {
            self = this;
        }

        protected override void OnLoad() {
            base.OnLoad();

            surfaceWidth = (float)ClientSize.X;
            surfaceHeight = (float)ClientSize.Y;

            camera.minWidth = 240;
            camera.minHeight = 135;
            camera.maxWidth = 480;
            camera.maxHeight = 270;
            camera.width = camera.minWidth;
            camera.height = camera.minHeight;
            camera.zoom = 0.0f;
            ResetSurface( surfaceWidth, surfaceHeight );

            InitShapeRendering();
            InitSpriteRendering();

            Content.LoadFonts();
            fontRenderer = new FontRenderer();

            //soloud = new SoLoud.Soloud();
            //soloud.Init( SoLoud.Soloud.CLIP_ROUNDOFF );
            //soloud.SetGlobalVolume( 4 );
            //SoLoud.Wav wav = new SoLoud.Wav();
            //wav.Load( "C:/Projects/CS/Mage/Content/basic_death_1.wav" );
            //soloud.Play( wav );

            gameCode = new GameCode();
            gameCode.Init();
        }

        protected override void OnUpdateFrame( OpenTK.Windowing.Common.FrameEventArgs args ) {
            base.OnUpdateFrame( args );
            gameCode.UpdateTick( (float)args.Time );
        }

        protected override void OnRenderFrame( OpenTK.Windowing.Common.FrameEventArgs args ) {
            base.OnRenderFrame( args );
            gameCode.UpdateRender( (float)args.Time );
            SwapBuffers();
        }

        public static void SubmitDrawCommands( DrawCommands commands ) {
            GL.ClearColor( 0.2f, 0.3f, 0.3f, 1.0f );
            GL.Clear( ClearBufferMask.ColorBufferBit );

            foreach ( DrawCommand cmd in commands.commands ) {
                switch ( cmd.type ) {
                    case DrawCommandType.CIRCLE: {
                        float x1 = cmd.c.X - cmd.r - camera.pos.X;
                        float y1 = cmd.c.Y - cmd.r - camera.pos.Y;
                        float x2 = cmd.c.X + cmd.r - camera.pos.X;
                        float y2 = cmd.c.Y + cmd.r - camera.pos.Y;

                        Vector4 tl = new Vector4( x1, y2, 0.0f, 1.0f ) * cameraProjection ;
                        Vector4 bl = new Vector4( x1, y1, 0.0f, 1.0f ) * cameraProjection ;
                        Vector4 br = new Vector4( x2, y1, 0.0f, 1.0f ) * cameraProjection ;
                        Vector4 tr = new Vector4( x2, y2, 0.0f, 1.0f ) * cameraProjection ;

                        float[] vertices = new float[]{
                            tl.X, tl.Y,
                            bl.X, bl.Y,
                            br.X, br.Y,
                            tl.X, tl.Y,
                            br.X, br.Y,
                            tr.X, tr.Y
                        };

                        Vector2 c = WorldPosToScreenPos( cmd.c - camera.pos );
                        float r = WorldLengthToScreenLength( cmd.r ) - 2.0f;

                        GLEnableAlphaBlending();

                        shapeProgram.Bind();
                        shapeProgram.SetUniformInt( "mode", 1 );
                        shapeProgram.SetUniformVec4( "color", cmd.color );
                        shapeProgram.SetUniformVec4( "shapePosAndSize", new Vector4( c.X, viewport.W - c.Y, 0, 0 ) );
                        shapeProgram.SetUniformVec4( "shapeRadius", new Vector4( r, 0, 0, 0 ) );
                        shapeBuffer.UpdateVertexBuffer( vertices );
                        shapeBuffer.DrawVertexBuffer();
                    }
                    break;
                    case DrawCommandType.RECT: {
                        Vector4 tl =  new Vector4( cmd.tl.X - camera.pos.X, cmd.tl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 bl =  new Vector4( cmd.bl.X - camera.pos.X, cmd.bl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 br =  new Vector4( cmd.br.X - camera.pos.X, cmd.br.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 tr =  new Vector4( cmd.tr.X - camera.pos.X, cmd.tr.Y - camera.pos.Y, 0.0f, 0.0f );

                        tl = new Vector4( tl.X, tl.Y, 0.0f, 1.0f ) * cameraProjection;
                        bl = new Vector4( bl.X, bl.Y, 0.0f, 1.0f ) * cameraProjection;
                        br = new Vector4( br.X, br.Y, 0.0f, 1.0f ) * cameraProjection;
                        tr = new Vector4( tr.X, tr.Y, 0.0f, 1.0f ) * cameraProjection;

                        float[] vertices = new float[]{
                            tl.X, tl.Y,
                            bl.X, bl.Y,
                            br.X, br.Y,
                            tl.X, tl.Y,
                            br.X, br.Y,
                            tr.X, tr.Y
                        };

                        GLEnableAlphaBlending();

                        shapeProgram.Bind();
                        shapeProgram.SetUniformInt( "mode", 0 );
                        shapeProgram.SetUniformVec4( "color", cmd.color );
                        shapeBuffer.UpdateVertexBuffer( vertices );
                        shapeBuffer.DrawVertexBuffer();
                    }
                    break;
                    case DrawCommandType.SPRITE: {
                        Vector4 tl =  new Vector4( cmd.tl.X - camera.pos.X, cmd.tl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 bl =  new Vector4( cmd.bl.X - camera.pos.X, cmd.bl.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 br =  new Vector4( cmd.br.X - camera.pos.X, cmd.br.Y - camera.pos.Y, 0.0f, 0.0f );
                        Vector4 tr =  new Vector4( cmd.tr.X - camera.pos.X, cmd.tr.Y - camera.pos.Y, 0.0f, 0.0f );

                        tl = new Vector4( tl.X, tl.Y, 0.0f, 1.0f ) * cameraProjection;
                        bl = new Vector4( bl.X, bl.Y, 0.0f, 1.0f ) * cameraProjection;
                        br = new Vector4( br.X, br.Y, 0.0f, 1.0f ) * cameraProjection;
                        tr = new Vector4( tr.X, tr.Y, 0.0f, 1.0f ) * cameraProjection;

                        float[] vertices = new float[]{
                            tl.X, tl.Y, 0.0f ,0.0f,
                            bl.X, bl.Y, 0.0f, 1.0f,
                            br.X, br.Y, 1.0f, 1.0f,
                            tl.X, tl.Y, 0.0f, 0.0f,
                            br.X, br.Y, 1.0f, 1.0f,
                            tr.X, tr.Y, 1.0f, 0.0f
                        };

                        GLEnablePreMultipliedAlphaBlending();

                        spriteProgram.Bind();
                        spriteProgram.SetUniformTexture( "texture1", 0, cmd.spriteTexture.texture );
                        spriteBuffer.UpdateVertexBuffer( vertices );
                        spriteBuffer.DrawVertexBuffer();
                    }
                    break;
                    case DrawCommandType.TEXT: {
                        DynamicSpriteFont font = Content.GetFont();
                        System.Numerics.Vector2 c = new System.Numerics.Vector2( cmd.c.X, cmd.c.Y );
                        font.DrawText( fontRenderer, cmd.text, c, FSColor.White );
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

        public static void CameraSetZoomCenter( float zoom ) {
            zoom = Math.Clamp( zoom, 0.0f, 1.0f );

            Vector2 centerPoint1 = ScreenPosToWorldPos( new Vector2( surfaceWidth / 2.0f, surfaceHeight / 2.0f ) );

            camera.zoom = zoom;
            camera.width = camera.minWidth + ( camera.maxWidth - camera.minWidth ) * camera.zoom;
            camera.height = camera.minHeight + ( camera.maxHeight - camera.minHeight ) * camera.zoom;
            ResetSurface( surfaceWidth, surfaceHeight );

            Vector2 centerPoint2 = ScreenPosToWorldPos( new Vector2( surfaceWidth / 2.0f, surfaceHeight / 2.0f ) );

            camera.pos = camera.pos + ( centerPoint1 - centerPoint2 );
        }

        public static void CameraSetZoomPoint( float zoom, Vector2 screenPoint ) {
            zoom = Math.Clamp( zoom, 0.0f, 1.0f );

            Vector2 centerPoint1 = ScreenPosToWorldPos( screenPoint );

            camera.zoom = zoom;
            camera.width = camera.minWidth + ( camera.maxWidth - camera.minWidth ) * camera.zoom;
            camera.height = camera.minHeight + ( camera.maxHeight - camera.minHeight ) * camera.zoom;
            ResetSurface( surfaceWidth, surfaceHeight );

            Vector2 centerPoint2 = ScreenPosToWorldPos( screenPoint );

            camera.pos = camera.pos + ( centerPoint1 - centerPoint2 );
        }


        private static void ResetSurface( float w, float h ) {
            float ratioX = w / camera.width;
            float ratioY = h / camera.height;
            float ratio = ratioX < ratioY ? ratioX : ratioY;

            int viewWidth = (int)( camera.width * ratio );
            int viewHeight = (int)( camera.height * ratio );

            int viewX = (int)( ( w - camera.width * ratio ) / 2 );
            int viewY = (int)( ( h - camera.height * ratio ) / 2 );

            GL.Viewport( viewX, viewY, viewWidth, viewHeight );

            surfaceWidth = w;
            surfaceHeight = h;
            viewport = new Vector4( viewX, viewY, viewWidth, viewHeight );
            cameraProjection = Matrix4.CreateOrthographicOffCenter( 0.0f, camera.width, 0.0f, camera.height, -1.0f, 1.0f );
            screenProjection = Matrix4.CreateOrthographicOffCenter( 0.0f, w, h, 0.0f, -1.0f, 1.0f );
        }

        public static void GLEnableAlphaBlending() {
            GL.Enable( EnableCap.Blend );
            GL.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        }

        public static void GLEnablePreMultipliedAlphaBlending() {
            GL.Enable( EnableCap.Blend );
            GL.BlendFunc( BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha );
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

            shapeBuffer = new VertexBuffer( 2 * sizeof( float ), 6 * 2 * sizeof( float ), true, stride => {
                GL.EnableVertexAttribArray( 0 );
                GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, stride, 0 );
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

            spriteBuffer = new VertexBuffer( 4 * sizeof( float ), 6 * 4 * sizeof( float ), true, stride => {
                GL.EnableVertexAttribArray( 0 );
                GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, stride, 0 );

                GL.EnableVertexAttribArray( 1 );
                GL.VertexAttribPointer( 1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof( float ) );
            } );
        }

        public static void OnFrameBufferSizeCallback( int w, int h ) {
            ResetSurface( w, h );
        }

        public static void OnCursorPosCallback( double x, double y ) {
            input.mousePos.X = (float)x;
            input.mousePos.Y = (float)y;
        }

        public static void OnScrollCallback( double x, double y ) {
            input.scrollX = (float)x;
            input.scrollY = (float)y;
        }

        public static bool KeyIsJustDown( InputKey key ) {
            return self.KeyboardState.IsKeyPressed( (OpenTK.Windowing.GraphicsLibraryFramework.Keys)key );
        }

        public static bool KeyIsDown( InputKey key ) {
            return self.KeyboardState.IsKeyDown( (OpenTK.Windowing.GraphicsLibraryFramework.Keys)key );
        }


    }
}
