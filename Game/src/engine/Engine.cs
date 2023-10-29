using FontStashSharp;
using Game;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using SoLoud;

namespace Game
{

    public class Engine : GameWindow {
        private static Engine self = null;
        private static float surfaceWidth = 0;
        private static float surfaceHeight = 0;
        private static Vector4 viewport = new Vector4( 0, 0, 0, 0 );
        public static Camera camera = new Camera();
        public static Matrix4 screenProjection;
        public static Matrix4 cameraProjection;
        private static ShaderProgram shapeProgram = null;
        private static VertexBuffer  shapeBuffer = null;
        private static ShaderProgram spriteProgram = null;
        private static VertexBuffer  spriteBuffer = null;
        private static FontRenderer fontRenderer = null;
        private static SoLoud.Soloud audioEngine;
        private static GameClient gameClient = null;
        private static byte[] gameClientPacketData = new byte[ 2048 ]; // TODO: Pre-allocate a buffer for receiving packets
        private static GameMode gameMode = null;
        private static GameMode nextGameMode = null;


        private static bool shouldClose = false;

        public Engine( string title ) :
            base( GameWindowSettings.Default, new NativeWindowSettings() { 
                Size = new Vector2i( GameSettings.Current.WindowWidth, GameSettings.Current.WindowHeight ),
                Title = title,
                Location = new Vector2i( GameSettings.Current.WindowPosX, GameSettings.Current.WindowPosY ),
                Vsync = OpenTK.Windowing.Common.VSyncMode.On
            } ) {
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

            audioEngine = new SoLoud.Soloud();
            audioEngine.Init( SoLoud.Soloud.CLIP_ROUNDOFF );
            audioEngine.SetGlobalVolume( 4 );

            gameClient = new GameClient();

            gameMode = new GameModeMainMenu();
            gameMode.Init();
        }

        protected override void OnUpdateFrame( OpenTK.Windowing.Common.FrameEventArgs args ) {
            base.OnUpdateFrame( args );

            if ( IsKeyDown( OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape ) || shouldClose ) {
                Close();
            }

            if ( gameClient.NetworkIsConnected() ) {
                int packetSize = 0;
                Array.Clear( gameClientPacketData, 0, gameClientPacketData.Length );
                while ( gameClient.NetworkPoll( gameClientPacketData, out packetSize ) == true ) {

                }
            }

            if ( nextGameMode != null ) {
                gameMode.Shutdown();
                gameMode = nextGameMode;
                gameMode.Init();
                nextGameMode = null;
                return; // skip update tick for this frame
            }

            gameMode.UpdateTick( (float)args.Time );
        }

        protected override void OnRenderFrame( OpenTK.Windowing.Common.FrameEventArgs args ) {
            base.OnRenderFrame( args );
            //if ( !IsFocused ) {
            //    return;
            //}

            gameMode.UpdateRender( (float)args.Time );
            SwapBuffers();
        }

        protected override void OnResize( OpenTK.Windowing.Common.ResizeEventArgs e ) {
            int w = e.Width;
            int h = e.Height;
            ResetSurface( w, h );
        }

        public static void SubmitDrawCommands( DrawCommands commands ) {
            GL.ClearColor( Colors.SILVER.X, Colors.SILVER.Y, Colors.SILVER.Z, 1.0f );
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
                        DynamicSpriteFont font = Content.GetDefaultFont();
                        System.Numerics.Vector2 c = new System.Numerics.Vector2( cmd.c.X, cmd.c.Y );
                        if ( cmd.center ) {
                            System.Numerics.Vector2 d = font.MeasureString( cmd.text );
                            c = new System.Numerics.Vector2( cmd.c.X, cmd.c.Y );
                            c -= new System.Numerics.Vector2( d.X / 2.0f, d.Y / 2.0f );
                        }

                        font.DrawText( fontRenderer, cmd.text, c, FSColor.White );
                    }
                    break;
                    case DrawCommandType.TRIANGLES: {
                        for ( int i = 0; i < cmd.verts.Length; i++ ) {
                            cmd.verts[i] -= camera.pos;
                            Vector4 r = new Vector4( cmd.verts[i].X, cmd.verts[i].Y, 0.0f, 1.0f ) * cameraProjection;
                            cmd.verts[i] = new Vector2( r.X, r.Y );
                        }

                        GLEnableAlphaBlending();

                        shapeProgram.Bind();
                        shapeProgram.SetUniformInt( "mode", 0 );
                        shapeProgram.SetUniformVec4( "color", cmd.color );
                        shapeBuffer.UpdateVertexBuffer( cmd.verts );
                        shapeBuffer.DrawVertexBuffer();
                    }
                    break;

                    case DrawCommandType.SCREEN_RECT: {
                        Vector4 tl =  new Vector4( cmd.tl.X, cmd.tl.Y, 0.0f, 0.0f );
                        Vector4 bl =  new Vector4( cmd.bl.X, cmd.bl.Y, 0.0f, 0.0f );
                        Vector4 br =  new Vector4( cmd.br.X, cmd.br.Y, 0.0f, 0.0f );
                        Vector4 tr =  new Vector4( cmd.tr.X, cmd.tr.Y, 0.0f, 0.0f );

                        tl = new Vector4( tl.X, tl.Y, 0.0f, 1.0f ) * screenProjection;
                        bl = new Vector4( bl.X, bl.Y, 0.0f, 1.0f ) * screenProjection;
                        br = new Vector4( br.X, br.Y, 0.0f, 1.0f ) * screenProjection;
                        tr = new Vector4( tr.X, tr.Y, 0.0f, 1.0f ) * screenProjection;

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

        public static void AudioPlay( SoloudObject wav ) {
            audioEngine.Play( wav );
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
                out vec4 FragColor;
                in vec2 texCoord;
                uniform sampler2D texture1;

                vec2 uv_cstantos( vec2 uv, vec2 res ) {
                    vec2 pixels = uv * res;

                    // Updated to the final article
                    vec2 alpha = 0.7 * fwidth(pixels);
                    vec2 pixels_fract = fract(pixels);
                    vec2 pixels_diff = clamp( .5 / alpha * pixels_fract, 0, .5 ) +
                                        clamp( .5 / alpha * (pixels_fract - 1) + .5, 0, .5 );
                    pixels = floor(pixels) + pixels_diff;
                    return pixels / res;
                }

                vec2 uv_klems( vec2 uv, ivec2 texture_size ) {
            
                    vec2 pixels = uv * texture_size + 0.5;
    
                    // tweak fractional value of the texture coordinate
                    vec2 fl = floor(pixels);
                    vec2 fr = fract(pixels);
                    vec2 aa = fwidth(pixels) * 0.75;

                    fr = smoothstep( vec2(0.5) - aa, vec2(0.5) + aa, fr);
    
                    return (fl + fr - 0.5) / texture_size;
                }

                vec2 uv_iq( vec2 uv, ivec2 texture_size ) {
                    vec2 pixel = uv * texture_size;

                    vec2 seam = floor(pixel + 0.5);
                    vec2 dudv = fwidth(pixel);
                    pixel = seam + clamp( (pixel - seam) / dudv, -0.5, 0.5);
    
                    return pixel / texture_size;
                }

                vec2 uv_fat_pixel( vec2 uv, ivec2 texture_size ) {
                    vec2 pixel = uv * texture_size;

                    vec2 fat_pixel = floor(pixel) + 0.5;
                    // subpixel aa algorithm (COMMENT OUT TO COMPARE WITH POINT SAMPLING)
                    fat_pixel += 1 - clamp((1.0 - fract(pixel)) * 3, 0, 1);
        
                    return fat_pixel / texture_size;
                }

                vec2 uv_aa_smoothstep( vec2 uv, vec2 res, float width ) {
                    uv = uv * res;
                    vec2 uv_floor = floor(uv + 0.5);
                    vec2 uv_fract = fract(uv + 0.5);
                    vec2 uv_aa = fwidth(uv) * width * 0.5;
                    uv_fract = smoothstep(
                        vec2(0.5) - uv_aa,
                        vec2(0.5) + uv_aa,
                        uv_fract
                        );
    
                    return (uv_floor + uv_fract - 0.5) / res;
                }


                void main() {
                    ivec2 tSize = textureSize(texture1, 0);
                    //vec2 uv = uv_cstantos(texCoord, vec2(tSize.x, tSize.y));
                    //vec2 uv = uv_iq(texCoord, tSize);
                    vec2 uv = uv_fat_pixel(texCoord, tSize);
                    //vec2 uv = uv_aa_smoothstep(texCoord, vec2(tSize.x, tSize.y), 1);
                    vec4 sampled = texture(texture1, uv);
                    //vec4 sampled = texture(texture1, texCoord);
                    //sampled.rgb *= sampled.a;
                    //if (sampled.a < 1) discard;
                    FragColor = sampled;
                }
                "
            );

            spriteBuffer = new VertexBuffer( 4 * sizeof( float ), 6 * 4 * sizeof( float ), true, stride => {
                GL.EnableVertexAttribArray( 0 );
                GL.VertexAttribPointer( 0, 2, VertexAttribPointerType.Float, false, stride, 0 );

                GL.EnableVertexAttribArray( 1 );
                GL.VertexAttribPointer( 1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof( float ) );
            } );
        }

        public static bool KeyIsJustDown( InputKey key ) {
            return self.KeyboardState.IsKeyPressed( (OpenTK.Windowing.GraphicsLibraryFramework.Keys)key );
        }

        public static bool KeyIsDown( InputKey key ) {
            return self.KeyboardState.IsKeyDown( (OpenTK.Windowing.GraphicsLibraryFramework.Keys)key );
        }

        public static new bool MouseDown( int mouseButton ) {
            return self.MouseState.IsButtonDown( (OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)( mouseButton - 1 ) );
        }

        public static bool MouseJustDown( int mouseButton ) {
            return self.MouseState.IsButtonPressed( (OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)( mouseButton - 1 ) );
        }

        public static bool MouseJustUp( int mouseButton ) {
            return self.MouseState.IsButtonReleased( (OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)( mouseButton - 1 ) );
        }

        public static Vector2 MouseScreenPos() {
            return new Vector2( self.MouseState.X, self.MouseState.Y );
        }

        public static float MouseScrollDelta() {
            return self.MouseState.ScrollDelta.Y;
        }
        
        public static Vector2 GetSurfaceSize() {
            return new Vector2( surfaceWidth, surfaceHeight );
        }

        public static void QuitGame() {
            shouldClose = true;
        }

        public static void MoveToGameMode( GameMode gameMode ) {
            nextGameMode = gameMode;
        }

        public static bool NetworkConnectToServer() {
            return gameClient.ConnectToServer( GameSettings.Current.ServerIp, GameSettings.Current.ServerPort );
        }

        public static void NetworkDisconnectFromServer() {
            gameClient.DisconnectFromServer();
        }

        public static void NetworkSendPacket( byte[] packet, bool reliable ) {
            gameClient.NetworkSendPacket( packet, reliable );
        }
    }
}
